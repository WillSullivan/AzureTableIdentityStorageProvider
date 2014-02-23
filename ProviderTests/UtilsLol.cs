using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using StateStreetGang.AspNet.Identity.AzureTable;

namespace ProviderTests
{
    /// <summary>
    /// A utilities class, lol.  What framework without one is worth its salt?
    /// </summary>
    public static class UtilsLol
    {
        /// <summary>
        /// Deletes an Azure table by the given <param name="name"/>
        /// </summary>
        /// <param name="tableName">The table's name.</param>
        public static void DeleteTableLol(string name)
        {
            // deleting the table each time we get a new target.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.TestTableConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(name);
            table.DeleteIfExists();

        }
    }
}
