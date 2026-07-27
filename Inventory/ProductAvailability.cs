using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class ProductAvailability : InventoryTestBase
    {
        #region "Product Availabilities"
        [Test]
        public async Task InventoryItem_ProductAvailabilities()
        {
            // Create an inventory item for the product availability routes.
            InventoryItem item = await CreateInventoryItemAsync("Product Availability Item");

            // Read all product availabilities for the inventory item.
            var productAvailabilities = await Client.GetAsync(new InventoryProductAvailabilitiesGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Read and update the first existing product availability when one is returned.
            if (productAvailabilities.Count > 0)
            {
                string warehouseId = ReadString(productAvailabilities[0], "LogicalWarehouseID");
                await Client.GetAsync(new InventoryProductAvailabilityGETRequest() { InventoryID = item.InventoryID, LogicalWarehouseID = warehouseId });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

                await Client.PatchAsync(new InventoryProductAvailabilityPATCHRequest() { InventoryID = item.InventoryID, LogicalWarehouseID = warehouseId });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            }

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}
