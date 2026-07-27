using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class CrossSell : InventoryTestBase
    {
        #region "Cross Sells"
        [Test]
        public async Task InventoryItem_CrossSells()
        {
            // Create inventory items for the cross-sell routes.
            InventoryItem item = await CreateInventoryItemAsync("Cross Sell Parent");
            InventoryItem linkedItem = await CreateInventoryItemAsync("Cross Sell Linked");

            // Read all cross sells for the inventory item.
            var crossSells = await Client.GetAsync(new InventoryCrossSellsGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Read and update the first existing cross sell when one is returned.
            if (crossSells.Count > 0)
            {
                string crossSellId = ReadString(crossSells[0], "CrossSellID");
                await Client.GetAsync(new InventoryCrossSellGETRequest() { InventoryID = item.InventoryID, CrossSellID = crossSellId });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

                await Client.PatchAsync(new InventoryCrossSellPATCHRequest() { InventoryID = item.InventoryID, CrossSellID = crossSellId });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            }

            // Create a cross sell for the inventory item.
            await Client.PostAsync(new InventoryCrossSellPOSTRequest() { InventoryID = item.InventoryID, CrossSellInventoryID = linkedItem.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Delete the temporary inventory items.
            await DeleteInventoryItemAsync(item.InventoryID);
            await DeleteInventoryItemAsync(linkedItem.InventoryID);
        }
        #endregion
    }
}

