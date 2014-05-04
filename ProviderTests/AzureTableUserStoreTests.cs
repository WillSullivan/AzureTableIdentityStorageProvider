using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StateStreetGang.AspNet.Identity.AzureTable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace ProviderTests
{
    [TestClass()]
    public partial class AzureTableUserStoreTests : AzureTableUserStoreTestBase
    {
        #region user
        [TestMethod()]
        public void CreateAsyncTest()
        {
            // wow, such similar, very copypasta 
            var target = Target();
            var user = new AzureTableUser
            {
                //ASP.NET identity doesn't set the ID on a new user, adding this into the provider
                //Id = "foo",
                UserName = "bar"
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

            target.CreateAsync(user).Wait();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(user.ETag));

            try
            {
                target.CreateAsync(user).Wait();
                Assert.Fail("CreateAsync created the same user twice");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is AzureTableUserException);
            }
            var backAgain = target.FindByIdAsync(user.Id).Result;

            Assert.AreEqual(user.UserName, backAgain.UserName);
            target.Dispose();

            try
            {
                target.CreateAsync(user).Wait();
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
            var user = new AzureTableUser
            {
                Id = "foo",
                UserName = "bar"
            };
            UtilsLol.AssertThrows(() => target.DeleteAsync(new AzureTableUser
            {
                UserName = "bar"
            }).Wait());
            try
            {
                target.DeleteAsync(null).Wait();
                Assert.Fail("DeleteAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentNullException);
            }

            target.CreateAsync(user).Wait();
            Assert.IsNotNull(target.FindByIdAsync(user.Id).Result); // sanity

            target.DeleteAsync(user).Wait();

            Assert.IsNull(target.FindByIdAsync(user.Id).Result);

            target.DeleteAsync(user).Wait();

            target.Dispose();

            try
            {
                target.DeleteAsync(user).Wait();
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
            // holy crap... this entire class is copypasta 
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

            var user = new AzureTableUser
            {
                Id = "foo",
                UserName = "bar"
            };

            target.CreateAsync(user).Wait();
            Assert.IsNotNull(target.FindByIdAsync(user.Id).Result); // sanity

            target.DeleteAsync(user).Wait();

            Assert.IsNull(target.FindByIdAsync(user.Id).Result);

            target.Dispose();

            try
            {
                target.FindByIdAsync(user.Id).Wait();
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
            var userName = "This is My User Name";
            var target = Target();
            var user = new AzureTableUser
            {
                Id = "foo",
                UserName = userName
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

            target.CreateAsync(user).Wait();
            var backagain = target.FindByNameAsync(user.UserName).Result;
            Assert.IsNotNull(backagain); // sanity

            //FindByName should be CASE INSENSITIVE!
            // The current storage emulator has a BUG that prevents this part of the test from working :/
            //var insensitive = target.FindByNameAsync(userName.ToLowerInvariant()).Result;
            //Assert.IsNotNull(insensitive);
            //insensitive = target.FindByNameAsync(userName.ToUpperInvariant()).Result;
            //Assert.IsNotNull(insensitive);
            //insensitive = target.FindByNameAsync(userName).Result;
            //Assert.IsNotNull(insensitive);

            target.DeleteAsync(user).Wait();

            Assert.IsNull(target.FindByNameAsync(user.Id).Result);

            target.Dispose();

            try
            {
                target.FindByNameAsync(user.Id).Wait();
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
            var user = new AzureTableUser
            {
                Id = "foo",
                UserName = "bar"
            };
            UtilsLol.AssertThrows(() => target.UpdateAsync(new AzureTableUser
            {
                UserName = "bar"
            }).Wait());

            UtilsLol.AssertThrows(() => target.UpdateAsync(null).Wait());

            target.CreateAsync(user).Wait();
            Assert.IsNotNull(target.FindByIdAsync(user.Id).Result); // sanity

            var newUser = new AzureTableUser
            {
                Id = "foo",
                UserName = "derp"
            };

            target.UpdateAsync(newUser).Wait();

            Assert.AreEqual(newUser.UserName, target.FindByIdAsync(user.Id).Result.UserName);

            target.Dispose();

            try
            {
                target.UpdateAsync(newUser).Wait();
                Assert.Fail("FindByNameAsync doesn't throw when disposed");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ObjectDisposedException);
            }
        }
        #endregion
    }
}
