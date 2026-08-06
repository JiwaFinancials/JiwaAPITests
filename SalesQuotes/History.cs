using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesQuoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuote;
using SalesQuoteHistoryDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuoteHistory;

namespace JiwaAPITests.SalesQuotes
{
    public class History : JiwaAPITest
    {
        private async Task<SalesQuoteDto> CreateSalesQuoteWithLineAsync()
        {
            // Create an inventory item
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Sales Quote History Test Item",
                DefaultPrice = 10.00M
            };
            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a debtor
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Sales Quote History Test Debtor"
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

        #region "{Historys}"
        [Test]
        public async Task SalesQuoteHistory_CRUD()
        {
            // Create a sales quote to obtain a history record
            SalesQuoteDto salesQuoteCreateRes = await CreateSalesQuoteWithLineAsync();
            string quoteID = salesQuoteCreateRes.QuoteID;
            string quoteHistoryID = salesQuoteCreateRes.Histories[0].QuoteHistoryID;

            // Read all histories for the sales quote
            SalesQuoteHistorysGETManyRequest historysGetManyReq = new SalesQuoteHistorysGETManyRequest()
            {
                QuoteID = quoteID
            };
            List<SalesQuoteHistoryDto> historysGetManyRes = await Client.GetAsync(historysGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(historysGetManyRes.Any(x => x.QuoteHistoryID == quoteHistoryID), Is.True);

            // Read the history
            SalesQuoteHistorysGETRequest historyGetReq = new SalesQuoteHistorysGETRequest()
            {
                QuoteID = quoteID,
                QuoteHistoryID = quoteHistoryID
            };
            SalesQuoteHistoryDto historyGetRes = await Client.GetAsync(historyGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(historyGetRes.QuoteHistoryID, Is.EqualTo(quoteHistoryID));

            // Update the history
            SalesQuoteHistorysPATCHRequest historyPatchReq = historyGetRes.ConvertTo<SalesQuoteHistorysPATCHRequest>();
            historyPatchReq.QuoteID = quoteID;
            historyPatchReq.QuoteHistoryID = quoteHistoryID;
            historyPatchReq.Notes = "Updated notes " + RandomString(6);
            SalesQuoteHistoryDto historyPatchRes = await Client.PatchAsync(historyPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(historyPatchRes.QuoteHistoryID, Is.EqualTo(historyPatchReq.QuoteHistoryID));
            Assert.That(historyPatchRes.QuoteHistoryID, Is.EqualTo(quoteHistoryID));
            Assert.That(historyPatchRes.Notes, Is.EqualTo(historyPatchReq.Notes));

            // Read the updated history and confirm the changes were saved
            historyGetRes = await Client.GetAsync(historyGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(historyGetRes.Notes, Is.EqualTo(historyPatchReq.Notes));
        }
        #endregion
    }
}

