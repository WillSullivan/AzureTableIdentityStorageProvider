using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace StateStreetGang.AspNet.Identity.AzureTable
{
    public partial class AzureTableUserStore<T> : IUserLoginStore<T>
    {
        #region props
        /// <summary>
        /// Gets the <see cref="TableEntity.PartitionKey"/> used for <see cref="AzureTableUserLogin"/> instances.
        /// </summary>
        protected virtual string AzureTableUserLoginPartitionKey
        {
            get { return AzureTableUserLogin.DefaultPartitionKey; }
        }

        #endregion

        #region IUserLoginStore<T> Members

        /// <summary>
        /// Adds a new <paramref name="user"/> to the store asynchronously.
        /// </summary>
        /// <param name="user">A A <see cref="AzureTableUser"/>-derived type-derived type</param>
        /// <param name="login"><see cref="UserLoginInfo"/></param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> or <paramref name="login"/> is <c>null</c>.</exception>
        /// <see cref="AzureTableUserException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        public virtual async Task AddLoginAsync(T user, UserLoginInfo login)
        {
            AssertNotDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (login == null)
                throw new ArgumentNullException("login");
            var atul = new AzureTableUserLogin
            {
                LoginProvider = login.LoginProvider,
                ProviderKey= login.ProviderKey,
                UserId = user.Id,
            };

            try
            {
                await Run(TableOperation.InsertOrReplace(atul));
            }
            catch (StorageException ex)
            {
                //TODO:  Not sure if I should throw or silently swallow this exception            
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict) throw new AzureTableRoleException("The login info already exists", ex);
                throw new AzureTableUserException("An exception was thrown while attempting to add this login info.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Finds a user by their login.
        /// </summary>
        /// <param name="login"><see cref="UserLoginInfo"/></param>
        /// <returns>A <see cref="Task{T}"/> that returns the A A <see cref="AzureTableUser"/>-derived type-derived type if found, or <c>null</c> otherwise.</returns>
        /// <see cref="AzureTableUserException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="login"/> is <c>null</c>.</exception>
        public virtual async Task<T> FindAsync(UserLoginInfo login)
        {
            AssertNotDisposed();
            if (login == null)
                throw new ArgumentNullException("login");
            var table = await GetTable();
            var query = TableOperation.Retrieve<AzureTableUserLogin>(AzureTableUserLoginPartitionKey, login.ProviderKey);
            try
            {
                var temp = await table.ExecuteAsync(query);
                var ul = temp.Result as AzureTableUserLogin;
                if (ul == null)
                    return null;
                return await this.FindByIdAsync(ul.UserId);
            }
            catch (StorageException ex)
            {
                throw new AzureTableUserException("An exception was thrown while attempting to find the login.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Finds all <see cref="UserLoginInfo">UserLoginInfos</see> associated with the given <paramref name="user"/>
        /// </summary>
        /// <param name="user">A A <see cref="AzureTableUser"/>-derived type-derived type</param>
        /// <returns>A <see cref="Task{T}"/> that returns an <see cref="IList{T}"/> of <see cref="UserLoginInfo"/> instances for the <paramref name="user"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
        /// <see cref="AzureTableUserException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        public virtual async Task<IList<UserLoginInfo>> GetLoginsAsync(T user)
        {
            AssertNotDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            // this goddamned method prevents me from using the fucking provider name as a fucking partition key.  Am upset.
            // this one-partition-per-type pattern is shitting up the place.
            var table = await GetTable();
            var userNameQuery = new TableQuery<AzureTableUserLogin>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(PropertyNames.PartitionKey, QueryComparisons.Equal, AzureTableUserLoginPartitionKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(PropertyNames.UserId, QueryComparisons.Equal, user.Id)));
            try
            {
                return table.ExecuteQuery(userNameQuery).Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey)).ToList();
            }catch(StorageException ex)
            {
                throw new AzureTableUserException("An exception was thrown while attempting to find the logins for this user.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Removes the given <paramref name="login"/> for the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">A A <see cref="AzureTableUser"/>-derived type-derived type</param>
        /// <param name="login"><see cref="UserLoginInfo"/></param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="login"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
        public async Task RemoveLoginAsync(T user, UserLoginInfo login)
        {
            AssertNotDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (login == null)
                throw new ArgumentNullException("login");

            try
            {
                var atul = new AzureTableUserLogin
                {
                    PartitionKey = AzureTableUserLoginPartitionKey,
                    ProviderKey = login.ProviderKey
                };
                atul.EnsureETagSet();
                await Run(TableOperation.Delete(atul));
            }
            catch (StorageException ex)
            {
                throw new AzureTableUserException("An exception was thrown while attempting to find the logins for this user.  See the inner exception for details.", ex);
            }
        }
        #endregion
    }
}
