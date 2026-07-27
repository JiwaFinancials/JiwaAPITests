using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class Region : InventoryTestBase
    {
        #region "Regions"
        [Test]
        public async Task InventoryItem_Regions()
        {
            // Create an inventory item for the region routes.
            InventoryItem item = await CreateInventoryItemAsync("Region Item");

            // Read all regions for the inventory item.
            var regions = await Client.GetAsync(new InventoryRegionsGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Read and update the first existing region when one is returned.
            if (regions.Count > 0)
            {
                string regionName = ReadString(regions[0], "RegionName");
                await Client.GetAsync(new InventoryRegionGETRequest() { InventoryID = item.InventoryID, RegionName = regionName });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

                await Client.PatchAsync(new InventoryRegionPATCHRequest() { InventoryID = item.InventoryID, RegionName = regionName });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            }

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}
