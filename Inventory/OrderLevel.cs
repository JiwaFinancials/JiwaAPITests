using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class OrderLevel : InventoryTestBase
    {
        #region "Order Levels"
        [Test]
        public async Task InventoryItem_OrderLevels()
        {
            // Create an inventory item for the order level routes.
            InventoryItem item = await CreateInventoryItemAsync("Order Level Item");

            // Read all order levels for the inventory item.
            var orderLevels = await Client.GetAsync(new InventoryOrderLevelsGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Read and update the first existing order level when one is returned.
            if (orderLevels.Count > 0)
            {
                string warehouseId = ReadString(orderLevels[0], "LogicalWarehouseID");
                int periodNo = ReadInt(orderLevels[0], "PeriodNo");
                await Client.GetAsync(new InventoryOrderLevelGETRequest() { InventoryID = item.InventoryID, LogicalWarehouseID = warehouseId, PeriodNo = periodNo });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

                await Client.PatchAsync(new InventoryOrderLevelPATCHRequest() { InventoryID = item.InventoryID, LogicalWarehouseID = warehouseId, PeriodNo = periodNo });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            }

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}
