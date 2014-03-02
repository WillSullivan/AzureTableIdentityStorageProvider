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
    public partial class AzureTableRoleStore : AzureTableStore, IRoleStore<AzureTableRole>
    {
        #region props
        /// <summary>
        /// The name of the <see cref="Microsoft.WindowsAzure.Storage.Table.TableEntity.PartitionKey"/> property.
        /// </summary>
        protected const string PartitionKey = "PartitionKey";
        /// <summary>
        /// The name of the <see cref="Microsoft.WindowsAzure.Storage.Table.TableEntity.RowKey"/> property.
        /// </summary>
        protected const string RowKey = "RowKey";
        /// <summary>
        /// The name of the <see cref="AzureTableRole.Id"/> property.
        /// </summary>
        protected const string Id = "Id";
        /// <summary>
        /// The name of the <see cref="AzureTableRole.Name"/> property.
        /// </summary>
        protected const string Name = "Name";

        /// <summary>
        /// The default <see cref="TableName">Azure table name</see> used by this instance.
        /// </summary>
        public const string DefaultTableName = "AspNetIdentityRoleStore";

        /// <summary>
        /// Gets the name of the Azure table.
        /// </summary>
        /// <value></value>
        protected override string TableName
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
        public AzureTableRoleStore(string connectionString)
            : base(connectionString)
        {
        }
        #endregion

        #region IRoleStore<AzureTableRole> Members

        /// <summary>
        /// Creates the given <paramref name="role"/> in the Role store.
        /// </summary>
        /// <param name="role"><see cref="AzureTableRole"/></param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="role"/> is <c>null</c>.</exception>
        /// <exception cref="AzureTableRoleException">Thrown when <paramref name="role"/> already exists, or when any other <see cref="StorageException"/> is thrown during creation.</exception>
        public async Task CreateAsync(AzureTableRole role)
        {
            AssertNotDisposed();
            if (role == null)
                throw new ArgumentNullException("role");
            try
            {
                var result = await Run(TableOperation.Insert(role));
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
        /// <param name="role"><see cref="AzureTableRole"/></param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="role"/> is <c>null</c>.</exception>
        /// <see cref="AzureTableRoleException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        public async Task DeleteAsync(AzureTableRole role)
        {
            AssertNotDisposed();
            if (role == null)
                throw new ArgumentNullException("role");
            role.EnsureETagSet();
            try
            {
                await Run(TableOperation.Delete(role));
            }
            catch (StorageException ex)
            {
                //TODO:  Should I swallow 404s?  I'm not following the same pattern as CreateAsync :/
                if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    return;

                throw new AzureTableRoleException("An exception was thrown while attempting to delete this role.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Finds a <see cref="AzureTableRole"/> by its id in the Role store.
        /// </summary>
        /// <param name="roleId">The role's id.</param>
        /// <returns><see cref="Task{T}"/> that returns the <see cref="AzureTableRole"/> on completion.</returns>
        /// <remarks>If not found, <see cref="Task{T}.Result"/> will be <c>null</c>.</remarks>
        /// <see cref="AzureTableRoleException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        /// <exception cref="ArgumentException">Thrown if <paramref name="roleId"/> is <c>null</c> or empty.</exception>
        public async Task<AzureTableRole> FindByIdAsync(string roleId)
        {
            AssertNotDisposed();
            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentException("roleId cannot be null, empty, or consist of whitespace.");
            var table = await GetTable();
            var query = TableOperation.Retrieve<AzureTableRole>(AzureTableRole.DefaultRolePartitionKey, roleId);
            try
            {
                var result = await table.ExecuteAsync(query);
                return result.Result as AzureTableRole;
            }
            catch (StorageException ex)
            {
                throw new AzureTableRoleException("An exception was thrown while attempting to find the role.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Finds an <see cref="AzureTableRole"/> by its name.
        /// </summary>
        /// <param name="roleName">The <see cref="AzureTableRole"/> name/></param>
        /// <returns><see cref="AzureTableRole"/></returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="roleName"/> is <c>null</c> or empty.</exception>
        /// <see cref="AzureTableRoleException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        public async Task<AzureTableRole> FindByNameAsync(string roleName)
        {
            AssertNotDisposed();
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("roleName cannot be null, empty, or consist of whitespace.");
            var table = await GetTable();
            var query = new TableQuery<AzureTableRole>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(PartitionKey, QueryComparisons.Equal, AzureTableRole.DefaultRolePartitionKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(Name, QueryComparisons.Equal, roleName)))
                    .Take(1);

            try
            {
                return table.ExecuteQuery<AzureTableRole>(query).FirstOrDefault();
            }
            catch (StorageException ex)
            {
                throw new AzureTableRoleException("An exception was thrown while attempting to find this role.  See the inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Updates the <see cref="AzureTableRole"/> in the Role store.
        /// </summary>
        /// <param name="role"><see cref="AzureTableRole"/></param>
        /// <returns><see cref="Task"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="role"/> is <c>null</c>.</exception>
            /// <see cref="AzureTableRoleException">Thrown whenever a table operation results in a <see cref="StorageException"/> being thrown.</see>
        public async Task UpdateAsync(AzureTableRole role)
        {
            AssertNotDisposed();
            if (role == null)
                throw new ArgumentNullException("role");
            role.EnsureETagSet();
            var op = TableOperation.Replace(role);
            try
            {
                var result = await Run(op);
                role.ETag = result.Etag;
            }
            catch (StorageException ex)
            {
                throw new AzureTableRoleException("An exception was thrown while attempting to update this role.  See the inner exception for details.", ex);
            }
        }

        #endregion
    }
}