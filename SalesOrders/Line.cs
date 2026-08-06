using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.SalesOrders
{
    public class Line : JiwaAPITest
    {
        private async Task<(JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder salesOrder, InventoryItem item, Debtor debtor)> CreateSalesOrderWithLineAsync()
        {
            // Create an inventory item
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "History Test Item",
                DefaultPrice = 10.00M
            };
            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a debtor
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "History Test Debtor"
            };
            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a sales order with one line
            SalesOrderPOSTRequest soCreateReq = new SalesOrderPOSTRequest()
            {
                DebtorAccountNo = debtorCreateReq.AccountNo,
                InvoiceInitDate = DateTime.Today,
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        QuantityOrdered = 1
                    }
                }
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder soCreateRes = await Client.PostAsync(soCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(soCreateRes.InvoiceID, Is.Not.Null);

            return (soCreateRes, itemCreateRes, debtorCreateRes);
        }

        #region "{Lines}"
        [Test]
        public async Task SalesOrderHistory_Lines_CRUD()
        {
            // Create a sales order with an initial line to operate against
            var (salesOrder, item, _) = await CreateSalesOrderWithLineAsync();
            string invoiceID = salesOrder.InvoiceID;
            string invoiceHistoryID = salesOrder.Histories[0].InvoiceHistoryID;

            // Read all lines for the history
            SalesOrderLinesGETManyRequest linesGetManyReq = new SalesOrderLinesGETManyRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID
            };
            List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine> linesGetManyRes = await Client.GetAsync(linesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linesGetManyRes.Count, Is.GreaterThanOrEqualTo(1));

            // Append a new line to the sales order history
            SalesOrderLinePOSTRequest lineCreateReq = new SalesOrderLinePOSTRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                InventoryID = item.InventoryID,
                QuantityOrdered = 3
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine lineCreateRes = await Client.PostAsync(lineCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(lineCreateRes.InvoiceLineID, Is.Not.Null);
            Assert.That(lineCreateRes.QuantityOrdered, Is.EqualTo(lineCreateReq.QuantityOrdered));

            // Read the created line
            SalesOrderLineGETRequest lineGetReq = new SalesOrderLineGETRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                InvoiceLineID = lineCreateRes.InvoiceLineID
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.InvoiceLineID, Is.EqualTo(lineCreateRes.InvoiceLineID));
            Assert.That(lineGetRes.QuantityOrdered, Is.EqualTo(lineCreateReq.QuantityOrdered));

            // Update the line
            SalesOrderLinePATCHRequest linePatchReq = new SalesOrderLinePATCHRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                InvoiceLineID = lineCreateRes.InvoiceLineID,
                QuantityOrdered = 8,
                DiscountedPrice = 9.99M
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine linePatchRes = await Client.PatchAsync(linePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linePatchRes.QuantityOrdered, Is.EqualTo(linePatchReq.QuantityOrdered));
            Assert.That(linePatchRes.DiscountedPrice, Is.EqualTo(linePatchReq.DiscountedPrice));

            // Read the updated line and confirm the changes were saved
            lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.QuantityOrdered, Is.EqualTo(linePatchReq.QuantityOrdered));
            Assert.That(lineGetRes.DiscountedPrice, Is.EqualTo(linePatchReq.DiscountedPrice));

            // Delete the line
            SalesOrderLineDELETERequest lineDeleteReq = new SalesOrderLineDELETERequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                InvoiceLineID = lineCreateRes.InvoiceLineID
            };
            await Client.DeleteAsync(lineDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the line was deleted
            WebServiceException exLine = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine deletedLineGetRes = await Client.GetAsync(lineGetReq);
            });
            Assert.That(exLine.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task SalesOrderHistory_Lines_CRUD_CommentLine()
        {
            // Create a sales order with an initial line to operate against
            var (salesOrder, _, _) = await CreateSalesOrderWithLineAsync();
            string invoiceID = salesOrder.InvoiceID;
            string invoiceHistoryID = salesOrder.Histories[0].InvoiceHistoryID;

            // Read all lines for the history
            SalesOrderLinesGETManyRequest linesGetManyReq = new SalesOrderLinesGETManyRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID
            };
            List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine> linesGetManyRes = await Client.GetAsync(linesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linesGetManyRes.Count, Is.GreaterThanOrEqualTo(1));

            // Append a new comment line to the sales order history
            SalesOrderLinePOSTRequest lineCreateReq = new SalesOrderLinePOSTRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                CommentLine = true,
                CommentText = "Initial history comment line"
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine lineCreateRes = await Client.PostAsync(lineCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(lineCreateRes.InvoiceLineID, Is.Not.Null);
            Assert.That(lineCreateRes.CommentLine, Is.True);
            Assert.That(lineCreateRes.CommentText, Is.EqualTo(lineCreateReq.CommentText));

            // Read the created line
            SalesOrderLineGETRequest lineGetReq = new SalesOrderLineGETRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                InvoiceLineID = lineCreateRes.InvoiceLineID
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.InvoiceLineID, Is.EqualTo(lineCreateRes.InvoiceLineID));
            Assert.That(lineGetRes.CommentLine, Is.True);
            Assert.That(lineGetRes.CommentText, Is.EqualTo(lineCreateReq.CommentText));

            // Update the line
            SalesOrderLinePATCHRequest linePatchReq = new SalesOrderLinePATCHRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                InvoiceLineID = lineCreateRes.InvoiceLineID,
                CommentLine = true,
                CommentText = "Updated history comment line"
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine linePatchRes = await Client.PatchAsync(linePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linePatchRes.CommentLine, Is.True);
            Assert.That(linePatchRes.CommentText, Is.EqualTo(linePatchReq.CommentText));

            // Read the updated line and confirm the changes were saved
            lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.CommentLine, Is.True);
            Assert.That(lineGetRes.CommentText, Is.EqualTo(linePatchReq.CommentText));

            // Delete the line
            SalesOrderLineDELETERequest lineDeleteReq = new SalesOrderLineDELETERequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                InvoiceLineID = lineCreateRes.InvoiceLineID
            };
            await Client.DeleteAsync(lineDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the line was deleted
            WebServiceException exLine = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine deletedLineGetRes = await Client.GetAsync(lineGetReq);
            });
            Assert.That(exLine.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "{Lines/LineDetails}"
        [Test]
        public async Task SalesOrderHistory_Lines_LineDetails_CRUD()
        {
            // Create a serialised inventory item
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Serialised Item for History LineDetail Test",
                DefaultPrice = 50.00M,
                UseSerialNo = true
            };
            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a debtor
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "History LineDetail Test Debtor"
            };
            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Transfer in serialised stock with two serial numbers
            string serialNo1 = RandomString(5);
            string serialNo2 = RandomString(5);
            StockTransferPOSTRequest transferCreateReq = new StockTransferPOSTRequest()
            {
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers.StockTransferLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers.StockTransferLine()
                    {
                        FromInventoryPartNo = "External",
                        ToInventoryPartNo = itemCreateReq.PartNo,
                        TransferQuantity = 1,
                        ToPartSerialNo = serialNo1
                    },
                    new JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers.StockTransferLine()
                    {
                        FromInventoryPartNo = "External",
                        ToInventoryPartNo = itemCreateReq.PartNo,
                        TransferQuantity = 1,
                        ToPartSerialNo = serialNo2
                    }
                }
            };
            StockTransfer transferCreateRes = await Client.PostAsync(transferCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Activate the transfer to make the serial numbers available
            StockTransferACTIVATERequest transferActivateReq = new StockTransferACTIVATERequest() { TransferID = transferCreateRes.TransferID };
            await Client.PostAsync(transferActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a sales order with the first serial number selected
            SalesOrderPOSTRequest soCreateReq = new SalesOrderPOSTRequest()
            {
                DebtorAccountNo = debtorCreateReq.AccountNo,
                InvoiceInitDate = DateTime.Today,
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        QuantityOrdered = 1,
                        LineDetails = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail>()
                        {
                            new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail()
                            {
                                SerialNo = serialNo1,
                                Quantity = 1
                            }
                        }
                    }
                }
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder soCreateRes = await Client.PostAsync(soCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(soCreateRes.InvoiceID, Is.Not.Null);

            string invoiceID = soCreateRes.InvoiceID;
            string invoiceHistoryID = soCreateRes.Histories[0].InvoiceHistoryID;
            string invoiceLineID = soCreateRes.Lines[0].InvoiceLineID;
            string existingLineDetailID = soCreateRes.Lines[0].LineDetails[0].LineDetailID;

            // Read all line details for the line
            SalesOrderLineDetailsGETManyRequest lineDetailsGetManyReq = new SalesOrderLineDetailsGETManyRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                InvoiceLineID = invoiceLineID
            };
            List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail> lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes.Any(x => x.LineDetailID == existingLineDetailID), Is.True);

            // Read the existing line detail
            SalesOrderLineDetailGETRequest lineDetailGetReq = new SalesOrderLineDetailGETRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                InvoiceLineID = invoiceLineID,
                LineDetailID = existingLineDetailID
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail lineDetailGetRes = await Client.GetAsync(lineDetailGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailGetRes.LineDetailID, Is.EqualTo(existingLineDetailID));
            Assert.That(lineDetailGetRes.SerialNo, Is.EqualTo(serialNo1));

            // Append the second serial number as an additional line detail
            SalesOrderLineDetailPOSTRequest lineDetailCreateReq = new SalesOrderLineDetailPOSTRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                InvoiceLineID = invoiceLineID,
                SerialNo = serialNo2,
                Quantity = 1
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail lineDetailCreateRes = await Client.PostAsync(lineDetailCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(lineDetailCreateRes.LineDetailID, Is.Not.Null);
            Assert.That(lineDetailCreateRes.SerialNo, Is.EqualTo(serialNo2));

            // Update the newly created line detail
            SalesOrderLineDetailPATCHRequest lineDetailPatchReq = new SalesOrderLineDetailPATCHRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                InvoiceLineID = invoiceLineID,
                LineDetailID = lineDetailCreateRes.LineDetailID,
                SerialNo = serialNo2,
                Quantity = 1
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail lineDetailPatchRes = await Client.PatchAsync(lineDetailPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailPatchRes.LineDetailID, Is.EqualTo(lineDetailCreateRes.LineDetailID));
            Assert.That(lineDetailPatchRes.SerialNo, Is.EqualTo(lineDetailPatchReq.SerialNo));

            // Read the updated line detail to confirm
            lineDetailGetReq.LineDetailID = lineDetailCreateRes.LineDetailID;
            lineDetailGetRes = await Client.GetAsync(lineDetailGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailGetRes.SerialNo, Is.EqualTo(lineDetailPatchReq.SerialNo));

            // Delete the added line detail
            SalesOrderLineDetailDELETERequest lineDetailDeleteReq = new SalesOrderLineDetailDELETERequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                InvoiceLineID = invoiceLineID,
                LineDetailID = lineDetailCreateRes.LineDetailID
            };
            await Client.DeleteAsync(lineDetailDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the deleted line detail is no longer accessible
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail deletedDetailGetRes = await Client.GetAsync(lineDetailGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}

