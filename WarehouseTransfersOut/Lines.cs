using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using JiwaFinancials.Jiwa.JiwaServiceModel.WarehouseTransfers;
using JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders;
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
    public class Lines : JiwaAPITest
    {
        [Test]
        public async Task WarehouseTransferOutLines_DeleteInlineLine()
        {
            InventoryItem item1 = await Client.PostAsync(new InventoryPOSTRequest() { PartNo = RandomString(5), Description = "Item1", DefaultPrice = 10M });
            InventoryItem item2 = await Client.PostAsync(new InventoryPOSTRequest() { PartNo = RandomString(5), Description = "Item2", DefaultPrice = 10M });

            QueryResponse<IN_Logical> warehouses = await Client.GetAsync(new IN_LogicalQuery());
            IN_Logical src = warehouses.Results.First();
            IN_Logical dst = warehouses.Results.First(x => x.IN_LogicalID != src.IN_LogicalID);

            // Create transfer with TWO inline lines so we can delete one and keep one
            WarehouseTransferOutDto transfer = await Client.PostAsync(new WarehouseTransferOutPOSTRequest()
            {
                TransferDate = DateTime.Today,
                SourceWarehouseID = src.IN_LogicalID,
                DestinationWarehouseID = dst.IN_LogicalID,
                Notes = "Diag2 " + RandomString(8),
                Lines = new List<WarehouseTransferOutLineDto>()
                {
                    new WarehouseTransferOutLineDto() { InventoryID = item1.InventoryID, PartNo = item1.PartNo, QuantityWanted = 1M },
                    new WarehouseTransferOutLineDto() { InventoryID = item2.InventoryID, PartNo = item2.PartNo, QuantityWanted = 1M }
                }
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            string transferID = transfer.WarehouseTransferOutID;
            TestContext.Out.WriteLine($"Transfer has {transfer.Lines.Count} lines");

            // Get all lines
            List<WarehouseTransferOutLineDto> allLines = await Client.GetAsync(new WarehouseTransferOutLinesGETManyRequest() { WarehouseTransferOutID = transferID });
            TestContext.Out.WriteLine($"GET many returned {allLines.Count} lines");
            foreach (var l in allLines) TestContext.Out.WriteLine($"  Line: {l.WarehouseTransferOutLineID} PartNo={l.PartNo}");

            // Try to delete the second inline line
            WarehouseTransferOutLineDELETERequest lineDeleteReq = new WarehouseTransferOutLineDELETERequest()
            {
                WarehouseTransferOutID = transferID,
                WarehouseTransferOutLineID = allLines[1].WarehouseTransferOutLineID
            };

            await Client.DeleteAsync(lineDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Try to get the deleted line
            WarehouseTransferOutLineGETRequest lineGetReq = new WarehouseTransferOutLineGETRequest()
            {
                WarehouseTransferOutID = transferID,
                WarehouseTransferOutLineID = allLines[1].WarehouseTransferOutLineID
            };

            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(lineGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
    }
}