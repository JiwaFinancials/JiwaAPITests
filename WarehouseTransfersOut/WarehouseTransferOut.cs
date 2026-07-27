using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseTransferOutDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferOut;
using WarehouseTransferOutLineDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferOutLine;

namespace JiwaAPITests.WarehouseTransfersOut
{
    public class WarehouseTransferOut : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task WarehouseTransferOut_CRUD()
        {
            // Create an inventory item to transfer.
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Warehouse Transfer Out Test Item",
                DefaultPrice = 10.00M
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.InventoryID, Is.Not.Null.And.Not.Empty);

            // Read logical warehouses so valid source and destination warehouse IDs can be used.
            QueryResponse<IN_Logical> logicalWarehouses = await Client.GetAsync(new IN_LogicalQuery());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(logicalWarehouses.Results, Is.Not.Null);
            Assert.That(logicalWarehouses.Results.Count, Is.GreaterThan(1));

            IN_Logical sourceWarehouse = logicalWarehouses.Results.First();
            IN_Logical destinationWarehouse = logicalWarehouses.Results.First(x => x.IN_LogicalID != sourceWarehouse.IN_LogicalID);
            Assert.That(sourceWarehouse.IN_LogicalID, Is.Not.Null.And.Not.Empty);
            Assert.That(destinationWarehouse.IN_LogicalID, Is.Not.Null.And.Not.Empty);

            // Create a warehouse transfer out.
            WarehouseTransferOutPOSTRequest transferCreateReq = new WarehouseTransferOutPOSTRequest()
            {
                TransferDate = DateTime.Today,
                SourceWarehouseID = sourceWarehouse.IN_LogicalID,
                DestinationWarehouseID = destinationWarehouse.IN_LogicalID,
                Notes = "Warehouse transfer out " + RandomString(8),
                Lines = new List<WarehouseTransferOutLineDto>()
                {
                    new WarehouseTransferOutLineDto()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        PartNo = itemCreateRes.PartNo,
                        QuantityWanted = 1M
                    }
                }
            };

            WarehouseTransferOutDto transferCreateRes = await Client.PostAsync(transferCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(transferCreateRes.WarehouseTransferOutID, Is.Not.Null.And.Not.Empty);

            // Read the created warehouse transfer out.
            WarehouseTransferOutGETRequest transferGetReq = new WarehouseTransferOutGETRequest()
            {
                WarehouseTransferOutID = transferCreateRes.WarehouseTransferOutID
            };

            WarehouseTransferOutDto transferGetRes = await Client.GetAsync(transferGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(transferGetRes.WarehouseTransferOutID, Is.EqualTo(transferCreateRes.WarehouseTransferOutID));
            Assert.That(transferGetRes.SourceWarehouseID, Is.EqualTo(transferCreateReq.SourceWarehouseID));

            // Update the warehouse transfer out.
            WarehouseTransferOutPATCHRequest transferPatchReq = new WarehouseTransferOutPATCHRequest()
            {
                WarehouseTransferOutID = transferCreateRes.WarehouseTransferOutID,
                Notes = "Updated warehouse transfer out " + RandomString(8)
            };

            WarehouseTransferOutDto transferPatchRes = await Client.PatchAsync(transferPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(transferPatchRes.WarehouseTransferOutID, Is.EqualTo(transferCreateRes.WarehouseTransferOutID));
            Assert.That(transferPatchRes.Notes, Is.EqualTo(transferPatchReq.Notes));

            // Verify the warehouse transfer out was updated.
            transferGetRes = await Client.GetAsync(transferGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(transferGetRes.Notes, Is.EqualTo(transferPatchReq.Notes));

            // Cancel the warehouse transfer out.
            WarehouseTransferOutCANCELRequest transferDeleteReq = new WarehouseTransferOutCANCELRequest()
            {
                WarehouseTransferOutID = transferCreateRes.WarehouseTransferOutID
            };

            await Client.DeleteAsync(transferDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the warehouse transfer out was canceled.
            transferGetRes = await Client.GetAsync(transferGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(transferGetRes.Status, Is.EqualTo(JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferOutStatuses.Cancelled));
        }
        #endregion

        #region "{Activate}"
        [Test]
        public async Task WarehouseTransferOut_Activate()
        {
            // Create an inventory item to transfer.
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Warehouse Transfer Out Activate Test Item",
                DefaultPrice = 10.00M
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.InventoryID, Is.Not.Null.And.Not.Empty);

            // Read logical warehouses so valid source and destination warehouse IDs can be used.
            QueryResponse<IN_Logical> logicalWarehouses = await Client.GetAsync(new IN_LogicalQuery());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(logicalWarehouses.Results, Is.Not.Null);
            Assert.That(logicalWarehouses.Results.Count, Is.GreaterThan(1));

            IN_Logical sourceWarehouse = logicalWarehouses.Results.First();
            IN_Logical destinationWarehouse = logicalWarehouses.Results.First(x => x.IN_LogicalID != sourceWarehouse.IN_LogicalID);

            // Create a warehouse transfer out to activate.
            WarehouseTransferOutPOSTRequest transferCreateReq = new WarehouseTransferOutPOSTRequest()
            {
                TransferDate = DateTime.Today,
                SourceWarehouseID = sourceWarehouse.IN_LogicalID,
                DestinationWarehouseID = destinationWarehouse.IN_LogicalID,
                Notes = "Warehouse transfer out activate " + RandomString(8),
                Lines = new List<WarehouseTransferOutLineDto>()
                {
                    new WarehouseTransferOutLineDto()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        PartNo = itemCreateRes.PartNo,
                        QuantityWanted = 1M
                    }
                }
            };

            WarehouseTransferOutDto transferCreateRes = await Client.PostAsync(transferCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(transferCreateRes.WarehouseTransferOutID, Is.Not.Null.And.Not.Empty);

            // Activate the warehouse transfer out.
            WarehouseTransferOutACTIVATERequest transferActivateReq = new WarehouseTransferOutACTIVATERequest()
            {
                WarehouseTransferOutID = transferCreateRes.WarehouseTransferOutID
            };

            WarehouseTransferOutDto transferActivateRes = await Client.PostAsync(transferActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(transferActivateRes, Is.Not.Null);
            Assert.That(transferActivateRes.WarehouseTransferOutID, Is.EqualTo(transferCreateRes.WarehouseTransferOutID));
            Assert.That(transferActivateRes.Status, Is.Not.EqualTo(JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferOutStatuses.Entered));
        }
        #endregion
    }
}
