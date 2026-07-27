using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockTransferDto = JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers.StockTransfer;
using StockTransferLineDto = JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers.StockTransferLine;
using StockTransferStatuses = JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers.Statuses;

namespace JiwaAPITests.StockTransfers
{
    public class StockTransfer : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task StockTransfer_CRD()
        {
            // Create an inventory item to use as the transfer destination.
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Stock Transfer Test Item",
                DefaultPrice = 15.00M
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.InventoryID, Is.Not.Null);

            // Create a stock transfer.
            StockTransferPOSTRequest transferCreateReq = new StockTransferPOSTRequest()
            {
                Reference = "Stock Transfer " + RandomString(8),
                TransferDate = DateTime.Today,
                Lines = new List<StockTransferLineDto>()
                {
                    new StockTransferLineDto()
                    {
                        FromInventoryPartNo = "External",
                        ToInventoryPartNo = itemCreateRes.PartNo,
                        TransferQuantity = 1
                    }
                }
            };

            StockTransferDto transferCreateRes = await Client.PostAsync(transferCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(transferCreateRes.TransferID, Is.Not.Null);

            // Read the created stock transfer.
            StockTransferGETRequest transferGetReq = new StockTransferGETRequest()
            {
                TransferID = transferCreateRes.TransferID
            };

            StockTransferDto transferGetRes = await Client.GetAsync(transferGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(transferGetRes.TransferID, Is.EqualTo(transferCreateRes.TransferID));
            Assert.That(transferGetRes.Lines.Count, Is.EqualTo(1));

            // Delete the stock transfer.
            StockTransferDELETERequest transferDeleteReq = new StockTransferDELETERequest()
            {
                TransferID = transferCreateRes.TransferID
            };

            await Client.DeleteAsync(transferDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the stock transfer was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                StockTransferDto deletedTransferGetRes = await Client.GetAsync(transferGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "{Activate}"
        [Test]
        public async Task StockTransfer_Activate()
        {
            // Create an inventory item to use as the transfer destination.
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Stock Transfer Activate Test Item",
                DefaultPrice = 25.00M
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.InventoryID, Is.Not.Null);

            // Create a stock transfer to activate.
            StockTransferPOSTRequest transferCreateReq = new StockTransferPOSTRequest()
            {
                Lines = new List<StockTransferLineDto>()
                {
                    new StockTransferLineDto()
                    {
                        FromInventoryPartNo = "External",
                        ToInventoryPartNo = itemCreateRes.PartNo,
                        TransferQuantity = 1
                    }
                }
            };

            StockTransferDto transferCreateRes = await Client.PostAsync(transferCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(transferCreateRes.TransferID, Is.Not.Null);

            // Activate the stock transfer.
            StockTransferACTIVATERequest transferActivateReq = new StockTransferACTIVATERequest()
            {
                TransferID = transferCreateRes.TransferID
            };

            StockTransferDto transferActivateRes = await Client.PostAsync(transferActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            if (transferActivateRes != null)
            {
                Assert.That(transferActivateRes.Status, Is.EqualTo(StockTransferStatuses.Activated));
            }
        }
        #endregion
    }
}
