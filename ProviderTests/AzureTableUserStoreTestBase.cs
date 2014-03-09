using StateStreetGang.AspNet.Identity.AzureTable;
namespace ProviderTests
{
    public abstract class AzureTableUserStoreTestBase
    {
        protected AzureTableUserStore<AzureTableUser> Target()
        {
            UtilsLol.DeleteTableLol(AzureTableUserStore<AzureTableUser>.DefaultTableName);
            return new AzureTableUserStore<AzureTableUser>(Properties.Settings.Default.TestTableConnectionString);
        }
    }
}