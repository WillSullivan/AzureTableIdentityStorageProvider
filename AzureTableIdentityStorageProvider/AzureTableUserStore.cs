using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

/*
 * See child partial classes for implementations of each interface.
 */
namespace StateStreetGang.AspNet.Identity.AzureTable
{
    /// <summary>
    /// A user store that uses Azure Table Storage as a backing store for ASP.NET Identity.
    /// </summary>
    /// <remarks>This class implements <see cref="IUserPasswordStore{T}"/>, <see cref="IUserUserStore{T}"/>, <see cref="IUserLoginStore{T}"/>, <see cref="IUserClaimStore{T}"/> and <see cref="IUserStore{T}"/>.
    /// The base class that implements <see cref="IUser"/> which is referenced by this implementation is <see cref="AzureTableUser"/>.
    /// 
    /// This pattern may seem a little odd, but it adds a level of convenience as all interfaces also implement <see cref="IUserStore{T}"/>.</remarks>
    public partial class AzureTableUserStore :
        AzureTableStore,
        IUserStore<AzureTableUser>
    {
        public const string DefaultTableName = "AspNetIdentityUserStore";

        #region Props
        /// <summary>
        /// The partition key used in the users table.
        /// </summary>
        public virtual string UserPartitionKey { get { return AzureTableUser.DefaultAspNetUserPartitionKey; } }

        /// <summary>
        /// Gets the type of <see cref="StringComparison"/> used in string comparisons.
        /// </summary>
        protected virtual StringComparison StringComparisonType
        {
            get
            {
                return StringComparison.CurrentCultureIgnoreCase;
            }
        }

        /// <summary>
        /// Gets the type of <see cref="StringComparer"/> used in string sorting.
        /// </summary>
        protected virtual StringComparer StringComparerType
        {
            get
            {
                return StringComparer.CurrentCultureIgnoreCase;
            }
        }
        #endregion

        #region AzureTableStore members
        /// <summary>
        /// Gets the name of the Azure table.
        /// </summary>
        /// <value></value>
        protected override string TableName
        {
            get { return DefaultTableName; }
        }
        #endregion

        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableUserStore" /> class.
        /// </summary>
        /// <param name="connectionString">The Azure table connection string.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="connectionString" /> is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionString"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="connectionString"/> is <c>null</c> or empty.</exception>
        public AzureTableUserStore(string connectionString)
            : base(connectionString)
        {
        }
        #endregion

        #region IUserStore<AzureTableUser> Members
        /// <summary>
        /// Creates the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user"><see cref="AzureTableUser"/></param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
        public virtual async Task CreateAsync(AzureTableUser user)
        {
            AssertNotDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            try
            {
                var result = await Run(TableOperation.Insert(user));
                user.ETag = result.Etag;
            }
            catch (StorageException ex)
            {
                //TODO:  Not sure of this pattern; should User ID be its name, in which case I should just swallow this exception silently?               
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict) throw new AzureTableUserException("The user already exists", ex);
                throw new AzureTableUserException("An exception was thrown while attempting to create this user.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Deletes the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user"><see cref="AzureTableUser"/></param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
        public virtual async Task DeleteAsync(AzureTableUser user)
        {
            AssertNotDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            user.EnsureETagSet();
            try
            {
                await Run(TableOperation.Delete(user));
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    return;

                throw new AzureTableUserException("An exception was thrown while attempting to delete this user.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Finds a <see cref="AuzreTableUser"/> by that user's <see cref="AzuretableUser.Id">Id</see>.
        /// </summary>
        /// <param name="userId">The user's Id</param>
        /// <returns>A <see cref="Task{T}"/> that returns the <see cref="AzureTableUser"/> if found, or <c>null</c> if not found.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="userId"/> is <c>null</c> or empty.</exception>
        public virtual async Task<AzureTableUser> FindByIdAsync(string userId)
        {
            AssertNotDisposed();
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId cannot be null, empty, or consist of whitespace.");
            var table = await GetTable();
            var query = TableOperation.Retrieve<AzureTableUser>(UserPartitionKey, userId);
            var result = await table.ExecuteAsync(query);
            try
            {
                return result.Result as AzureTableUser; 
            }
            catch (StorageException ex)
            {
                throw new AzureTableUserException("An exception was thrown while attempting to find the user.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Finds a <see cref="AuzreTableUser"/> by that user's <see cref="AzuretableUser.Name">name</see>.
        /// </summary>
        /// <param name="userName">The name of the user to find.</param>
        /// <returns>A <see cref="Task{T}"/> that returns the <see cref="AzureTableUser"/> if found, or <c>null</c> if not found.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="userName"/> is <c>null</c> or empty.</exception>
        public virtual async Task<AzureTableUser> FindByNameAsync(string userName)
        {
            AssertNotDisposed();
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("userName cannot be null, empty, or consist of whitespace.");
            var table = await GetTable();
            var userNameQuery = new TableQuery<AzureTableUser>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(PropertyNames.PartitionKey, QueryComparisons.Equal, UserPartitionKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(PropertyNames.UserName, QueryComparisons.Equal, userName)))
                    .Take(1);

            try
            {
                return table.ExecuteQuery(userNameQuery).FirstOrDefault();
            }
            catch (StorageException ex)
            {
                throw new AzureTableUserException("An exception was thrown while attempting to find this user.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Updates the given <paramref name="user"/> in the database.
        /// </summary>
        /// <param name="user"><see cref="AzureTableUser"/></param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
        public virtual async Task UpdateAsync(AzureTableUser user)
        {
            AssertNotDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            user.EnsureETagSet();
            var op = TableOperation.Replace(user);
            try
            {
                var result = await Run(op);
                user.ETag = result.Etag;
            }
            catch (StorageException ex)
            {
                throw new AzureTableUserException("An exception was thrown while attempting to update this user.  See the inner exception for details.", ex);
            }
        }

        #endregion
    }
}
