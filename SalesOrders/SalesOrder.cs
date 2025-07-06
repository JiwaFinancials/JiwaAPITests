using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using JiwaFinancials.Jiwa.JiwaServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ServiceStack.Diagnostics.Events;
using NUnit.Framework;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using ServiceStack;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers;

namespace JiwaAPITests.SalesOrders
{
    [TestFixture]
    class SalesOrder : JiwaAPITest
    {
        #region "{Main}"
        [Test]       
        public async Task SalesOrder_CRUD()
        {
            // Create an item
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest() 
            { 
                PartNo = RandomString(5), 
                Description = "Item Test",
                DefaultPrice = 125.67M 
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.PartNo, Is.EqualTo(itemCreateReq.PartNo));
            Assert.That(itemCreateRes.InventoryID, !Is.Null);

            // Create a debtor
            DebtorPOSTRequest accountCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test"                
            };

            Debtor accountCreateRes = await Client.PostAsync(accountCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(accountCreateRes.AccountNo, Is.EqualTo(accountCreateReq.AccountNo));
            Assert.That(accountCreateRes.DebtorID, !Is.Null);

            // Create a sales order
            SalesOrderPOSTRequest salesOrderCreateReq = new SalesOrderPOSTRequest()
            {
                DebtorAccountNo = accountCreateReq.AccountNo,
                InvoiceInitDate = DateTime.Today.Date,
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine() 
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        QuantityOrdered = 5
                    }
                }
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder salesOrderCreateRes = await Client.PostAsync(salesOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(salesOrderCreateRes.InvoiceID, !Is.Null);

            // Read the created sales order using the InvoiceID
            SalesOrderGETRequest salesOrderGetReq = new SalesOrderGETRequest() { InvoiceID = salesOrderCreateRes.InvoiceID };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder salesOrderGetRes = await Client.GetAsync(salesOrderGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(salesOrderGetRes.InvoiceNo, Is.EqualTo(salesOrderCreateRes.InvoiceNo));
            Assert.That(salesOrderGetRes.Lines.Count, Is.EqualTo(1));

            // Update the sales order - including adding another line
            SalesOrderPATCHRequest salesOrderPatchReq = new SalesOrderPATCHRequest() 
            { 
                InvoiceID = salesOrderCreateRes.InvoiceID, 
                SOReference = "Updated sales order Test",
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        Description = "second line added of same part",
                        QuantityOrdered = 11,
                        DiscountedPrice = 100.00M
                    }
                }
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder salesOrderPatchRes =  await Client.PatchAsync(salesOrderPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(salesOrderPatchRes.Lines.Count, Is.EqualTo(2));
            Assert.That(salesOrderPatchRes.Lines[1].DiscountedPrice, Is.EqualTo(salesOrderPatchReq.Lines[0].DiscountedPrice));

            // Patch the second sales order line
            SalesOrderLinePATCHRequest salesOrderLinePatchReq = new SalesOrderLinePATCHRequest()
            {
                InvoiceID = salesOrderPatchRes.InvoiceID,
                InvoiceHistoryID = salesOrderPatchRes.Histories[0].InvoiceHistoryID,
                InvoiceLineID = salesOrderPatchRes.Lines[1].InvoiceLineID,
                QuantityOrdered = 123,
                DiscountedPrice = 456.78M
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine salesOrderLinePatchRes = await Client.PatchAsync(salesOrderLinePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(salesOrderLinePatchRes.DiscountedPrice, Is.EqualTo(salesOrderLinePatchReq.DiscountedPrice));            

            // Remove the second sales order line
            SalesOrderLineDELETERequest salesOrderLineDeleteReq = new SalesOrderLineDELETERequest() 
            { 
                InvoiceID = salesOrderPatchRes.InvoiceID, 
                InvoiceHistoryID = salesOrderPatchRes.Histories[0].InvoiceHistoryID,
                InvoiceLineID = salesOrderPatchRes.Lines[1].InvoiceLineID 
            };
            await Client.DeleteAsync(salesOrderLineDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // ensure the deleted item is not there anymore            
            salesOrderGetRes = await Client.GetAsync(salesOrderGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(salesOrderGetRes.InvoiceNo, Is.EqualTo(salesOrderCreateRes.InvoiceNo));
            Assert.That(salesOrderGetRes.Lines.Count, Is.EqualTo(1));
            Assert.That(salesOrderGetRes.Lines.Where(x => x.InvoiceLineID == salesOrderPatchRes.Lines[1].InvoiceLineID).FirstOrDefault(), Is.EqualTo(null));

            // ensure attempting to get the deleted item throws a 404
            SalesOrderLineGETRequest salesOrderLineGetReq = new SalesOrderLineGETRequest()
            {
                InvoiceID = salesOrderPatchRes.InvoiceID,
                InvoiceHistoryID = salesOrderPatchRes.Histories[0].InvoiceHistoryID,
                InvoiceLineID = salesOrderPatchRes.Lines[1].InvoiceLineID
            };
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine salesOrderLineGetRes = await Client.GetAsync(salesOrderLineGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));// ensure the deleted item is not there anymore            

            // Add a sales order line
            SalesOrderLinePOSTRequest salesOrderLineCreateReq = new SalesOrderLinePOSTRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                InvoiceHistoryID = salesOrderCreateRes.Histories[0].InvoiceHistoryID,
                PartNo = itemCreateRes.PartNo,
                QuantityOrdered = 18           
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine salesOrderLineCreateRes = await Client.PostAsync(salesOrderLineCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(salesOrderLineCreateRes.QuantityOrdered, Is.EqualTo(salesOrderLineCreateReq.QuantityOrdered));
            Assert.That(salesOrderLineCreateRes.DiscountedPrice, Is.EqualTo(itemCreateReq.DefaultPrice));

            // Get the line we just created
            salesOrderLineGetReq = new SalesOrderLineGETRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                InvoiceHistoryID = salesOrderCreateRes.Histories[0].InvoiceHistoryID,
                InvoiceLineID = salesOrderLineCreateRes.InvoiceLineID
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine salesOrderLineGetRes = await Client.GetAsync(salesOrderLineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(salesOrderLineGetRes.InventoryID, Is.EqualTo(salesOrderLineCreateRes.InventoryID));
            Assert.That(salesOrderLineGetRes.PartNo, Is.EqualTo(salesOrderLineCreateRes.PartNo));
            Assert.That(salesOrderLineGetRes.DiscountedPrice, Is.EqualTo(salesOrderLineCreateRes.DiscountedPrice));
            Assert.That(salesOrderLineGetRes.QuantityOrdered, Is.EqualTo(salesOrderLineCreateRes.QuantityOrdered));

            // Try to GET non-existent sales order to make sure we get a 404
            salesOrderGetReq.InvoiceID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder salesOrderGetRes = await Client.GetAsync(salesOrderGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }        

        [Test]
        public async Task SalesOrder_Serialised_LineDetails_CRUD()
        {
            // Create a serialised item
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Item Test",
                DefaultPrice = 125.67M,
                UseSerialNo = true
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.PartNo, Is.EqualTo(itemCreateReq.PartNo));
            Assert.That(itemCreateRes.InventoryID, !Is.Null);

            // Create a debtor
            DebtorPOSTRequest accountCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test"
            };

            Debtor accountCreateRes = await Client.PostAsync(accountCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(accountCreateRes.AccountNo, Is.EqualTo(accountCreateReq.AccountNo));
            Assert.That(accountCreateRes.DebtorID, !Is.Null);

            // Transfer in some stock of the serialised item - 4 serial numbers
            StockTransferPOSTRequest transferCreateReq = new StockTransferPOSTRequest()
            {                
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers.StockTransferLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers.StockTransferLine()
                    {
                        FromInventoryPartNo = "External",
                        ToInventoryPartNo = itemCreateReq.PartNo,
                        TransferQuantity = 1,
                        ToPartSerialNo = RandomString(5)
                    },
                    new JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers.StockTransferLine()
                    {
                        FromInventoryPartNo = "External",
                        ToInventoryPartNo = itemCreateReq.PartNo,
                        TransferQuantity = 1,
                        ToPartSerialNo = RandomString(5)
                    },
                    new JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers.StockTransferLine()
                    {
                        FromInventoryPartNo = "External",
                        ToInventoryPartNo = itemCreateReq.PartNo,
                        TransferQuantity = 1,
                        ToPartSerialNo = RandomString(5)
                    },
                    new JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers.StockTransferLine()
                    {                        
                        FromInventoryPartNo = "External",
                        ToInventoryPartNo = itemCreateReq.PartNo,
                        TransferQuantity = 1,
                        ToPartSerialNo = RandomString(5)
                    }
                }
            };
            StockTransfer transferCreateRes = await Client.PostAsync(transferCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(transferCreateRes.TransferID, !Is.Null);

            // Activate the transfer
            StockTransferACTIVATERequest transferActivateReq = new StockTransferACTIVATERequest() { TransferID = transferCreateRes.TransferID };
            StockTransfer transferActivateRes = await Client.PostAsync(transferActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a sales order
            SalesOrderPOSTRequest salesOrderCreateReq = new SalesOrderPOSTRequest()
            {
                DebtorAccountNo = accountCreateReq.AccountNo,
                InvoiceInitDate = DateTime.Today.Date,
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        QuantityOrdered = 2,
                        LineDetails = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail>()
                        {
                            new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail()
                            {
                                SerialNo = transferCreateReq.Lines[0].ToPartSerialNo,
                                Quantity = 1
                            },
                            new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail()
                            {
                                SerialNo = transferCreateReq.Lines[1].ToPartSerialNo,
                                Quantity = 1
                            }
                        }
                    }
                }
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder salesOrderCreateRes = await Client.PostAsync(salesOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(salesOrderCreateRes.InvoiceID, !Is.Null);
            Assert.That(salesOrderCreateRes.Lines[0].LineDetails.Where(x => x.SerialNo == transferCreateReq.Lines[0].ToPartSerialNo).First(), !Is.EqualTo(null));
            Assert.That(salesOrderCreateRes.Lines[0].LineDetails.Where(x => x.SerialNo == transferCreateReq.Lines[1].ToPartSerialNo).First(), !Is.EqualTo(null));
            Assert.That(salesOrderCreateRes.Lines[0].QuantityThisDel, Is.EqualTo(2));

            // Add another serial no to the first line of the sales order - this should increase the quantity taken.
            SalesOrderLineDetailPOSTRequest salesOrderLineDetailCreateReq = new SalesOrderLineDetailPOSTRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                InvoiceHistoryID = salesOrderCreateRes.Histories[0].InvoiceHistoryID,
                InvoiceLineID = salesOrderCreateRes.Lines[0].InvoiceLineID,
                SerialNo = transferCreateReq.Lines[2].ToPartSerialNo,
                Quantity = 1
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail salesOrderLineDetailCreateRes = await Client.PostAsync(salesOrderLineDetailCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(salesOrderLineDetailCreateRes.LineDetailID, !Is.Null);
            Assert.That(salesOrderLineDetailCreateRes.SerialNo, Is.EqualTo(salesOrderLineDetailCreateReq.SerialNo));            

            // ensure getting the line now has all 3 serials
            SalesOrderLineGETRequest salesOrderLineGetReq = new SalesOrderLineGETRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                InvoiceHistoryID = salesOrderCreateRes.Histories[0].InvoiceHistoryID,
                InvoiceLineID = salesOrderCreateRes.Lines[0].InvoiceLineID
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine salesOrderLineGetRes = await Client.GetAsync(salesOrderLineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(salesOrderLineGetRes.InvoiceLineID, Is.EqualTo(salesOrderCreateRes.Lines[0].InvoiceLineID));
            Assert.That(salesOrderLineGetRes.QuantityThisDel, Is.EqualTo(3));
            Assert.That(salesOrderLineGetRes.LineDetails.Count, Is.EqualTo(3));
            Assert.That(salesOrderLineGetRes.LineDetails.Where(x => x.SerialNo == transferCreateReq.Lines[0].ToPartSerialNo).FirstOrDefault(), !Is.EqualTo(null));
            Assert.That(salesOrderLineGetRes.LineDetails.Where(x => x.SerialNo == transferCreateReq.Lines[1].ToPartSerialNo).FirstOrDefault(), !Is.EqualTo(null));
            Assert.That(salesOrderLineGetRes.LineDetails.Where(x => x.SerialNo == transferCreateReq.Lines[2].ToPartSerialNo).FirstOrDefault(), !Is.EqualTo(null));

            // Remove a serial number from the sales order line
            string deletedSerialNo = salesOrderLineGetRes.LineDetails[0].SerialNo;
            SalesOrderLineDetailDELETERequest salesOrderLineDetailDeleteReq = new SalesOrderLineDetailDELETERequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                InvoiceHistoryID = salesOrderCreateRes.Histories[0].InvoiceHistoryID,
                InvoiceLineID = salesOrderLineGetRes.InvoiceLineID,
                LineDetailID = salesOrderLineGetRes.LineDetails[0].LineDetailID
            };
            await Client.DeleteAsync(salesOrderLineDetailDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Read the line to make sure the serial is gone
            salesOrderLineGetRes = await Client.GetAsync(salesOrderLineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(salesOrderLineGetRes.LineDetails.Where(x => x.SerialNo == deletedSerialNo).FirstOrDefault(), Is.EqualTo(null));

            // Create another sales order line with a serial number selected
            SalesOrderLinePOSTRequest salesOrderLineCreateReq = new SalesOrderLinePOSTRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                InvoiceHistoryID = salesOrderCreateRes.Histories[0].InvoiceHistoryID,
                PartNo = itemCreateRes.PartNo,
                QuantityOrdered = 1,
                LineDetails = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail()
                    {
                        SerialNo = transferCreateReq.Lines[3].ToPartSerialNo,
                        Quantity = 1
                    }
                }
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine salesOrderLineCreateRes = await Client.PostAsync(salesOrderLineCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(salesOrderLineCreateRes.QuantityOrdered, Is.EqualTo(salesOrderLineCreateReq.QuantityOrdered));
            Assert.That(salesOrderLineCreateRes.LineDetails[0].SerialNo, Is.EqualTo(transferCreateReq.Lines[3].ToPartSerialNo));

            // Try to create another sales order line with a non-existent serial number
            salesOrderLineCreateReq = new SalesOrderLinePOSTRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                InvoiceHistoryID = salesOrderCreateRes.Histories[0].InvoiceHistoryID,
                PartNo = itemCreateRes.PartNo,
                QuantityOrdered = 1,
                LineDetails = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail()
                    {
                        SerialNo = RandomString(5),
                        Quantity = 1
                    }
                }
            };
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine salesOrderLineCreateRes = await Client.PostAsync(salesOrderLineCreateReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Try to add a line detail with a non-existent serial number
            salesOrderLineDetailCreateReq = new SalesOrderLineDetailPOSTRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                InvoiceHistoryID = salesOrderCreateRes.Histories[0].InvoiceHistoryID,
                InvoiceLineID = salesOrderCreateRes.Lines[0].InvoiceLineID,
                SerialNo = RandomString(5),
                Quantity = 1
            };
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail salesOrderLineDetailCreateRes = await Client.PostAsync(salesOrderLineDetailCreateReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));            
        }
        #endregion

        #region "Queries"
        [Test]
        public async Task SO_MainQuery()
        {
            SO_MainQuery SO_MainQueryRequest = new SO_MainQuery();
            ServiceStack.QueryResponse<SO_Main> SO_MainQueryResponse;

            //Read all inventory items            
            SO_MainQueryResponse = await Client.GetAsync(SO_MainQueryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Let's assume we expect to get at least one sales order back - demo data has many sales orders.
            Assert.That(SO_MainQueryResponse.Results.Count > 0);

            // Try with an invalid APIKey on to make sure we get a 401
            // Need to use a new client for this, as existing session Id's cookied will bind us to the session from
            // previous requests
            using (ServiceStack.JsonApiClient tempClient = new ServiceStack.JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                var ex = Assert.Throws<ServiceStack.WebServiceException>(() => SO_MainQueryResponse = tempClient.Get(SO_MainQueryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }                
        }

        [Test]
        public async Task v_Jiwa_SalesOrder_ListQuery()
        {
            // get first 10 sales orders
            v_Jiwa_SalesOrder_ListQuery v_Jiwa_SalesOrder_ListQueryRequest = new v_Jiwa_SalesOrder_ListQuery() 
            { 
                Take = 10, 
                OrderBy = "InvoiceNo" 
            };
            ServiceStack.QueryResponse<v_Jiwa_SalesOrder_List> v_Jiwa_SalesOrder_ListQueryResponse;

            v_Jiwa_SalesOrder_ListQueryResponse = await Client.GetAsync(v_Jiwa_SalesOrder_ListQueryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Ensure we got only the 10 we asked for
            Assert.That(v_Jiwa_SalesOrder_ListQueryResponse.Results.Count == 10);

            // Try with an invalid APIKey on to make sure we get a 401
            // Need to use a new client for this, as existing session Id's cookied will bind us to the session from
            // previous requests
            using (ServiceStack.JsonApiClient tempClient = new ServiceStack.JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                var ex = Assert.Throws<ServiceStack.WebServiceException>(() => v_Jiwa_SalesOrder_ListQueryResponse = tempClient.Get(v_Jiwa_SalesOrder_ListQueryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }
        }
        
        #endregion
    }
}
