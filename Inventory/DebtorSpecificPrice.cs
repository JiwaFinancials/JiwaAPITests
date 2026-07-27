using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class DebtorSpecificPrice : InventoryTestBase
    {
        #region "Debtor Specific Prices"
        [Test]
        public async Task InventoryItem_DebtorSpecificPrices()
        {
            // Create an inventory item for the debtor specific price routes.
            InventoryItem item = await CreateInventoryItemAsync("Debtor Specific Price Item");

            // Read all debtor specific prices for the inventory item.
            await Client.GetAsync(new InventoryDebtorSpecificPricesGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}
