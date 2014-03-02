using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace StateStreetGang.AspNet.Identity.AzureTable
{
    /// <summary>
    /// A <see cref="TableEntity"/> used to store roles for a user.
    /// </summary>
    /// <remarks>The <see cref="Microsoft.WindowsAzure.Storage.Table.TableEntity.RowKey"/> should be the user's Id.</remarks>
    public class AzureTableUserRoles : TableEntity
    {
        /// <summary>
        /// The default partition key used for <see cref="AzureTableUserRoles"/> instances.
        /// </summary>
        // sick of doing this :/
        public const string DefaultAzureTableUserRolesPartitionKey = "AzuretableUserRolesPartition";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableUserRoles" /> class.
        /// </summary>
        public AzureTableUserRoles()
        {
            PartitionKey = DefaultAzureTableUserRolesPartitionKey;
        }

        /// <summary>
        /// Gets or sets the <see cref="StateStreetGang.AspNet.Identity.AzureTable.AzureTableUser.Id">user's id</see>.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public string UserId
        {
            get { return RowKey; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                RowKey = value;
            }
        }

        /// <summary>
        /// A delimited list of roles for the user.
        /// </summary>
        public string Roles { get; set; }
    }
}