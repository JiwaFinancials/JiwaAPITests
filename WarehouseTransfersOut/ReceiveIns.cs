using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseTransferOutDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferOut;
using WarehouseTransferOutLineDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferOutLine;
using WarehouseTransferOutReceiveInDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferOutReceiveIn;

namespace JiwaAPITests.WarehouseTransfersOut
{
    public class ReceiveIns : JiwaAPITest
    {
        #region "{ReceiveIns}"
        [Test]
        public async Task WarehouseTransferOut_ReceiveIns_GETMany()
        {
            // Create resource
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Warehouse Transfer Out Receive Ins Test Item",
                DefaultPrice = 10.00M
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.InventoryID, Is.Not.Null.And.Not.Empty);

            // Create parent resource
            QueryResponse<IN_Logical> logicalWarehouses = await Client.GetAsync(new IN_LogicalQuery());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(logicalWarehouses.Results, Is.Not.Null);
            Assert.That(logicalWarehouses.Results.Count, Is.GreaterThan(1));

            IN_Logical sourceWarehouse = logicalWarehouses.Results.First();
            IN_Logical destinationWarehouse = logicalWarehouses.Results.First(x => x.IN_LogicalID != sourceWarehouse.IN_LogicalID);

            WarehouseTransferOutPOSTRequest transferCreateReq = new WarehouseTransferOutPOSTRequest()
            {
                TransferDate = DateTime.Today,
                SourceWarehouseID = sourceWarehouse.IN_LogicalID,
                DestinationWarehouseID = destinationWarehouse.IN_LogicalID,
                Notes = "Warehouse transfer out receive-ins " + RandomString(8),
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

            // Read child resource
            WarehouseTransferOutReceiveInsGETManyRequest receiveInsGetManyReq = new WarehouseTransferOutReceiveInsGETManyRequest()
            {
                WarehouseTransferOutID = transferCreateRes.WarehouseTransferOutID
            };

            List<WarehouseTransferOutReceiveInDto> receiveInsGetManyRes = await Client.GetAsync(receiveInsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(receiveInsGetManyRes, Is.Not.Null);
            Assert.That(receiveInsGetManyRes.All(x => x.WarehouseTransferOutID == transferCreateRes.WarehouseTransferOutID), Is.True);

            // Delete resource
            WarehouseTransferOutCANCELRequest transferDeleteReq = new WarehouseTransferOutCANCELRequest()
            {
                WarehouseTransferOutID = transferCreateRes.WarehouseTransferOutID
            };

            await Client.DeleteAsync(transferDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}
