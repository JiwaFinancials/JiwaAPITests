using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class SellingPrice : InventoryTestBase
    {
        #region "Selling Price"
        [Test]
        public async Task InventoryItem_SellingPrice()
        {
            // Create an inventory item for the selling price routes.
            InventoryItem item = await CreateInventoryItemAsync("Selling Price Item");

            // Read the selling price for the inventory item.
            await Client.GetAsync(new InventorySellingPriceGETRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Update the selling price for the inventory item.
            await Client.PatchAsync(new InventorySellingPricePATCHRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}
