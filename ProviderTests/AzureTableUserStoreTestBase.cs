using StateStreetGang.AspNet.Identity.AzureTable;
namespace ProviderTests
{
    public abstract class AzureTableUserStoreTestBase
    {
        protected AzureTableUserStore Target()
        {
            UtilsLol.DeleteTableLol(AzureTableUserStore.DefaultTableName);
            return new AzureTableUserStore(Properties.Settings.Default.TestTableConnectionString);
        }
    }
}