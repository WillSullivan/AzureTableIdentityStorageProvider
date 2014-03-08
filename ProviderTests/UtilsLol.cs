using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

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
        /// <summary>
        /// Performs an assert that the given action should throw when called.
        /// </summary>
        /// <param name="shouldThrow"><see cref="Action"/> to call.</param>
        /// <param name="exceptionType">Ensures the exception thrown is this expected type.</param>
        public static void AssertThrows(Action shouldThrow, Type exceptionType = null)
        {
            if (shouldThrow == null)
                throw new ArgumentNullException("shouldThrow");
            try
            {
                shouldThrow();
                Assert.Fail((exceptionType ?? typeof(Exception)).ToString() + " was not thrown.");
            }
            catch (Exception ex)
            {
                if (exceptionType != null)
                    Assert.AreSame(ex.GetType(), exceptionType);
            }
        }
        /// <summary>
        /// Performs an assert that the given func should throw when called.
        /// </summary>
        /// <typeparam name="T">The type of the return value of the func.</typeparam>
        /// <param name="shouldThrow"><see cref="Func{T}" /> to call.</param>
        /// <param name="exceptionType">Ensures the exception thrown is this expected type.</param>
        public static void AssertThrows<T>(Func<T> shouldThrow, Type exceptionType = null)
        {
            if (shouldThrow == null)
                throw new ArgumentNullException("shouldThrow");
            try
            {
                var temp = shouldThrow();
                Assert.Fail((exceptionType ?? typeof(Exception)).ToString() + " was not thrown.");
            }
            catch (Exception ex)
            {
                if (exceptionType != null)
                    Assert.AreSame(ex.GetType(), exceptionType);
            }
        }
    }
}
