using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace StateStreetGang.AspNet.Identity.AzureTable
{
    /// <summary>
    /// A user store that uses Azure Table Storage as a backing store for ASP.NET Identity.
    /// </summary>
    /// <remarks>Unfortunately, the role partition key is pretty much static, as we must get the key with only the role's name or the role's Id, precluding the use of some kind of mapping function.</remarks>
    public partial class AzureTableRoleStore<T> : AzureTableRoleStore, IRoleStore<T> where T : AzureTableRole, new()
    {
        #region properties
        /// <summary>
        /// Returns the role partition key.
        /// </summary>
        /// <remarks>if not overridden, the <see cref="AzureTableRole.DefaultRolePartitionKey">default partition key</see> is returned.</remarks>
        protected virtual string RolePartitionKey
        {
            get
            {
                return AzureTableRole.DefaultRolePartitionKey;
            }
        }
        #endregion

        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableRoleStore" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="connectionString" /> is <c>null</c> or empty.</exception>
        public AzureTableRoleStore(string connectionString)
            : base(connectionString)
        {
        }
        #endregion

        #region IRoleStore<T> Members
        /// <summary>
        /// Creates the given <paramref name="role"/> in the Role store.
        /// </summary>
        /// <param name="role"><see cref="T"/></param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="role"/> is <c>null</c>.</exception>
        /// <exception cref="AzureTableRoleException">Thrown when <paramref name="role"/> already exists, or when any other <see cref="StorageException"/> is thrown during creation.</exception>
        public virtual async Task CreateAsync(T role)
        {
            AssertNotDisposed();
            if (role == null)
                throw new ArgumentNullException("role");
            if (role.Id == null)
                SetRoleId(role);
            try
            {
                var result = await Run(RoleTableName, TableOperation.Insert(role.EnsureETagSet()));
                role.ETag = result.Etag;
            }
            catch (StorageException ex)
            {
                //TODO:  Not sure of this pattern; should role ID be its name, in which case I should just swallow this exception silently?               
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict) throw new AzureTableRoleException("The role already exists", ex);
                throw new AzureTableRoleException("An exception was thrown while attempting to create this role.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Removes the given <paramref name="role"/> from the Role store.
        /// </summary>
        /// <param name="role"><see cref="T"/></param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="role"/> is <c>null</c>.</exception>
        /// <see cref="AzureTableRoleException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        public virtual async Task DeleteAsync(T role)
        {
            AssertNotDisposed();
            if (role == null)
                throw new ArgumentNullException("role");
            if (string.IsNullOrWhiteSpace(role.RowKey))
                throw new ArgumentException("Role Id not set", "role");
            role.EnsureETagSet();
            try
            {
                await Run(RoleTableName, TableOperation.Delete(role.EnsureETagSet()));
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    return;

                throw new AzureTableRoleException("An exception was thrown while attempting to delete this role.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Finds a <see cref="T"/> by its id in the Role store.
        /// </summary>
        /// <param name="roleId">The role's id.</param>
        /// <returns><see cref="Task{T}"/> that returns the <see cref="T"/> on completion.</returns>
        /// <remarks>If not found, <see cref="Task{T}.Result"/> will be <c>null</c>.</remarks>
        /// <see cref="AzureTableRoleException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        /// <exception cref="ArgumentException">Thrown if <paramref name="roleId"/> is <c>null</c> or empty.</exception>
        public virtual async Task<T> FindByIdAsync(string roleId)
        {
            AssertNotDisposed();
            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentException("roleId cannot be null, empty, or consist of whitespace.");
            var table = await GetTable(RoleTableName);
            var query = TableOperation.Retrieve<T>(GetPartitionKey(), roleId);
            try
            {
                var result = await table.ExecuteAsync(query);
                return result.Result as T;
            }
            catch (StorageException ex)
            {
                throw new AzureTableRoleException("An exception was thrown while attempting to find the role.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Finds an <see cref="T"/> by its name.
        /// </summary>
        /// <param name="roleName">The <see cref="T"/> name/></param>
        /// <returns><see cref="T"/></returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="roleName"/> is <c>null</c> or empty.</exception>
        /// <see cref="AzureTableRoleException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        public virtual async Task<T> FindByNameAsync(string roleName)
        {
            AssertNotDisposed();
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("roleName cannot be null, empty, or consist of whitespace.");
            var table = await GetTable(RoleTableName);
            var query = new TableQuery<T>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(PropertyNames.PartitionKey, QueryComparisons.Equal, GetPartitionKey()),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(PropertyNames.Name, QueryComparisons.Equal, roleName)))
                    .Take(1);

            try
            {
                return table.ExecuteQuery<T>(query).FirstOrDefault();
            }
            catch (StorageException ex)
            {
                throw new AzureTableRoleException("An exception was thrown while attempting to find this role.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Updates the <see cref="T"/> in the Role store.
        /// </summary>
        /// <param name="role"><see cref="T"/></param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="role"/> is <c>null</c>.</exception>
        /// <see cref="AzureTableRoleException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        /// <see cref="AzureTableRoleException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        public virtual async Task UpdateAsync(T role)
        {
            AssertNotDisposed();
            if (role == null)
                throw new ArgumentNullException("role");
            if (string.IsNullOrWhiteSpace(role.RowKey))
                throw new ArgumentException("Role Id not set", "role");
            var op = TableOperation.Replace(role.EnsureETagSet());
            try
            {
                var result = await Run(RoleTableName, op);
                role.ETag = result.Etag;
            }
            catch (StorageException ex)
            {
                throw new AzureTableRoleException("An exception was thrown while attempting to update this role.  See the inner exception for details.", ex);
            }
        }
        #endregion

        #region protecteds
        /// <summary>
        /// Sets the <see cref="T.Id">role's Id</see>.
        /// </summary>
        /// <param name="role"><see cref="T"/></param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="role"/> is <c>null</c>.</exception>
        /// <remarks>When a role is created, ASP.NET Identity does not generate an Id.  This method allows inheritors to use their own algorithm for the role's Id.</remarks>
        protected virtual void SetRoleId(T role)
        {
            if (role == null)
                throw new ArgumentNullException("role");
            role.Id = Guid.NewGuid().ToString(); // haaaaaaack
        }
        #endregion

        #region privates
        /// <summary>
        /// Ensures we get a valid key every time.
        /// </summary>
        /// <returns>The <see cref="RolePartitionKey"/> or the default if not found.</returns>
        private string GetPartitionKey()
        {
            return RolePartitionKey ?? AzureTableRole.DefaultRolePartitionKey;
        }
        #endregion
    }

    /// <summary>
    /// An implementation of <see cref="AzureTableStore"/> for storing roles
    /// </summary>
    public abstract class AzureTableRoleStore : AzureTableStore
    {

        #region props
        /// <summary>
        /// The default <see cref="RoleTableName">Azure table name</see> used by this instance.
        /// </summary>
        public const string DefaultTableName = "AspNetIdentityRoleStore";

        /// <summary>
        /// Gets the name of the Azure table.
        /// </summary>
        /// <value></value>
        protected virtual string RoleTableName
        {
            get
            {
                return DefaultTableName;
            }
        }
        #endregion

        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableRoleStore" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="connectionString" /> is <c>null</c> or empty.</exception>
        protected AzureTableRoleStore(string connectionString)
            : base(connectionString)
        {
        }
        #endregion
    }
}