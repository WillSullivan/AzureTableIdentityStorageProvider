using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace StateStreetGang.AspNet.Identity.AzureTable
{
    /// <summary>
    /// Contains information about a currently logged in user.
    /// </summary>
    public class AzureTableUserLogin : TableEntity
    {
        /// <summary>
        /// The default partition key used for all instances.
        /// </summary>
        // goddamned API won't let me use provider key name as a partition key.  Pissed.
        public const string DefaultPartitionKey = "AzureTableUserLoginPartition";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableUserLogin" /> class.
        /// </summary>
        public AzureTableUserLogin()
        {
            PartitionKey = DefaultPartitionKey;
        }

        /// <summary>
        /// Gets or sets the login provider name.
        /// </summary>
        /// <remarks>The login provider name is also used as the <see cref="TableEntity.PartitionKey"/> for this type.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public string LoginProvider { get; set; }

        /// <summary>
        /// The provider's key for this <see cref="UserId">user</see>.
        /// </summary>
        /// <remarks>The provider key is used as the <see cref="TableIdentity.RowKey"/>.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public string ProviderKey
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
        /// The user's Id
        /// </summary>
        public string UserId { get; set; }
    }
}