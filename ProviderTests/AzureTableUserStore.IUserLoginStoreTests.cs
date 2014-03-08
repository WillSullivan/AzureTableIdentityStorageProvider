using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StateStreetGang.AspNet.Identity.AzureTable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNet.Identity;

namespace ProviderTests
{
    [TestClass]
    public class AzureTableUserStoreIUserLoginStoreTests : AzureTableUserStoreTestBase
    {

        AzureTableUser _user = new AzureTableUser
        {
            Id = "derpLogin",
            UserName = "herp"
        };


        UserLoginInfo _login1 = new UserLoginInfo("providerOne", "keyOne");
        UserLoginInfo _login2 = new UserLoginInfo("providerTwo", "keyTwo");

        [TestMethod]
        public void AddLoginAsyncTests()
        {
            var target = Target();
            UtilsLol.AssertThrows(() => target.AddLoginAsync(null, null).Wait());

            UtilsLol.AssertThrows(() => target.AddLoginAsync(_user, null).Wait());
            target.AddLoginAsync(_user, _login1).Wait();
            target.AddLoginAsync(_user, _login2).Wait();
            var backAgain = target.GetLoginsAsync(_user).Result;
            Assert.AreEqual(2, backAgain.Count);
            Assert.IsTrue(backAgain.Any(x => x.LoginProvider == _login1.LoginProvider && x.ProviderKey == _login1.ProviderKey));
            Assert.IsTrue(backAgain.Any(x => x.LoginProvider == _login2.LoginProvider && x.ProviderKey == _login2.ProviderKey));
        }
        [TestMethod]
        public void FindAsyncTests()
        {
            var target = Target();
            UtilsLol.AssertThrows(() => target.FindAsync(null).Result);
            Assert.IsNull(target.FindAsync(_login1).Result);

            target.CreateAsync(_user).Wait();

            Assert.IsNull(target.FindAsync(_login1).Result);
            Assert.IsNull(target.FindAsync(_login2).Result);
            target.AddLoginAsync(_user, _login1).Wait();
            Assert.IsNull(target.FindAsync(_login2).Result);
            target.AddLoginAsync(_user, _login2).Wait();
            var backAgain = target.FindAsync(_login2).Result;
            Assert.IsTrue(backAgain.UserName == _user.UserName);
            backAgain = target.FindAsync(_login1).Result;
            Assert.IsTrue(backAgain.UserName == _user.UserName);
        }
        [TestMethod]
        public void GetLoginsAsyncTests()
        {
            var target = Target();
            UtilsLol.AssertThrows(() => target.GetLoginsAsync(null).Result);
            Assert.AreEqual(0, target.GetLoginsAsync(_user).Result.Count);

            target.CreateAsync(_user).Wait();

            Assert.AreEqual(0, target.GetLoginsAsync(_user).Result.Count);

            target.AddLoginAsync(_user, _login1).Wait();
            Assert.AreEqual(1, target.GetLoginsAsync(_user).Result.Count);
            target.AddLoginAsync(_user, _login2).Wait();
            var backAgain = target.GetLoginsAsync(_user).Result;
            Assert.AreEqual(2, backAgain.Count);
            Assert.IsTrue(backAgain.Any(x => x.LoginProvider == _login1.LoginProvider && x.ProviderKey == _login1.ProviderKey));
            Assert.IsTrue(backAgain.Any(x => x.LoginProvider == _login2.LoginProvider && x.ProviderKey == _login2.ProviderKey));
        }
        [TestMethod]
        public void RemoveLoginAsyncTests()
        {
            var target = Target();
            UtilsLol.AssertThrows(() => target.RemoveLoginAsync(null, null).Wait());

            target.CreateAsync(_user).Wait();

            UtilsLol.AssertThrows(() => target.RemoveLoginAsync(_user, null).Wait());

            target.AddLoginAsync(_user, _login1).Wait();
            target.AddLoginAsync(_user, _login2).Wait();
            var backAgain = target.GetLoginsAsync(_user).Result;
            Assert.AreEqual(2, backAgain.Count);
            Assert.IsTrue(backAgain.Any(x => x.LoginProvider == _login1.LoginProvider && x.ProviderKey == _login1.ProviderKey));
            Assert.IsTrue(backAgain.Any(x => x.LoginProvider == _login2.LoginProvider && x.ProviderKey == _login2.ProviderKey));

            target.RemoveLoginAsync(_user, _login1).Wait();

            backAgain = target.GetLoginsAsync(_user).Result;
            Assert.AreEqual(1, backAgain.Count);
            Assert.IsTrue(backAgain.First().LoginProvider == _login2.LoginProvider && backAgain.First().ProviderKey == _login2.ProviderKey);

            target.RemoveLoginAsync(_user, _login2).Wait();
            backAgain = target.GetLoginsAsync(_user).Result;
            Assert.AreEqual(0, backAgain.Count);
        }

    }

}


