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
    public partial class AzureTableUserStoreTests
    {

        private AzureTableUserStore Target()
        {
            UtilsLol.DeleteTableLol(AzureTableUserStore.DefaultTableName);
            return new AzureTableUserStore(Properties.Settings.Default.TestTableConnectionString);
        }

        #region user
        [TestMethod()]
        public void CreateAsyncTest()
        {
            // wow, such similar, very copypasta 
            var target = Target();
            var user = new AzureTableUser
            {
                Id = "foo",
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
            var target = Target();
            var user = new AzureTableUser
            {
                Id = "foo",
                UserName = "bar"
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

            try
            {
                target.UpdateAsync(null).Wait();
                Assert.Fail("UpdateAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentNullException);
            }

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

        #region role
        [TestMethod()]
        public void AddToRoleAsyncTest()
        {
            var target = Target();
            var user = new AzureTableUser
            {
                Id = "foo",
                UserName = "bar"
            };
            var role = "derp";

            try
            {
                target.AddToRoleAsync(null, null).Wait();
                Assert.Fail("CreateAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentNullException);
            }
            try
            {
                target.AddToRoleAsync(null, role).Wait();
                Assert.Fail("CreateAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentNullException);
            }
            try
            {
                target.AddToRoleAsync(user, null).Wait();
                Assert.Fail("CreateAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentNullException);
            }
            try
            {
                target.AddToRoleAsync(user, "").Wait();
                Assert.Fail("CreateAsync didn't throw on empty");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentException);
            }
            try
            {
                target.AddToRoleAsync(user, " ").Wait();
                Assert.Fail("CreateAsync didn't throw on whitespace");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentException);
            }

            target.AddToRoleAsync(user, role).Wait();
            target.AddToRoleAsync(user, role).Wait();
            var roles = target.GetRolesAsync(user).Result;
            Assert.AreEqual(1, roles.Count);
            Assert.AreEqual(role, roles[0]);

            target.Dispose();

            try
            {
                target.AddToRoleAsync(user, role).Wait();
                Assert.Fail("CreateAsync doesn't throw when disposed");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ObjectDisposedException);
            }
        }

        [TestMethod()]
        public void IsInRoleAsyncTest()
        {

            var target = Target();
            var user = new AzureTableUser
            {
                Id = "foo",
                UserName = "bar"
            };
            var role1 = "derp";
            var role2 = "herp";

            try
            {
                target.IsInRoleAsync(null, null).Wait();
                Assert.Fail("IsInRoleAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentNullException);
            }
            try
            {
                target.IsInRoleAsync(null, role1).Wait();
                Assert.Fail("IsInRoleAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentNullException);
            }
            try
            {
                target.IsInRoleAsync(user, null).Wait();
                Assert.Fail("IsInRoleAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentNullException);
            }
            try
            {
                target.IsInRoleAsync(user, "").Wait();
                Assert.Fail("IsInRoleAsync didn't throw on empty");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentException);
            }
            try
            {
                target.IsInRoleAsync(user, " ").Wait();
                Assert.Fail("IsInRoleAsync didn't throw on whitespace");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentException);
            }

            Assert.IsFalse(target.IsInRoleAsync(user, role1).Result);

            target.AddToRoleAsync(user, role1).Wait();
            target.AddToRoleAsync(user, role2).Wait();

            Assert.IsTrue(target.IsInRoleAsync(user, role1).Result);
            Assert.IsTrue(target.IsInRoleAsync(user, role2).Result);

            target.Dispose();

            try
            {
                target.IsInRoleAsync(user, role1).Wait();
                Assert.Fail("GetRolesAsync doesn't throw when disposed");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ObjectDisposedException);
            }
        }

        [TestMethod()]
        public void GetRolesAsyncTest()
        {
            var target = Target();
            var user = new AzureTableUser
            {
                Id = "foo",
                UserName = "bar"
            };
            var role1 = "derp";
            var role2 = "herp";

            try
            {
                target.GetRolesAsync(null).Wait();
                Assert.Fail("GetRolesAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentNullException);
            }


            var result = target.GetRolesAsync(user).Result;
            Assert.AreEqual(0, result.Count);

            target.AddToRoleAsync(user, role1).Wait();
            target.AddToRoleAsync(user, role2).Wait();


            result = target.GetRolesAsync(user).Result;
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(x => x.Equals(role1) || x.Equals(role2)));

            target.Dispose();

            try
            {
                target.GetRolesAsync(user).Wait();
                Assert.Fail("GetRolesAsync doesn't throw when disposed");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ObjectDisposedException);
            }
        }

        [TestMethod()]
        public void RemoveFromRoleAsyncTest()
        {

            var target = Target();
            var user = new AzureTableUser
            {
                Id = "foo",
                UserName = "bar"
            };
            var role1 = "derp";
            var role2 = "herp";

            try
            {
                target.RemoveFromRoleAsync(null, null).Wait();
                Assert.Fail("RemoveFromRoleAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentNullException);
            }
            try
            {
                target.RemoveFromRoleAsync(null, role1).Wait();
                Assert.Fail("RemoveFromRoleAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentNullException);
            }
            try
            {
                target.RemoveFromRoleAsync(user, null).Wait();
                Assert.Fail("RemoveFromRoleAsync didn't throw on null");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentNullException);
            }
            try
            {
                target.RemoveFromRoleAsync(user, "").Wait();
                Assert.Fail("RemoveFromRoleAsync didn't throw on empty");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentException);
            }
            try
            {
                target.RemoveFromRoleAsync(user, " ").Wait();
                Assert.Fail("RemoveFromRoleAsync didn't throw on whitespace");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ArgumentException);
            }
            target.RemoveFromRoleAsync(user, role1).Wait();

            target.AddToRoleAsync(user, role1).Wait();
            target.AddToRoleAsync(user, role2).Wait();

            Assert.IsTrue(target.IsInRoleAsync(user, role1).Result);
            Assert.IsTrue(target.IsInRoleAsync(user, role2).Result);

            target.RemoveFromRoleAsync(user, role1).Wait();
            Assert.IsFalse(target.IsInRoleAsync(user, role1).Result);
            Assert.IsTrue(target.IsInRoleAsync(user, role2).Result);
            target.RemoveFromRoleAsync(user, role2).Wait();
            Assert.IsFalse(target.IsInRoleAsync(user, role1).Result);
            Assert.IsFalse(target.IsInRoleAsync(user, role2).Result);

            target.Dispose();

            try
            {
                target.RemoveFromRoleAsync(user, role1).Wait();
                Assert.Fail("GetRolesAsync doesn't throw when disposed");
            }
            catch (AggregateException ex)
            {
                Assert.IsTrue(ex.InnerException is ObjectDisposedException);
            }
        }
        #endregion
    }
}
