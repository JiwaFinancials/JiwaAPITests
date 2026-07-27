using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WarehouseTransferInDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferIn;
using WarehouseTransferInReceiveInDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferInReceiveIn;
using WarehouseTransferOutDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferOut;
using WarehouseTransferOutLineDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferOutLine;

namespace JiwaAPITests.WarehouseTransfersIn
{
    public class ReceiveIns : JiwaAPITest
    {
        private async Task<WarehouseTransferInDto> CreateWarehouseTransferInWithLineAsync()
        {
            // Create resource
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Warehouse Transfer In Receive Ins Test Item",
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

            WarehouseTransferOutPOSTRequest transferOutCreateReq = new WarehouseTransferOutPOSTRequest()
            {
                TransferDate = DateTime.Today,
                SourceWarehouseID = sourceWarehouse.IN_LogicalID,
                DestinationWarehouseID = destinationWarehouse.IN_LogicalID,
                Notes = "Warehouse transfer out for transfer in receive-ins " + RandomString(8),
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
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(transferOutCreateRes.WarehouseTransferOutID, Is.Not.Null.And.Not.Empty);

            WarehouseTransferInPOSTRequest transferInCreateReq = new WarehouseTransferInPOSTRequest()
            {
                WarehouseTransferOutID = transferOutCreateRes.WarehouseTransferOutID,
                Notes = "Warehouse transfer in receive-ins " + RandomString(8)
            };

            WarehouseTransferInDto transferInCreateRes = await Client.PostAsync(transferInCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(transferInCreateRes.WarehouseTransferInID, Is.Not.Null.And.Not.Empty);

            return transferInCreateRes;
        }

        #region "{ReceiveIns}"
        [Test]
        public async Task WarehouseTransferIn_ReceiveIns_GETMany()
        {
            // Create parent resource
            WarehouseTransferInDto transferInCreateRes = await CreateWarehouseTransferInWithLineAsync();

            // Read child resource
            WarehouseTransferInReceiveInsGETManyRequest receiveInsGetManyReq = new WarehouseTransferInReceiveInsGETManyRequest()
            {
                WarehouseTransferInID = transferInCreateRes.WarehouseTransferInID
            };

            List<WarehouseTransferInReceiveInDto> receiveInsGetManyRes = await Client.GetAsync(receiveInsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(receiveInsGetManyRes, Is.Not.Null);
            Assert.That(receiveInsGetManyRes.All(x => x.WarehouseTransferInID == transferInCreateRes.WarehouseTransferInID), Is.True);
        }
        #endregion
    }
}
