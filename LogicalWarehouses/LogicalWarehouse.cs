using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.LogicalWarehouses
{
    public class LogicalWarehouse : JiwaAPITest
    {
        #region "LogicalWarehouses_Current"
        [Test]
        public async Task LogicalWarehouses_Current_GET()
        {
            // Read the current logical warehouse.
            IN_Logical currentLogicalWarehouse = await Client.GetAsync(new LogicalWarehousesCurrentGETRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(currentLogicalWarehouse, Is.Not.Null);
            Assert.That(currentLogicalWarehouse.IN_LogicalID, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public async Task LogicalWarehouses_Current_PATCH()
        {
            // Read the current logical warehouse before changing it.
            IN_Logical originalCurrentLogicalWarehouse = await Client.GetAsync(new LogicalWarehousesCurrentGETRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(originalCurrentLogicalWarehouse, Is.Not.Null);
            Assert.That(originalCurrentLogicalWarehouse.IN_LogicalID, Is.Not.Null.And.Not.Empty);

            // Read all logical warehouses so the current warehouse can be set to a valid warehouse.
            ServiceStack.QueryResponse<IN_Logical> logicalWarehouses = await Client.GetAsync(new IN_LogicalQuery());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(logicalWarehouses.Results, Is.Not.Null);
            Assert.That(logicalWarehouses.Results.Count, Is.GreaterThan(0));

            IN_Logical targetLogicalWarehouse = logicalWarehouses.Results.FirstOrDefault(x => x.IN_LogicalID != originalCurrentLogicalWarehouse.IN_LogicalID) ?? originalCurrentLogicalWarehouse;
            Assert.That(targetLogicalWarehouse.IN_LogicalID, Is.Not.Null.And.Not.Empty);

            // Set the current logical warehouse.
            LogicalWarehousesCurrentPATCHRequest logicalWarehousePatchReq = new LogicalWarehousesCurrentPATCHRequest()
            {
                IN_LogicalID = targetLogicalWarehouse.IN_LogicalID
            };

            IN_Logical patchedLogicalWarehouse = await Client.PatchAsync(logicalWarehousePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(patchedLogicalWarehouse.IN_LogicalID, Is.EqualTo(logicalWarehousePatchReq.IN_LogicalID));
            Assert.That(patchedLogicalWarehouse.IN_LogicalID, Is.EqualTo(targetLogicalWarehouse.IN_LogicalID));

            // Read the current logical warehouse after the change.
            IN_Logical currentLogicalWarehouse = await Client.GetAsync(new LogicalWarehousesCurrentGETRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(currentLogicalWarehouse.IN_LogicalID, Is.EqualTo(targetLogicalWarehouse.IN_LogicalID));

            if (originalCurrentLogicalWarehouse.IN_LogicalID != targetLogicalWarehouse.IN_LogicalID)
            {
                // Restore the original current logical warehouse.
                IN_Logical restoredLogicalWarehouse = await Client.PatchAsync(new LogicalWarehousesCurrentPATCHRequest()
                {
                    IN_LogicalID = originalCurrentLogicalWarehouse.IN_LogicalID
                });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(restoredLogicalWarehouse.IN_LogicalID, Is.EqualTo(originalCurrentLogicalWarehouse.IN_LogicalID));

                // Verify the original current logical warehouse was restored.
                currentLogicalWarehouse = await Client.GetAsync(new LogicalWarehousesCurrentGETRequest());
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(currentLogicalWarehouse.IN_LogicalID, Is.EqualTo(originalCurrentLogicalWarehouse.IN_LogicalID));
            }
        }
        #endregion
    }
}

