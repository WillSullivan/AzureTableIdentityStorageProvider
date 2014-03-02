using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateStreetGang.AspNet.Identity.AzureTable
{
    /// <summary>
    /// Property names used in table queries.
    /// </summary>
    internal static class PropertyNames
    {
        /// <summary>
        /// The name of the <see cref="Microsoft.WindowsAzure.Storage.Table.TableEntity.PartitionKey"/> property.
        /// </summary>
        public const string PartitionKey = "PartitionKey";
        /// <summary>
        /// The name of the <see cref="Microsoft.WindowsAzure.Storage.Table.TableEntity.RowKey"/> property.
        /// </summary>
        public const string RowKey = "RowKey";
        /// <summary>
        /// The name of the <see cref="AzureTableUser.Id"/> property.
        /// </summary>
        public const string Id = "Id";
        /// <summary>
        /// The name of the <see cref="AzureTableUserLogin.UserId"/> property.
        /// </summary>
        public const string UserId = "UserId";
        /// <summary>
        /// The name of the <see cref="AzureTableUser.UserName"/> property.
        /// </summary>
        public const string UserName = "UserName";
        /// <summary>
        /// The name of the <see cref="AzureTableRole.Name"/> property.
        /// </summary>
        public const string Name = "Name";
    }
}
