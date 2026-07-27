using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class DebtorClassificationPrice : InventoryTestBase
    {
        #region "Debtor Classification Prices"
        [Test]
        public async Task InventoryItem_DebtorClassificationPrices()
        {
            // Create an inventory item for the debtor classification price routes.
            InventoryItem item = await CreateInventoryItemAsync("Debtor Classification Price Item");

            // Read all debtor classification prices for the inventory item.
            await Client.GetAsync(new InventoryDebtorClassificationPricesGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}
