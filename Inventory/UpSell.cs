using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class UpSell : InventoryTestBase
    {
        #region "Up Sells"
        [Test]
        public async Task InventoryItem_UpSells()
        {
            // Create inventory items for the up-sell routes.
            InventoryItem item = await CreateInventoryItemAsync("Up Sell Parent");
            InventoryItem linkedItem = await CreateInventoryItemAsync("Up Sell Linked");

            // Read all up sells for the inventory item.
            await Client.GetAsync(new InventoryUpSellsGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Create an up sell for the inventory item.
            await Client.PostAsync(new InventoryUpSellPOSTRequest() { InventoryID = item.InventoryID, UpSellInventoryID = linkedItem.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Delete the temporary inventory items.
            await DeleteInventoryItemAsync(item.InventoryID);
            await DeleteInventoryItemAsync(linkedItem.InventoryID);
        }
        #endregion
    }
}

