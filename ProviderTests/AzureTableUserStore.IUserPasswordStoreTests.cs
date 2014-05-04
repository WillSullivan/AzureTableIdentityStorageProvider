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
    public class AzureTableUserStoreIUserPasswordStoreTests : AzureTableUserStoreTestBase
    {
        [TestMethod]
        public void GetPasswordHashAsyncTests()
        {
            var target = Target();
            UtilsLol.AssertThrows(() => target.GetPasswordHashAsync(null).Result, typeof(ArgumentNullException));

            var user = new AzureTableUser
            {
                Id="derpPassword",
                UserName = "herp"
            };
            Assert.IsNull(target.GetPasswordHashAsync(user).Result);
            target.CreateAsync(user).Wait();
            Assert.IsNull(target.GetPasswordHashAsync(user).Result);
            // eh, oh well.
            var hash = "stupid or liar";
            target.SetPasswordHashAsync(user, hash).Wait();
            Assert.AreEqual(hash, target.GetPasswordHashAsync(user).Result);
        }
        [TestMethod]
        public void HasPasswordAsyncTests()
        {
            var target = Target();
            UtilsLol.AssertThrows(() => target.HasPasswordAsync(null).Result, typeof(ArgumentNullException));

            var user = new AzureTableUser
            {
                Id = "derp",
                UserName = "herp"
            };
            Assert.IsFalse(target.HasPasswordAsync(user).Result);
            target.CreateAsync(user);
            Assert.IsFalse(target.HasPasswordAsync(user).Result);

            var hash = "stupid or liar";
            target.SetPasswordHashAsync(user, hash).Wait();
            Assert.IsTrue(target.HasPasswordAsync(user).Result);
        }
        [TestMethod]
        public void SetPasswordHashAsyncTests()
        {
            var target = Target();
            UtilsLol.AssertThrows(() => target.SetPasswordHashAsync(null, null).Wait());
            UtilsLol.AssertThrows(() => target.SetPasswordHashAsync(null, "").Wait());
            UtilsLol.AssertThrows(() => target.SetPasswordHashAsync(null, "  ").Wait());
            UtilsLol.AssertThrows(() => target.SetPasswordHashAsync(null, "derp").Wait());

            var user = new AzureTableUser
            {
                Id = "derpHash",
                UserName = "herp"
            };
            UtilsLol.AssertThrows(() => target.SetPasswordHashAsync(user, null).Wait());
            UtilsLol.AssertThrows(() => target.SetPasswordHashAsync(user, "").Wait());
            UtilsLol.AssertThrows(() => target.SetPasswordHashAsync(user, "  ").Wait());
            target.CreateAsync(user).Wait();
            UtilsLol.AssertThrows(() => target.SetPasswordHashAsync(user, null).Wait());
            UtilsLol.AssertThrows(() => target.SetPasswordHashAsync(user, "").Wait());
            UtilsLol.AssertThrows(() => target.SetPasswordHashAsync(user, "  ").Wait());

            Assert.IsNull(target.GetPasswordHashAsync(user).Result);
            // eh, oh well.
            var hash = "stupid or liar";
            target.SetPasswordHashAsync(user, hash).Wait();
            Assert.AreEqual(hash, target.GetPasswordHashAsync(user).Result);
        }

    }

}


