using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class Budget : InventoryTestBase
    {
        #region "Budgets"
        [Test]
        public async Task InventoryItem_Budgets()
        {
            // Create an inventory item for the budget routes.
            InventoryItem item = await CreateInventoryItemAsync("Budget Item");

            // Read all budgets for the inventory item.
            var budgets = await Client.GetAsync(new InventoryBudgetsGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Read and update the first existing budget when one is returned.
            if (budgets.Count > 0)
            {
                string warehouseId = ReadString(budgets[0], "LogicalWarehouseID");
                int periodNo = ReadInt(budgets[0], "PeriodNo");
                await Client.GetAsync(new InventoryBudgetGETRequest() { InventoryID = item.InventoryID, LogicalWarehouseID = warehouseId, PeriodNo = periodNo });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

                await Client.PatchAsync(new InventoryBudgetPATCHRequest() { InventoryID = item.InventoryID, LogicalWarehouseID = warehouseId, PeriodNo = periodNo });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            }

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}
