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
using WarehouseTransferInLineDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferInLine;
using WarehouseTransferOutDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferOut;
using WarehouseTransferOutLineDto = JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers.WarehouseTransferOutLine;

namespace JiwaAPITests.WarehouseTransfersIn
{
    public class Lines : JiwaAPITest
    {
        private async Task<WarehouseTransferInDto> CreateWarehouseTransferInWithLineAsync()
        {
            // Create resource
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Warehouse Transfer In Lines Test Item",
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
                Notes = "Warehouse transfer out for transfer in lines " + RandomString(8),
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
                Notes = "Warehouse transfer in lines " + RandomString(8)
            };

            WarehouseTransferInDto transferInCreateRes = await Client.PostAsync(transferInCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(transferInCreateRes.WarehouseTransferInID, Is.Not.Null.And.Not.Empty);

            return transferInCreateRes;
        }

        #region "{Lines}"
        [Test]
        public async Task WarehouseTransferIn_Lines_ReadAndUpdate()
        {
            // Create parent resource
            WarehouseTransferInDto transferInCreateRes = await CreateWarehouseTransferInWithLineAsync();

            // Read child resource
            WarehouseTransferInLinesGETManyRequest linesGetManyReq = new WarehouseTransferInLinesGETManyRequest()
            {
                WarehouseTransferInID = transferInCreateRes.WarehouseTransferInID
            };

            List<WarehouseTransferInLineDto> linesGetManyRes = await Client.GetAsync(linesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linesGetManyRes.Count, Is.GreaterThan(0));

            WarehouseTransferInLineDto firstLine = linesGetManyRes.First();
            Assert.That(firstLine.WarehouseTransferInLineID, Is.Not.Null.And.Not.Empty);

            // Read child resource
            WarehouseTransferInLineGETRequest lineGetReq = new WarehouseTransferInLineGETRequest()
            {
                WarehouseTransferInID = transferInCreateRes.WarehouseTransferInID,
                WarehouseTransferInLineID = firstLine.WarehouseTransferInLineID
            };

            WarehouseTransferInLineDto lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.WarehouseTransferInLineID, Is.EqualTo(firstLine.WarehouseTransferInLineID));

            // Update child resource
            string updatedReference = "WTI-LINE-" + RandomString(8);
            WarehouseTransferInLinePATCHRequest linePatchReq = new WarehouseTransferInLinePATCHRequest()
            {
                WarehouseTransferInID = transferInCreateRes.WarehouseTransferInID,
                WarehouseTransferInLineID = firstLine.WarehouseTransferInLineID,
                Ref = updatedReference
            };

            WarehouseTransferInLineDto linePatchRes = await Client.PatchAsync(linePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linePatchRes.WarehouseTransferInLineID, Is.EqualTo(firstLine.WarehouseTransferInLineID));
            Assert.That(linePatchRes.Ref, Is.EqualTo(updatedReference));

            // Verify update
            lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.Ref, Is.EqualTo(updatedReference));
        }
        #endregion
    }
}
