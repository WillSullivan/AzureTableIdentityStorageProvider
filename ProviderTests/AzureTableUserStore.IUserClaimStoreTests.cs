using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StateStreetGang.AspNet.Identity.AzureTable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNet.Identity;
using System.Security.Claims;

namespace ProviderTests
{
    [TestClass]
    public class AzureTableUserStoreIUserClaimStoreTests : AzureTableUserStoreTestBase
    {
        AzureTableUser _user = new AzureTableUser
        {
            Id = "derpClaim",
            UserName = "herp"
        };
        Claim _claim1 = new Claim("FirstClaim", "LOL", "Stupid", "UnitTests", "Will");
        Claim _claim2 = new Claim("SecondClaim", "@U", "Stupid", "UnitTests", "Will");

        [TestMethod]
        public void AddClaimAsyncTests()
        {
            var target = Target();
            UtilsLol.AssertThrows(() => target.AddClaimAsync(null, null).Wait());

            UtilsLol.AssertThrows(() => target.AddClaimAsync(_user, null).Wait());
            target.AddClaimAsync(_user, _claim1).Wait();
            var returned = target.GetClaimsAsync(_user).Result.Single();
            Assert.IsTrue(AreEquivalent(returned, _claim1));
            target.AddClaimAsync(_user, _claim2).Wait();
            var backAgain = target.GetClaimsAsync(_user).Result;
            Assert.AreEqual(2, backAgain.Count);
            AssertClaims(backAgain.ToArray());
        }
        [TestMethod]
        public void GetClaimsAsyncTests()
        {
            var target = Target();
            UtilsLol.AssertThrows(() => target.GetClaimsAsync(null).Result);
            Assert.AreEqual(0, target.GetClaimsAsync(_user).Result.Count);

            target.CreateAsync(_user).Wait();

            Assert.AreEqual(0, target.GetClaimsAsync(_user).Result.Count);

            target.AddClaimAsync(_user, _claim1).Wait();
            var first = target.GetClaimsAsync(_user).Result.Single();
            Assert.IsTrue(AreEquivalent(_claim1, first));
            target.AddClaimAsync(_user, _claim2).Wait();
            var backAgain = target.GetClaimsAsync(_user).Result;
            Assert.AreEqual(2, backAgain.Count);
            AssertClaims(backAgain.ToArray());
        }
        [TestMethod]
        public void RemoveClaimAsyncTests()
        {
            var target = Target();
            UtilsLol.AssertThrows(() => target.RemoveClaimAsync(null, null).Wait());

            target.CreateAsync(_user).Wait();

            UtilsLol.AssertThrows(() => target.RemoveClaimAsync(_user, null).Wait());

            target.AddClaimAsync(_user, _claim1).Wait();
            target.AddClaimAsync(_user, _claim2).Wait();
            var backAgain = target.GetClaimsAsync(_user).Result;
            Assert.AreEqual(2, backAgain.Count);
            Assert.AreEqual(2, backAgain.Count);
            AssertClaims(backAgain.ToArray());
           
            target.RemoveClaimAsync(_user, _claim2).Wait();
            var first = target.GetClaimsAsync(_user).Result.Single();
            Assert.IsTrue(AreEquivalent(_claim1, first));
            
            target.RemoveClaimAsync(_user, _claim1).Wait();
            Assert.AreEqual(0, target.GetClaimsAsync(_user).Result.Count);
        }

        private void AssertClaims(params Claim[] returnedClaims)
        {
            Assert.IsTrue(returnedClaims.Any(x => AreEquivalent(x, _claim1)));
            Assert.IsTrue(returnedClaims.Any(x => AreEquivalent(x, _claim2)));
        }

        private bool AreEquivalent(Claim first, Claim second)
        {
            return first.Issuer == second.Issuer &&
                   first.OriginalIssuer == second.OriginalIssuer &&
                   first.Type == second.Type &&
                   first.Value == second.Value &&
                   first.ValueType == second.ValueType;
        }
    }

}


