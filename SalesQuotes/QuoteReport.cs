using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SalesQuoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuote;

namespace JiwaAPITests.SalesQuotes
{
    public class QuoteReport : JiwaAPITest
    {
        private async Task<SalesQuoteDto> CreateSalesQuoteWithLineAsync()
        {
            // Create an inventory item
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Sales Quote Report Test Item",
                DefaultPrice = 10.00M
            };
            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a debtor
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Sales Quote Report Test Debtor"
            };
            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a sales quote with one line
            SalesQuotePOSTRequest quoteCreateReq = new SalesQuotePOSTRequest()
            {
                DebtorAccountNo = debtorCreateReq.AccountNo,
                InvoiceInitDate = DateTime.Today,
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuoteLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuoteLine()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        QuantityOrdered = 1
                    }
                }
            };
            SalesQuoteDto quoteCreateRes = await Client.PostAsync(quoteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(quoteCreateRes.QuoteID, Is.Not.Null);

            return quoteCreateRes;
        }

        #region "{QuoteReport}"
        [Test]
        public async Task SalesQuote_QuoteReport_GET()
        {
            // Create a sales quote to obtain a current quote report
            SalesQuoteDto salesQuoteCreateRes = await CreateSalesQuoteWithLineAsync();

            // Attempt to retrieve the current quote report
            SalesQuoteReportGETRequest reportGetReq = new SalesQuoteReportGETRequest()
            {
                QuoteID = salesQuoteCreateRes.QuoteID,
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

        #region "{QuoteSnapshotReport}"
        [Test]
        public async Task SalesQuote_QuoteSnapshotReport_GET()
        {
            // Create a sales quote to obtain a snapshot quote report
            SalesQuoteDto salesQuoteCreateRes = await CreateSalesQuoteWithLineAsync();
            string quoteHistoryID = salesQuoteCreateRes.Histories[0].QuoteHistoryID;

            // Attempt to retrieve the quote snapshot report
            SalesQuoteSnapshotReportGETRequest reportGetReq = new SalesQuoteSnapshotReportGETRequest()
            {
                QuoteHistoryID = quoteHistoryID,
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

