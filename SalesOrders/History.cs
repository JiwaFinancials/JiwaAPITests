using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Carriers;
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
    public class History : JiwaAPITest
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

        #region "{Main}"
        [Test]
        public async Task SalesOrderHistory_CRUD()
        {
            // Create a sales order to obtain a history record
            var (salesOrder, _, _) = await CreateSalesOrderWithLineAsync();
            string invoiceID = salesOrder.InvoiceID;
            string invoiceHistoryID = salesOrder.Histories[0].InvoiceHistoryID;

            // Read all histories for the sales order
            SalesOrderHistorysGETManyRequest historysGetManyReq = new SalesOrderHistorysGETManyRequest()
            {
                InvoiceID = invoiceID
            };
            List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderHistory> historysGetManyRes = await Client.GetAsync(historysGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(historysGetManyRes.Any(x => x.InvoiceHistoryID == invoiceHistoryID), Is.True);

            // Read the history
            SalesOrderHistorysGETRequest historyGetReq = new SalesOrderHistorysGETRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderHistory historyGetRes = await Client.GetAsync(historyGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(historyGetRes.InvoiceHistoryID, Is.EqualTo(invoiceHistoryID));

            // Update the history
            SalesOrderHistorysPATCHRequest historyPatchReq = new SalesOrderHistorysPATCHRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                Notes = "Updated notes " + RandomString(6)
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderHistory historyPatchRes = await Client.PatchAsync(historyPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(historyPatchRes.InvoiceHistoryID, Is.EqualTo(historyPatchReq.InvoiceHistoryID));
            Assert.That(historyPatchRes.InvoiceHistoryID, Is.EqualTo(invoiceHistoryID));
            Assert.That(historyPatchRes.Notes, Is.EqualTo(historyPatchReq.Notes));

            // Read the updated history and confirm the changes were saved
            historyGetRes = await Client.GetAsync(historyGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(historyGetRes.Notes, Is.EqualTo(historyPatchReq.Notes));
        }
        #endregion

        #region "{InvoiceReport}"
        [Test]
        public async Task SalesOrderHistoryInvoiceReport_GET()
        {
            // Create a sales order to obtain a history record
            var (salesOrder, _, _) = await CreateSalesOrderWithLineAsync();
            string invoiceID = salesOrder.InvoiceID;
            string invoiceHistoryID = salesOrder.Histories[0].InvoiceHistoryID;

            // Attempt to retrieve the invoice report for the history snapshot
            InvoiceHistoryReportGETRequest reportGetReq = new InvoiceHistoryReportGETRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                ReportID = Guid.NewGuid().ToString(),
                AsAttachment = false
            };

            try
            {
                object reportGetRes = await Client.GetAsync(reportGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            }
            catch (WebServiceException ex)
            {
                // A 404 is acceptable when no matching report is configured in the test environment
                Assert.That(ex.StatusCode, Is.EqualTo(404));
            }
        }
        #endregion

        #region "{ConsignmentNotes}"
        [Test]
        public async Task SalesOrderHistory_ConsignmentNotes_CRUD()
        {
            // Create a sales order to operate against
            var (salesOrder, _, _) = await CreateSalesOrderWithLineAsync();
            string invoiceID = salesOrder.InvoiceID;
            string invoiceHistoryID = salesOrder.Histories[0].InvoiceHistoryID;

            // Create a consignment note on the history
            SalesOrderHistoryConsignmentNotesPOSTRequest consignmentCreateReq = new SalesOrderHistoryConsignmentNotesPOSTRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                ConsignmentNoteNo = "CN-" + RandomString(6),
                ConsignmentNoteDate = DateTime.Today,
                ExGSTAmount = 15.00M,
                GSTAmount = 1.50M
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderConsignmentNote consignmentCreateRes = await Client.PostAsync(consignmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(consignmentCreateRes.ConsignmentNoteID, Is.Not.Null);
            Assert.That(consignmentCreateRes.ConsignmentNoteNo, Is.EqualTo(consignmentCreateReq.ConsignmentNoteNo));

            // Read all consignment notes for the history
            SalesOrderHistoryConsignmentNotesGETManyRequest consignmentsGetManyReq = new SalesOrderHistoryConsignmentNotesGETManyRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID
            };
            List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderConsignmentNote> consignmentsGetManyRes = await Client.GetAsync(consignmentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(consignmentsGetManyRes.Any(x => x.ConsignmentNoteID == consignmentCreateRes.ConsignmentNoteID), Is.True);

            // Read the created consignment note
            SalesOrderHistoryConsignmentNotesGETRequest consignmentGetReq = new SalesOrderHistoryConsignmentNotesGETRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                ConsignmentNoteID = consignmentCreateRes.ConsignmentNoteID
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderConsignmentNote consignmentGetRes = await Client.GetAsync(consignmentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(consignmentGetRes.ConsignmentNoteID, Is.EqualTo(consignmentCreateRes.ConsignmentNoteID));
            Assert.That(consignmentGetRes.ConsignmentNoteNo, Is.EqualTo(consignmentCreateReq.ConsignmentNoteNo));

            // Update the consignment note
            SalesOrderHistoryConsignmentNotesPATCHRequest consignmentPatchReq = new SalesOrderHistoryConsignmentNotesPATCHRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                ConsignmentNoteID = consignmentCreateRes.ConsignmentNoteID,
                ConsignmentNoteNo = "CN-UPDATED-" + RandomString(4),
                ExGSTAmount = 20.00M,
                GSTAmount = 2.00M
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderConsignmentNote consignmentPatchRes = await Client.PatchAsync(consignmentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(consignmentPatchRes.ConsignmentNoteID, Is.EqualTo(consignmentPatchReq.ConsignmentNoteID));
            Assert.That(consignmentPatchRes.ConsignmentNoteID, Is.EqualTo(consignmentCreateRes.ConsignmentNoteID));
            Assert.That(consignmentPatchRes.ConsignmentNoteNo, Is.EqualTo(consignmentPatchReq.ConsignmentNoteNo));

            // Read the updated consignment note and confirm the changes were saved
            consignmentGetRes = await Client.GetAsync(consignmentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(consignmentGetRes.ConsignmentNoteNo, Is.EqualTo(consignmentPatchReq.ConsignmentNoteNo));
            Assert.That(consignmentGetRes.ExGSTAmount, Is.EqualTo(consignmentPatchReq.ExGSTAmount));

            // Delete the consignment note
            SalesOrderHistoryConsignmentNotesDELETERequest consignmentDeleteReq = new SalesOrderHistoryConsignmentNotesDELETERequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                ConsignmentNoteID = consignmentCreateRes.ConsignmentNoteID
            };
            await Client.DeleteAsync(consignmentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the consignment note was deleted
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderConsignmentNote deletedConsignmentGetRes = await Client.GetAsync(consignmentGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "{FreightItems}"
        [Test]
        public async Task SalesOrderHistory_FreightItems_CRUD()
        {
            // Create a sales order to operate against
            var (salesOrder, _, _) = await CreateSalesOrderWithLineAsync();
            string invoiceID = salesOrder.InvoiceID;
            string invoiceHistoryID = salesOrder.Histories[0].InvoiceHistoryID;

            // Create a carrier to use for the freight item
            CarrierPOSTRequest carrierCreateReq = new CarrierPOSTRequest()
            {
                CarrierName = $"Carrier {RandomString(5)}",
                AccountNo = RandomString(6),
                Enabled = false,
                Notes = "Freight Items CRUD Test"
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Carriers.Carrier carrierCreateRes = await Client.PostAsync(carrierCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(carrierCreateRes.CarrierID, Is.Not.Null);
            Assert.That(carrierCreateRes.CarrierName, Is.EqualTo(carrierCreateReq.CarrierName));

            // Read the created carrier
            CarrierGETRequest carrierGetReq = new CarrierGETRequest()
            {
                CarrierID = carrierCreateRes.CarrierID
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Carriers.Carrier carrierGetRes = await Client.GetAsync(carrierGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(carrierGetRes.CarrierID, Is.EqualTo(carrierCreateRes.CarrierID));
            Assert.That(carrierGetRes.CarrierName, Is.EqualTo(carrierCreateReq.CarrierName));

            // Create a carrier service
            CarrierServicePOSTRequest serviceCreateReq = new CarrierServicePOSTRequest()
            {
                CarrierID = carrierCreateRes.CarrierID,
                ServiceName = $"Service {RandomString(5)}",
                Enabled = true,
                MaximumWeight = 123.45M,
                DefaultItem = true
            };

            CarrierService serviceCreateRes = await Client.PostAsync(serviceCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(serviceCreateRes.ServiceID, Is.Not.Null);
            Assert.That(serviceCreateRes.ServiceName, Is.EqualTo(serviceCreateReq.ServiceName));

            // Create a carrier freight description
            CarrierFreightDescriptionPOSTRequest freightDescriptionCreateReq = new CarrierFreightDescriptionPOSTRequest()
            {
                CarrierID = carrierCreateRes.CarrierID,
                Description = $"Freight {RandomString(5)}",
                Enabled = true,
                DefaultItem = true
            };

            CarrierFreightDescription freightDescriptionCreateRes = await Client.PostAsync(freightDescriptionCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(freightDescriptionCreateRes.FreightDescriptionID, Is.Not.Null);
            Assert.That(freightDescriptionCreateRes.Description, Is.EqualTo(freightDescriptionCreateReq.Description));

            // Enable the created carrier
            CarrierPATCHRequest carrierPatchReq = new CarrierPATCHRequest()
            {
                CarrierID = carrierCreateRes.CarrierID,
                Enabled = true
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Carriers.Carrier carrierPatchRes = await Client.PatchAsync(carrierPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(carrierPatchRes.CarrierID, Is.EqualTo(carrierPatchReq.CarrierID));
            Assert.That(carrierPatchRes.CarrierID, Is.EqualTo(carrierCreateRes.CarrierID));
            Assert.That(carrierPatchRes.Enabled, Is.EqualTo(true));

            // Update the history to use the carrier we created (to ensure the freight item can be created)
            SalesOrderHistorysPATCHRequest historyPatchReq = new SalesOrderHistorysPATCHRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                Notes = "Updated carrier " + RandomString(6),
                Carrier = new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderCarrier
                {
                    CarrierID = carrierGetRes.CarrierID,
                    CarrierName = carrierGetRes.CarrierName,
                    AccountNo = carrierGetRes.AccountNo
                }
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderHistory historyPatchRes = await Client.PatchAsync(historyPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(historyPatchRes.InvoiceHistoryID, Is.EqualTo(historyPatchReq.InvoiceHistoryID));
            Assert.That(historyPatchRes.InvoiceHistoryID, Is.EqualTo(invoiceHistoryID));
            Assert.That(historyPatchRes.Notes, Is.EqualTo(historyPatchReq.Notes));

            // Create a freight item on the history
            SalesOrderHistoryFreightItemsPOSTRequest freightItemCreateReq = new SalesOrderHistoryFreightItemsPOSTRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                NumberItems = 3,
                ItemWeight = 1.5M,
                Reference = "FI-" + RandomString(6)
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderFreightItem freightItemCreateRes = await Client.PostAsync(freightItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(freightItemCreateRes.FreightItemID, Is.Not.Null);
            Assert.That(freightItemCreateRes.NumberItems, Is.EqualTo(freightItemCreateReq.NumberItems));

            // Read all freight items for the history
            SalesOrderHistoryFreightItemsGETManyRequest freightItemsGetManyReq = new SalesOrderHistoryFreightItemsGETManyRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID
            };
            List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderFreightItem> freightItemsGetManyRes = await Client.GetAsync(freightItemsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(freightItemsGetManyRes.Any(x => x.FreightItemID == freightItemCreateRes.FreightItemID), Is.True);

            // Read the created freight item
            SalesOrderHistoryFreightItemsGETRequest freightItemGetReq = new SalesOrderHistoryFreightItemsGETRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                FreightItemID = freightItemCreateRes.FreightItemID
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderFreightItem freightItemGetRes = await Client.GetAsync(freightItemGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(freightItemGetRes.FreightItemID, Is.EqualTo(freightItemCreateRes.FreightItemID));
            Assert.That(freightItemGetRes.NumberItems, Is.EqualTo(freightItemCreateReq.NumberItems));

            // Update the freight item
            SalesOrderHistoryFreightItemsPATCHRequest freightItemPatchReq = new SalesOrderHistoryFreightItemsPATCHRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                FreightItemID = freightItemCreateRes.FreightItemID,
                NumberItems = 7,
                ItemWeight = 3.0M,
                Reference = "FI-UPDATED-" + RandomString(4)
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderFreightItem freightItemPatchRes = await Client.PatchAsync(freightItemPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(freightItemPatchRes.FreightItemID, Is.EqualTo(freightItemPatchReq.FreightItemID));
            Assert.That(freightItemPatchRes.FreightItemID, Is.EqualTo(freightItemCreateRes.FreightItemID));
            Assert.That(freightItemPatchRes.NumberItems, Is.EqualTo(freightItemPatchReq.NumberItems));

            // Read the updated freight item and confirm the changes were saved
            freightItemGetRes = await Client.GetAsync(freightItemGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(freightItemGetRes.NumberItems, Is.EqualTo(freightItemPatchReq.NumberItems));
            Assert.That(freightItemGetRes.Reference, Is.EqualTo(freightItemPatchReq.Reference));

            // Delete the freight item
            SalesOrderHistoryFreightItemsDELETERequest freightItemDeleteReq = new SalesOrderHistoryFreightItemsDELETERequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                FreightItemID = freightItemCreateRes.FreightItemID
            };
            await Client.DeleteAsync(freightItemDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the freight item was deleted
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderFreightItem deletedFreightItemGetRes = await Client.GetAsync(freightItemGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "{Payments}"
        [Test]
        public async Task SalesOrderHistory_Payments_CRUD()
        {
            // Get all available payment types
            SalesOrderPaymentTypesGETManyRequest paymentTypesGetManyReq = new SalesOrderPaymentTypesGETManyRequest();
            List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.PaymentType> paymentTypesGetManyRes = await Client.GetAsync(paymentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(paymentTypesGetManyRes.Count, Is.GreaterThan(0), "At least one payment type must be configured to run the payment test");

            // Create a sales order to operate against
            var (salesOrder, _, _) = await CreateSalesOrderWithLineAsync();
            string invoiceID = salesOrder.InvoiceID;
            string invoiceHistoryID = salesOrder.Histories[0].InvoiceHistoryID;

            // Add a payment to the sales order history
            SalesOrderPaymentsPOSTRequest paymentCreateReq = new SalesOrderPaymentsPOSTRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                PaymentType = paymentTypesGetManyRes[0],
                AmountPaid = 10.00M,
                PaymentDate = DateTime.Today
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderPayment paymentCreateRes = await Client.PostAsync(paymentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(paymentCreateRes.PaymentID, Is.Not.Null);
            Assert.That(paymentCreateRes.AmountPaid, Is.EqualTo(paymentCreateReq.AmountPaid));
        }
        #endregion
    }
}


