using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using StateStreetGang.AspNet.Identity.AzureTable;

namespace ProviderTests
{
    /// <summary>
    /// These tests depend on the Azure storage emulator running, and the configured connection string in app.config being correct.
    /// </summary>
    [TestClass]
    public class AzureTableRoleStoreTests
    {
        private AzureTableRoleStore Target()
        {
            //TODO:  Figure out why TestInitialize is being ignored :/
            UtilsLol.DeleteTableLol(AzureTableRoleStore.DefaultTableName);
            return new AzureTableRoleStore(Properties.Settings.Default.TestTableConnectionString);
        }

        [TestMethod]
        public void CrudIntegrationTests()
        {
            var role = new AzureTableRole
            {
                // ASP.NET Identity doesn't supply this on create
                //Id = "Foo",
                Name = "Subject"
            };
            var second = new AzureTableRole
            {
                //Id = "Bar",
                Name = "Dummy"
            };
            var target = Target();
            target.CreateAsync(role).Wait();
            target.CreateAsync(second).Wait();

            var findById = target.FindByIdAsync(role.Id).Result;
            Assert.AreEqual(role.Id, findById.Id);
            Assert.AreEqual(role.Name, findById.Name);

            var findByName = target.FindByNameAsync(role.Name).Result;
            Assert.AreEqual(role.Id, findByName.Id);
            Assert.AreEqual(role.Name, findByName.Name);

            var newRole = new AzureTableRole
            {
                Id = role.Id,
                Name = "Updated"
            };

            target.UpdateAsync(newRole).Wait();
            var updatedRole = target.FindByIdAsync(newRole.Id).Result;
            Assert.AreEqual(newRole.Name, updatedRole.Name);

            target.DeleteAsync(updatedRole).Wait();

            Assert.IsNull(target.FindByIdAsync(updatedRole.Id).Result);
            Assert.IsNull(target.FindByNameAsync(updatedRole.Name).Result);

            target.DeleteAsync(second).Wait();
            target.Dispose();
        }

        [TestMethod()]
        public void CreateAsyncTest()
        {
            var target = Target();
            var role = new AzureTableRole
            {
                //Id = "foo",
                Name = "bar"
            };
            try
            {
                target.CreateAsync(null).Wait();
                Assert.Fail("CreateAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentNullException);
            }

            target.CreateAsync(role).Wait();

            try
            {
                target.CreateAsync(role).Wait();
                Assert.Fail("CreateAsync created the same role twice");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is AzureTableRoleException);
            }
            Assert.AreEqual(role.Name, target.FindByIdAsync(role.Id).Result.Name);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(role.ETag)); // set the etag before the method returns
            target.Dispose();

            try
            {
                target.CreateAsync(role).Wait();
                Assert.Fail("CreateAsync doesn't throw when disposed");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ObjectDisposedException);
            }
        }

        [TestMethod()]
        public void DeleteAsyncTest()
        {

            var target = Target();
            var role = new AzureTableRole
            {
                Id = "foo",
                Name = "bar"
            };
            UtilsLol.AssertThrows(() => target.DeleteAsync(
            new AzureTableRole
            {
                Name = "bar"
            }).Wait());
            UtilsLol.AssertThrows(() => target.DeleteAsync(null).Wait());

            target.CreateAsync(role).Wait();
            Assert.IsNotNull(target.FindByIdAsync(role.Id).Result); // sanity

            target.DeleteAsync(role).Wait();

            Assert.IsNull(target.FindByIdAsync(role.Id).Result);

            // am currently swallowing exceptions if attempting to delete something not there
            target.DeleteAsync(role).Wait();

            target.Dispose();

            try
            {
                target.DeleteAsync(role).Wait();
                Assert.Fail("deleteAsync doesn't throw when disposed");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ObjectDisposedException);
            }
        }

        [TestMethod()]
        public void FindByIdAsyncTest()
        {
            // pretty much same as delete, grumble grumble
            var target = Target();

            try
            {
                target.FindByIdAsync(null).Wait();
                Assert.Fail("FindByIdAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentException);
            }
            try
            {
                target.FindByIdAsync("").Wait();
                Assert.Fail("FindByIdAsync didn't throw on empty");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentException);
            }
            try
            {
                target.FindByIdAsync(" ").Wait();
                Assert.Fail("FindByIdAsync didn't throw on whitespace");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentException);
            }

            var role = new AzureTableRole
            {
                Id = "foo",
                Name = "bar"
            };

            target.CreateAsync(role).Wait();
            Assert.IsNotNull(target.FindByIdAsync(role.Id).Result); // sanity

            target.DeleteAsync(role).Wait();

            Assert.IsNull(target.FindByIdAsync(role.Id).Result);

            target.Dispose();

            try
            {
                target.FindByIdAsync(role.Id).Wait();
                Assert.Fail("FindByIdAsync doesn't throw when disposed");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ObjectDisposedException);
            }
        }

        [TestMethod()]
        public void FindByNameAsyncTest()
        {
            var target = Target();
            var role = new AzureTableRole
            {
                Id = "foo",
                Name = "bar"
            };

            try
            {
                target.FindByNameAsync(null).Wait();
                Assert.Fail("FindByNameAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentException);
            }
            try
            {
                target.FindByNameAsync("").Wait();
                Assert.Fail("FindByNameAsync didn't throw on empty");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentException);
            }
            try
            {
                target.FindByNameAsync(" ").Wait();
                Assert.Fail("FindByNameAsync didn't throw on whitespace");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentException);
            }

            target.CreateAsync(role).Wait();
            Assert.IsNotNull(target.FindByNameAsync(role.Name).Result); // sanity

            target.DeleteAsync(role).Wait();

            Assert.IsNull(target.FindByNameAsync(role.Id).Result);

            target.Dispose();

            try
            {
                target.FindByNameAsync(role.Id).Wait();
                Assert.Fail("FindByNameAsync doesn't throw when disposed");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ObjectDisposedException);
            }
        }

        [TestMethod()]
        public void UpdateAsyncTest()
        {
            var target = Target();
            var role = new AzureTableRole
            {
                Id = "foo",
                Name = "bar"
            };

            UtilsLol.AssertThrows(() => target.UpdateAsync(
            new AzureTableRole
            {
                Name = "bar"
            }).Wait());

            UtilsLol.AssertThrows(() => target.UpdateAsync(null).Wait());

            target.CreateAsync(role).Wait();
            Assert.IsNotNull(target.FindByIdAsync(role.Id).Result); // sanity

            var newRole = new AzureTableRole
            {
                Id = "foo",
                Name = "derp"
            };

            target.UpdateAsync(newRole).Wait();

            Assert.AreEqual(newRole.Name, target.FindByIdAsync(role.Id).Result.Name);

            target.Dispose();

            try
            {
                target.UpdateAsync(newRole).Wait();
                Assert.Fail("FindByNameAsync doesn't throw when disposed");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ObjectDisposedException);
            }
        }

    }
}
