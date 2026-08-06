using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WarehouseTransferInDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferIn;
using WarehouseTransferOutDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferOut;
using WarehouseTransferOutLineDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferOutLine;

namespace JiwaAPITests.WarehouseTransfersIn
{
    public class WarehouseTransferIn : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task WarehouseTransferIn_CRU()
        {
            // Create resource
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Warehouse Transfer In Test Item",
                DefaultPrice = 10.00M
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(itemCreateRes.InventoryID, Is.Not.Null.And.Not.Empty);

            // Create parent resource
            QueryResponse<IN_Logical> logicalWarehouses = await Client.GetAsync(new IN_LogicalQuery());
            Assert.That(LastHttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(logicalWarehouses.Results, Is.Not.Null);
            Assert.That(logicalWarehouses.Results.Count, Is.GreaterThan(1));

            IN_Logical sourceWarehouse = logicalWarehouses.Results.First();
            IN_Logical destinationWarehouse = logicalWarehouses.Results.First(x => x.IN_LogicalID != sourceWarehouse.IN_LogicalID);

            WarehouseTransferOutPOSTRequest transferOutCreateReq = new WarehouseTransferOutPOSTRequest()
            {
                TransferDate = DateTime.Today,
                SourceWarehouseID = sourceWarehouse.IN_LogicalID,
                DestinationWarehouseID = destinationWarehouse.IN_LogicalID,
                Notes = "Warehouse transfer out for transfer in " + RandomString(8),
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

            WarehouseTransferOutDto transferOutCreateRes = await Client.PostAsync(transferOutCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(transferOutCreateRes.WarehouseTransferOutID, Is.Not.Null.And.Not.Empty);

            // Create resource
            WarehouseTransferInPOSTRequest transferCreateReq = new WarehouseTransferInPOSTRequest()
            {
                WarehouseTransferOutID = transferOutCreateRes.WarehouseTransferOutID,
                Notes = "Warehouse transfer in " + RandomString(8)
            };

            WarehouseTransferInDto transferCreateRes = await Client.PostAsync(transferCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(transferCreateRes.WarehouseTransferInID, Is.Not.Null.And.Not.Empty);

            // Read resource
            WarehouseTransferInGETRequest transferGetReq = new WarehouseTransferInGETRequest()
            {
                WarehouseTransferInID = transferCreateRes.WarehouseTransferInID
            };

            WarehouseTransferInDto transferGetRes = await Client.GetAsync(transferGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(transferGetRes.WarehouseTransferInID, Is.EqualTo(transferCreateRes.WarehouseTransferInID));

            // Update resource
            WarehouseTransferInPATCHRequest transferPatchReq = new WarehouseTransferInPATCHRequest()
            {
                WarehouseTransferInID = transferCreateRes.WarehouseTransferInID,
                Notes = "Updated warehouse transfer in " + RandomString(8)
            };

            WarehouseTransferInDto transferPatchRes = await Client.PatchAsync(transferPatchReq);
            Assert.That(LastHttpStatusCode, Is.AnyOf(HttpStatusCode.OK, HttpStatusCode.Created));
            Assert.That(transferPatchRes.WarehouseTransferInID, Is.EqualTo(transferCreateRes.WarehouseTransferInID));
            Assert.That(transferPatchRes.Notes, Is.EqualTo(transferPatchReq.Notes));

            // Verify update
            transferGetRes = await Client.GetAsync(transferGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(transferPatchRes.WarehouseTransferInID, Is.EqualTo(transferPatchReq.WarehouseTransferInID));
            Assert.That(transferGetRes.Notes, Is.EqualTo(transferPatchReq.Notes));
        }
        #endregion

        #region "{Activate}"
        [Test]
        public async Task WarehouseTransferIn_Activate()
        {
            // Create resource
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Warehouse Transfer In Activate Test Item",
                DefaultPrice = 10.00M
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(itemCreateRes.InventoryID, Is.Not.Null.And.Not.Empty);

            // Create parent resource
            QueryResponse<IN_Logical> logicalWarehouses = await Client.GetAsync(new IN_LogicalQuery());
            Assert.That(LastHttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(logicalWarehouses.Results, Is.Not.Null);
            Assert.That(logicalWarehouses.Results.Count, Is.GreaterThan(1));

            IN_Logical sourceWarehouse = logicalWarehouses.Results.First();
            IN_Logical destinationWarehouse = logicalWarehouses.Results.First(x => x.IN_LogicalID != sourceWarehouse.IN_LogicalID);

            // Create parent resource
            WarehouseTransferOutPOSTRequest transferOutCreateReq = new WarehouseTransferOutPOSTRequest()
            {
                TransferDate = DateTime.Today,
                SourceWarehouseID = sourceWarehouse.IN_LogicalID,
                DestinationWarehouseID = destinationWarehouse.IN_LogicalID,
                Notes = "Warehouse transfer out for transfer in activate " + RandomString(8),
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

            WarehouseTransferOutDto transferOutCreateRes = await Client.PostAsync(transferOutCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(transferOutCreateRes.WarehouseTransferOutID, Is.Not.Null.And.Not.Empty);

            // Create resource
            WarehouseTransferInPOSTRequest transferCreateReq = new WarehouseTransferInPOSTRequest()
            {
                WarehouseTransferOutID = transferOutCreateRes.WarehouseTransferOutID,
                Notes = "Warehouse transfer in activate " + RandomString(8)
            };

            WarehouseTransferInDto transferCreateRes = await Client.PostAsync(transferCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(transferCreateRes.WarehouseTransferInID, Is.Not.Null.And.Not.Empty);

            // Activate resource
            WarehouseTransferInACTIVATERequest transferActivateReq = new WarehouseTransferInACTIVATERequest()
            {
                WarehouseTransferInID = transferCreateRes.WarehouseTransferInID
            };

            WarehouseTransferInDto transferActivateRes = await Client.PostAsync(transferActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(transferActivateRes, Is.Not.Null);
            Assert.That(transferActivateRes.WarehouseTransferInID, Is.EqualTo(transferCreateRes.WarehouseTransferInID));

            // Verify activation
            WarehouseTransferInGETRequest transferGetReq = new WarehouseTransferInGETRequest()
            {
                WarehouseTransferInID = transferCreateRes.WarehouseTransferInID
            };

            WarehouseTransferInDto transferGetRes = await Client.GetAsync(transferGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(transferGetRes.Status, Is.Not.EqualTo(JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferInStatuses.Entered));
        }
        #endregion
    }
}


