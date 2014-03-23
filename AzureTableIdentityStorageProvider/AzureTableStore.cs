using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using StateStreetGang.AspNet.Identity.AzureTable.Properties;

namespace StateStreetGang.AspNet.Identity.AzureTable
{
    /// <summary>
    /// A base class with convenience methods for types that use Azure Tables as entity stores.
    /// </summary>
    public abstract class AzureTableStore 
    {
        #region properties
        
        /// <summary>
        /// The <see cref="CloudStorageAccount"/> used by this instance.
        /// </summary>
        [CLSCompliant(false)]
        protected CloudStorageAccount StorageAccount { get; private set; }
        
        #endregion
        
        #region ctor
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStore" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="connectionString" /> is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionString"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="connectionString"/> is <c>null</c> or empty.</exception>
        protected AzureTableStore(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException(Resources.ConnectionStringNOW);
            try
            {
                StorageAccount = CloudStorageAccount.Parse(connectionString);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(Resources.InvalidConnectionString, ex);
            }
        }

        #endregion
        
        #region protected methods
        
        /// <summary>
        /// Gets the <see cref="CloudTable" /> that backs this store.
        /// </summary>
        /// <param name="tableName">The name of the table to retrieve.</param>
        /// <returns>
        /// A <see cref="Task{T}" /> that returns the <see cref="CloudTable" /> instance.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="tableName"/> is <c>null</c> or empty.</exception>
        protected virtual async Task<CloudTable> GetTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("tableName cannot be null, empty, or consist of whitespace.");
            var client = GetTableClient();
            var result = client.GetTableReference(tableName);
            await result.CreateIfNotExistsAsync();
            return result;
        }
        
        /// <summary>
        /// Runs the given <paramref name="op"/> on the Azure table.
        /// </summary>
        /// <param name="op"><see cref="TableOperation"/></param>
        /// <returns><see cref="Task"/></returns>
        protected virtual async Task<TableResult> Run(string tableName, TableOperation op)
        {
            var table = await GetTable(tableName);
            return await table.ExecuteAsync(op);
        }
        
        /// <summary>
        /// Gets the <see cref="CloudTableClient"/> for the role table.
        /// </summary>
        /// <returns><see cref="CloudTableClient"/></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "StorageAccount", Justification = "Referring to a property")]
        protected virtual CloudTableClient GetTableClient()
        {
            var acct = StorageAccount;
            if (acct == null)
                throw new InvalidOperationException("StorageAccount is null.");

            return StorageAccount.CreateCloudTableClient();
        }
        
        #endregion        
    }
}