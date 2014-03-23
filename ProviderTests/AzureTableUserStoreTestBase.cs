using System;
using StateStreetGang.AspNet.Identity.AzureTable;

namespace ProviderTests
{
    public abstract class AzureTableUserStoreTestBase
    {
        protected AzureTableUserStore<AzureTableUser> Target()
        {
            UtilsLol.DeleteTableLol(AzureTableUserStore<AzureTableUser>.DefaultUserTableName);
            UtilsLol.DeleteTableLol(AzureTableUserStore<AzureTableUser>.DefaultUserClaimTableName);
            UtilsLol.DeleteTableLol(AzureTableUserStore<AzureTableUser>.DefaultUserLoginTableName);
            UtilsLol.DeleteTableLol(AzureTableUserStore<AzureTableUser>.DefaultUserRoleTableName);
            return new TestStore(Properties.Settings.Default.TestTableConnectionString);
        }
    }

    public sealed class TestStore : AzureTableUserStore<AzureTableUser>
    {
        public TestStore(string connex)
            : base(connex)
        {
        }

        protected override string GetPartitionKeyByUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("userName cannot be null, empty, or consist of whitespace.");
            // IIAC this gives us a maximum of 2000 buckets (-1000 to 1000), which would mean roughly 2500 users
            // per partition for 5m users.  
            return AzureTableUser.DefaultAspNetUserPartitionKey + (GetHashCode() % 10000);
        }
    }
}