using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace StateStreetGang.AspNet.Identity.AzureTable
{
    /// <summary>
    /// An implementation of <see cref="IRole"/> that can be stored in an Azure table.
    /// </summary>
    public class AzureTableRole : Microsoft.WindowsAzure.Storage.Table.TableEntity, IRole
    {        
        /// <summary>
        /// The Azure Table partition key used by roles.
        /// </summary>
        public const string DefaultRolePartitionKey = "AspNetRolePartition";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableRole" /> class.
        /// </summary>
        /// <remarks>Azure tables consist of partitions with rows.  Row lookup in partitions is very fast.  It is not necessary to have a separate table 
        /// for roles, so a separate partition in an existing table should be used.  The partition key used for roles can be found at
        /// <see cref="DefaultRolePartitionKey"/>.</remarks>
        public AzureTableRole()
        {
            PartitionKey = DefaultRolePartitionKey;
        }

        /// <summary>
        /// The Id of the role
        /// </summary>
        /// <remarks>Ids should be unique.  The Id is also used as the <see cref="Microsoft.WindowsAzure.Storage.Table.TableEntity.RowKey"/>.</remarks>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is <c>null</c>, empty, or consists only of whitespace.</exception>
        public string Id
        {
            get { return this.RowKey; }
            set { 
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("value cannot be null, empty, or consist of whitespace.");
                this.RowKey = value; }
        }

        /// <summary>
        /// Gets or sets the name of the role.
        /// </summary>
        public string Name { get; set; }
    }
}
