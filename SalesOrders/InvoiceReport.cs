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
    public class InvoiceReport : JiwaAPITest
    {
        private async Task<(JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder salesOrder, InventoryItem item, Debtor debtor)> CreateSalesOrderWithLineAsync()
        {
            // Create an inventory item
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Invoice Report Test Item",
                DefaultPrice = 10.00M
            };
            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a debtor
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Invoice Report Test Debtor"
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

        #region "{InvoiceReport}"
        [Test]
        public async Task SalesOrder_InvoiceReport_GET()
        {
            // Create a sales order to obtain a current invoice report
            var (salesOrder, _, _) = await CreateSalesOrderWithLineAsync();

            // Attempt to retrieve the current invoice report
            InvoiceReportGETRequest reportGetReq = new InvoiceReportGETRequest()
            {
                InvoiceID = salesOrder.InvoiceID,
                ReportID = Guid.NewGuid().ToString(),
                AsAttachment = false
            };

            try
            {
                object reportGetRes = await Client.GetAsync(reportGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(reportGetRes, Is.Not.Null);
            }
            catch (WebServiceException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
            }
        }
        #endregion

        #region "{InvoiceSnapshotReport}"
        [Test]
        public async Task SalesOrder_InvoiceSnapshotReport_GET()
        {
            // Create a sales order to obtain a snapshot invoice report
            var (salesOrder, _, _) = await CreateSalesOrderWithLineAsync();
            string invoiceHistoryID = salesOrder.Histories[0].InvoiceHistoryID;

            // Attempt to retrieve the invoice snapshot report
            InvoiceSnapshotReportGETRequest reportGetReq = new InvoiceSnapshotReportGETRequest()
            {
                InvoiceHistoryID = invoiceHistoryID,
                ReportID = Guid.NewGuid().ToString(),
                AsAttachment = false
            };

            try
            {
                object reportGetRes = await Client.GetAsync(reportGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(reportGetRes, Is.Not.Null);
            }
            catch (WebServiceException ex)
            {
                Assert.That(ex.StatusCode, Is.EqualTo(404));
            }
        }
        #endregion
    }
}

