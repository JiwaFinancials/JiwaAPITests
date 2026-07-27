using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class DebtorPriceGroupPrice : InventoryTestBase
    {
        #region "Debtor Price Group Prices"
        [Test]
        public async Task InventoryItem_DebtorPriceGroupPrices()
        {
            // Create an inventory item for the debtor price group routes.
            InventoryItem item = await CreateInventoryItemAsync("Debtor Price Group Item");

            // Read all debtor price group prices for the inventory item.
            await Client.GetAsync(new InventoryDebtorPriceGroupPricesGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}
