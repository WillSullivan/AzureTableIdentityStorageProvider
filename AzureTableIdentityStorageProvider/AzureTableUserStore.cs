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
    /// <remarks>This class implements <see cref="IUserPasswordStore{T}"/>, <see cref="IUserRoleStore{T}"/>, <see cref="IUserLoginStore{T}"/>, <see cref="IUserClaimStore{T}"/> and <see cref="IUserStore{T}"/>.
    /// The base class that implements <see cref="IUser"/> which is referenced by this implementation is A <see cref="AzureTableUser"/>-derived type.
    /// 
    /// This pattern may seem a little odd, but it adds a level of convenience as all interfaces also implement <see cref="IUserStore{T}"/>.</remarks>
    public partial class AzureTableUserStore<T> :
        AzureTableStore,
        IUserStore<T> where T : AzureTableUser, new()
    {
        #region Props
        /// <summary>
        /// The default table name used for the AzureTableUserStore.
        /// </summary>
        public const string DefaultUserTableName = "AspNetIdentityUserStore";

        /// <summary>
        /// Gets the name of the Azure table.
        /// </summary>
        /// <value></value>
        protected virtual string UserTableName
        {
            get { return DefaultUserTableName; }
        }

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
        /// <param name="user">A <see cref="AzureTableUser"/>-derived type</param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="T.UserName"/> is <c>null</c> or empty.</exception>
        public virtual async Task CreateAsync(T user)
        {
            AssertNotDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(user.UserName))
                throw new ArgumentException("user.UserName cannot be null, empty, or consist of whitespace.");
            if (user.Id == null)
                SetUserId(user);
            try
            {
                user.PartitionKey = GetPartitionKeyByUserName(user.UserName);
                var result = await Run(UserTableName, TableOperation.Insert(user));
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
        /// <param name="user">A <see cref="AzureTableUser"/>-derived type</param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="user">user's</paramref> Id is null, empty or whitespace.</exception>
        public virtual async Task DeleteAsync(T user)
        {
            AssertNotDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(user.RowKey))
                throw new ArgumentException("User Id not set", "user");
            user.EnsureETagSet();
            try
            {
                user.PartitionKey = GetPartitionKeyByUserName(user.UserName);
                user.EnsureETagSet(); // I don't think this is necessary here but, whatevs
                await Run(UserTableName, TableOperation.Delete(user));
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    return;

                throw new AzureTableUserException("An exception was thrown while attempting to delete this user.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Finds a A <see cref="AzureTableUser"/>-derived type by that user's <see cref="AzureTableUser.Id">Id</see>.
        /// </summary>
        /// <param name="userId">The user's Id</param>
        /// <returns>A <see cref="Task{T}"/> that returns the A <see cref="AzureTableUser"/>-derived type if found, or <c>null</c> if not found.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="userId"/> is <c>null</c> or empty.</exception>
        public virtual async Task<T> FindByIdAsync(string userId)
        {
            AssertNotDisposed();
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId cannot be null, empty, or consist of whitespace.");
            var table = await GetTable(UserTableName);
            var query = new TableQuery<T>().Where(
                    TableQuery.GenerateFilterCondition(PropertyNames.Id, QueryComparisons.Equal, userId))
                    .Take(1);
            try
            {
                return table.ExecuteQuery(query).FirstOrDefault() as T;
            }
            catch (StorageException ex)
            {
                throw new AzureTableUserException("An exception was thrown while attempting to find the user.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Finds a A <see cref="AzureTableUser"/>-derived type by that user's <see cref="AzureTableUser.UserName">name</see>.
        /// </summary>
        /// <param name="userName">The name of the user to find.</param>
        /// <returns>A <see cref="Task{T}"/> that returns the A <see cref="AzureTableUser"/>-derived type if found, or <c>null</c> if not found.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="userName"/> is <c>null</c> or empty.</exception>
        public virtual async Task<T> FindByNameAsync(string userName)
        {
            AssertNotDisposed();
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("userName cannot be null, empty, or consist of whitespace.");
            var table = await GetTable(UserTableName);
            var userNameQuery = new TableQuery<T>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(PropertyNames.PartitionKey, QueryComparisons.Equal, GetPartitionKeyByUserName(userName)),
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
        /// <param name="user">A <see cref="AzureTableUser"/>-derived type</param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="user">user's</paramref> Id is null, empty or whitespace.</exception>
        public virtual async Task UpdateAsync(T user)
        {
            AssertNotDisposed();
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(user.RowKey))
                throw new ArgumentException("User Id not set", "user");
            user.EnsureETagSet();
            var op = TableOperation.Replace(user);
            try
            {
                user.PartitionKey = GetPartitionKeyByUserName(user.UserName);
                var result = await Run(UserTableName, op);
                user.ETag = result.Etag;
            }
            catch (StorageException ex)
            {
                throw new AzureTableUserException("An exception was thrown while attempting to update this user.  See the inner exception for details.", ex);
            }
        }

        #endregion

        #region protecteds
        /// <summary>
        /// Called when a new user is created.  Allows inheritors to generate a new Id for the user.
        /// </summary>
        /// <param name="user">The user to create an Id for.</param>
        protected virtual void SetUserId(T user)
        {
            user.Id = Guid.NewGuid().ToString(); // I feel dirty
        }

        /// <summary>
        /// Gets the partition key by user name.  
        /// </summary>
        /// <param name="userName">The user's name.</param>
        /// <returns>The partition key</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="userName"/> is <c>null</c> or empty.</exception>
        /// <remarks>The default implementation returns <see cref="AzureTableUser.DefaultAspNetUserPartitionKey"/>.  This method is called on creation to allow inheritors to use an algorithm to generate partitions for users.  It is also used when searching for a user by name.</remarks>
        protected virtual string GetPartitionKeyByUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("userName cannot be null, empty, or consist of whitespace.");
            return AzureTableUser.DefaultAspNetUserPartitionKey;
        }
        #endregion
    }
}
