using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace StateStreetGang.AspNet.Identity.AzureTable
{
    /// <summary>
    /// Ensures <see cref="TableEntity.ETag"/> is set.
    /// </summary>
    /// <remarks>When performing a <see cref="TableOperation.Replace"/>, 
    /// <see cref="TableOperation.Merge"/> or <see cref="TableOperation.Delete"/> operation,
    /// <see cref="TableEntity.ETag"/> must be set, otherwise an <see cref="ArugmentException"/> is thrown.
    /// The ETag is used in optimistic concurrency by Azure storage.  The responsibility to set this value is on the Azure Storage subsystem.  When <c>null</c> or empty/whitespace,
    /// this method sets the ETag to the default value (the wildcard "*").
    /// </remarks>
    public static class TableEntityExtensions 
    {
        public static void EnsureETagSet(this TableEntity entity)
        {
            if (entity == null)
                return;
            if (string.IsNullOrWhiteSpace(entity.ETag))
                entity.ETag = "*";
        }
    }
}
