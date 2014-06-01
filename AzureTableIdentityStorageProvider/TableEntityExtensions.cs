using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace StateStreetGang.AspNet.Identity.AzureTable
{
    /// <summary>
    /// Extension methods for the <see cref="TableEntity"/> type.
    /// </summary>
    public static class TableEntityExtensions
    {
        /// <summary>
        /// Ensures <see cref="TableEntity.ETag" /> is set.
        /// </summary>
        /// <typeparam name="T">The type of the entity. Must extend <see cref="TableEntity" />.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="eTag">The (optional) eTag.</param>
        /// <remarks>When performing a <see cref="TableOperation.Replace" />,
        /// <see cref="TableOperation.Merge" /> or <see cref="TableOperation.Delete" /> operation,
        /// <see cref="TableEntity.ETag" /> must be set, otherwise an <see cref="ArgumentException" /> is thrown.
        /// The ETag is used in optimistic concurrency by Azure storage.  The responsibility to set this value is on the Azure Storage subsystem.  When <c>null</c> or empty/whitespace,
        /// this method sets the ETag to the default value (the wildcard "*").  If the <paramref name="eTag"/> is provided, it will be used.
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity" /> is <c>null</c>.</exception>
        /// </remarks>
        /// <returns>The <paramref name="entity" /> passed into this method.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "e"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static T EnsureETagSet<T>(this T entity, string eTag = null) where T : TableEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            if (!string.IsNullOrWhiteSpace(eTag))
                entity.ETag = eTag;
            else if (string.IsNullOrWhiteSpace(entity.ETag))
                entity.ETag = "*";
            return entity;
        }
    }
}
