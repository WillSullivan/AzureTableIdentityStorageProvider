using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace StateStreetGang.AspNet.Identity.AzureTable
{
    public partial class AzureTableUserStore<T> :
        IUserClaimStore<T>
    {
        #region consts
        /// <summary>
        /// The key in <see cref="Claim.Properties"/> that stores the <see cref="Microsoft.WindowsAzure.Storage.Table.TableEntity.PartitionKey"/> of a <see cref="Claim">Claim's</see> storage model.
        /// </summary>
        public const string ClaimsPartitionKeyPropertyKey = "ClaimsPartitionKey";
        /// <summary>
        /// The key in <see cref="Claim.Properties"/> that stores the <see cref="Microsoft.WindowsAzure.Storage.Table.TableEntity.RowKey"/> of a <see cref="Claim">Claim's</see> storage model.
        /// </summary>
        public const string ClaimsRowKeyPropertyKey = "ClaimsRowKey";
        #endregion

        #region privates
        // Goddamnit, framework.
        private Func<T, Claim, AzureTableClaim> _defaultMapToStoreDomainFunc;
        // storing the partition/row keys, but I never assume they exist, so it's pretty much worthless to me
        private Func<AzureTableClaim, Claim> _defaultMapToApplicationDomainFunc = ad =>
        {
            var retval = new Claim(ad.Type, ad.Value, ad.ValueType, ad.Issuer, ad.OriginalIssuer);
            retval.Properties[ClaimsPartitionKeyPropertyKey] = ad.PartitionKey;
            retval.Properties[ClaimsRowKeyPropertyKey] = ad.RowKey;
            return retval;
        };
        // The partition is scoped to the user.  Not sure if this is a good idea.
        private Func<T, string> _defaultGetClaimsPartitionKey = atu => atu.RowKey + "Claims";
        // Not even sure if issuer, type and value are adequate to identify a user's claim
        private Func<T, Claim, string> _defaultGetClaimsRowKey = (atu, claim) => atu.RowKey + (claim.Issuer.GetHashCode() ^ claim.Type.GetHashCode() ^ claim.Value.GetHashCode()).ToString(CultureInfo.InvariantCulture);
        #endregion

        #region props
        /// <summary>
        /// The default table name used for the user role store.
        /// </summary>
        public const string DefaultUserClaimTableName = "AspNetIdentityUserClaimStore";

        /// <summary>
        /// Gets the name of the Azure table that stores the user claim information.
        /// </summary>
        /// <value></value>
        protected virtual string UserClaimTableName
        {
            get { return DefaultUserClaimTableName; }
        }
        /// <summary>
        /// Maps an A <see cref="AzureTableUser"/>-derived type and a specific <see cref="Claim"/> to a row key for storing the specified <see cref="Claim"/> for the given user.
        /// </summary>
        protected virtual Func<T, Claim, string> MapToClaimsRowKey
        {
            get
            {
                return _defaultGetClaimsRowKey;
            }
        }

        /// <summary>
        /// Maps an A <see cref="AzureTableUser"/>-derived type to a partition key for claims data.
        /// </summary>
        protected virtual Func<T, string> MapUserToClaimsPartitionKey
        {
            get
            {
                return _defaultGetClaimsPartitionKey;
            }
        }

        /// <summary>
        /// Maps a <see cref="Claim"/> to an <see cref="AzureTableClaim"/>, which can be stored in an Azure table.
        /// </summary>
        protected virtual Func<T, Claim, AzureTableClaim> MapClaimsToStoreDomain
        {
            get
            {
                return _defaultMapToStoreDomainFunc ??
                    (_defaultMapToStoreDomainFunc = (atu, ad) =>
                    {
                        var part = (MapUserToClaimsPartitionKey ?? _defaultGetClaimsPartitionKey)(atu);
                        var key = (MapToClaimsRowKey ?? _defaultGetClaimsRowKey)(atu, ad);
                        return new AzureTableClaim
                        {
                            Issuer = ad.Issuer,
                            OriginalIssuer = ad.OriginalIssuer,
                            Type = ad.Type,
                            Value = ad.Value,
                            ValueType = ad.ValueType,
                            RowKey = key,
                            PartitionKey = part
                        };
                    });
            }
        }

        /// <summary>
        /// Maps an <see cref="AzureTableClaim"/> to a <see cref="Claim"/>.
        /// </summary>
        protected virtual Func<AzureTableClaim, Claim> MapClaimsToApplicationDomain
        {
            get
            {
                return _defaultMapToApplicationDomainFunc;
            }
        }
        #endregion

        #region IUserClaimStore<T>
        /// <summary>
        /// Removes the <paramref name="claim"/> from the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">A <see cref="AzureTableUser"/>-derived type</param>
        /// <param name="claim"><see cref="Claim"/></param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> or <paramref name="claim"/> is <c>null</c>.</exception>
        /// <see cref="AzureTableUserException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        public async virtual Task RemoveClaimAsync(T user, Claim claim)
        {
            AssertNotDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (claim == null)
                throw new ArgumentNullException("claim");
            var conv = MapClaimsToStoreDomain(user, claim);
            conv.EnsureETagSet();
            try
            {
                await Run(UserClaimTableName, TableOperation.Delete(conv));
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    return;
                throw new AzureTableUserException("An exception was thrown while attempting to remove this claim.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Adds the <paramref name="claim"/> to the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">A <see cref="AzureTableUser"/>-derived type</param>
        /// <param name="claim"><see cref="Claim"/></param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> or <paramref name="claim"/> is <c>null</c>.</exception>
        /// <see cref="AzureTableUserException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        public async virtual Task AddClaimAsync(T user, Claim claim)
        {
            AssertNotDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (claim == null)
                throw new ArgumentNullException("claim");
            var dclaim = MapClaimsToStoreDomain(user, claim);
            dclaim.EnsureETagSet();
            try
            {
                await Run(UserClaimTableName, TableOperation.Insert(dclaim));
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
                    throw new AzureTableUserException("The claim already exists", ex);
                throw new AzureTableUserException("An exception was thrown while attempting to add this claim.  See the inner exception for details.", ex);
            }
        }
        ///<summary>
        /// Gets all <see cref="Claim">claims</see> for the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">A <see cref="AzureTableUser"/>-derived type</param>
        /// <returns><see cref="Task{T}"/> that returns an <see cref="IList{T}"/> containing the <paramref name="user">user's</paramref> <see cref="Claim">Claims</see>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
        /// <see cref="AzureTableUserException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        public async virtual Task<IList<Claim>> GetClaimsAsync(T user)
        {
            AssertNotDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            var table = await GetTable(UserClaimTableName);
            var query = new TableQuery<AzureTableClaim>().Where(
                    TableQuery.GenerateFilterCondition(PropertyNames.PartitionKey, QueryComparisons.Equal, MapUserToClaimsPartitionKey(user)));
            try
            {
                return table.ExecuteQuery<AzureTableClaim>(query).Select(x => MapClaimsToApplicationDomain(x)).ToList();
            }
            catch (StorageException ex)
            {
                throw new AzureTableUserException("An exception was thrown while attempting to retrieve these claims.  See the inner exception for details.", ex);
            }
        }
        #endregion
    }
}
