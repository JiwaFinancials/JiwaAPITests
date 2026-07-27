using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesQuoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuote;
using SalesQuoteLineDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuoteLine;

namespace JiwaAPITests.SalesQuotes
{
    public class Lines : JiwaAPITest
    {
        private async Task<SalesQuoteDto> CreateSalesQuoteWithLineAsync()
        {
            // Create an inventory item
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Sales Quote Lines Test Item",
                DefaultPrice = 10.00M
            };
            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a debtor
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Sales Quote Lines Test Debtor"
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

        #region "{Historys/Lines}"
        [Test]
        public async Task SalesQuoteHistory_Lines_GETMany()
        {
            // Create a sales quote with an initial line to operate against
            SalesQuoteDto salesQuoteCreateRes = await CreateSalesQuoteWithLineAsync();
            string quoteID = salesQuoteCreateRes.QuoteID;
            string quoteHistoryID = salesQuoteCreateRes.Histories[0].QuoteHistoryID;
            string quoteLineID = salesQuoteCreateRes.Lines[0].QuoteLineID;

            // Read all lines for the quote history
            SalesQuoteLinesGETManyRequest linesGetManyReq = new SalesQuoteLinesGETManyRequest()
            {
                QuoteID = quoteID,
                QuoteHistoryID = quoteHistoryID
            };
            List<SalesQuoteLineDto> linesGetManyRes = await Client.GetAsync(linesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linesGetManyRes.Any(x => x.QuoteLineID == quoteLineID), Is.True);
        }
        #endregion
    }
}

