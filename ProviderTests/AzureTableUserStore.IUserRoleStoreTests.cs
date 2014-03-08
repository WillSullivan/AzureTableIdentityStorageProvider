using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StateStreetGang.AspNet.Identity.AzureTable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProviderTests
{
    [TestClass]
    public class AzureTableUserStoreIUserRoleStoreTests : AzureTableUserStoreTestBase
    {
        #region role
        [TestMethod()]
        public void AddToRoleAsyncTest()
        {
            var target = Target();
            var user = new AzureTableUser
            {
                Id = "fooRole",
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


