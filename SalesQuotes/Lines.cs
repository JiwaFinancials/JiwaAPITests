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

        [Test]
        public async Task SalesQuoteHistory_Lines_CRUD_CommentLine()
        {
            // Create a sales quote with an initial line to operate against
            SalesQuoteDto salesQuoteCreateRes = await CreateSalesQuoteWithLineAsync();
            string quoteID = salesQuoteCreateRes.QuoteID;
            string quoteHistoryID = salesQuoteCreateRes.Histories[0].QuoteHistoryID;

            // Read all lines for the quote history
            SalesQuoteLinesGETManyRequest linesGetManyReq = new SalesQuoteLinesGETManyRequest()
            {
                QuoteID = quoteID,
                QuoteHistoryID = quoteHistoryID
            };
            List<SalesQuoteLineDto> linesGetManyRes = await Client.GetAsync(linesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linesGetManyRes.Count, Is.GreaterThanOrEqualTo(1));

            // Append a new comment line to the sales quote history
            SalesQuoteLinePOSTRequest lineCreateReq = new SalesQuoteLinePOSTRequest()
            {
                QuoteID = quoteID,
                QuoteHistoryID = quoteHistoryID,
                CommentLine = true,
                CommentText = "Initial history comment line"
            };
            SalesQuoteLineDto lineCreateRes = await Client.PostAsync(lineCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(lineCreateRes.QuoteLineID, Is.Not.Null);
            Assert.That(lineCreateRes.CommentLine, Is.True);
            Assert.That(lineCreateRes.CommentText, Is.EqualTo(lineCreateReq.CommentText));

            // Read the created line
            SalesQuoteLineGETRequest lineGetReq = new SalesQuoteLineGETRequest()
            {
                QuoteID = quoteID,
                QuoteHistoryID = quoteHistoryID,
                QuoteLineID = lineCreateRes.QuoteLineID
            };
            SalesQuoteLineDto lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.QuoteLineID, Is.EqualTo(lineCreateRes.QuoteLineID));
            Assert.That(lineGetRes.CommentLine, Is.True);
            Assert.That(lineGetRes.CommentText, Is.EqualTo(lineCreateReq.CommentText));

            // Update the line
            SalesQuoteLinePATCHRequest linePatchReq = new SalesQuoteLinePATCHRequest()
            {
                QuoteID = quoteID,
                QuoteHistoryID = quoteHistoryID,
                QuoteLineID = lineCreateRes.QuoteLineID,
                CommentLine = true,
                CommentText = "Updated history comment line"
            };
            SalesQuoteLineDto linePatchRes = await Client.PatchAsync(linePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linePatchRes.QuoteLineID, Is.EqualTo(linePatchReq.QuoteLineID));
            Assert.That(linePatchRes.CommentLine, Is.True);
            Assert.That(linePatchRes.CommentText, Is.EqualTo(linePatchReq.CommentText));

            // Read the updated line and confirm the changes were saved
            lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.CommentLine, Is.True);
            Assert.That(lineGetRes.CommentText, Is.EqualTo(linePatchReq.CommentText));

            // Delete the line
            SalesQuoteLineDELETERequest lineDeleteReq = new SalesQuoteLineDELETERequest()
            {
                QuoteID = quoteID,
                QuoteHistoryID = quoteHistoryID,
                QuoteLineID = lineCreateRes.QuoteLineID
            };
            await Client.DeleteAsync(lineDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the line was deleted
            ServiceStack.WebServiceException exLine = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                SalesQuoteLineDto deletedLineGetRes = await Client.GetAsync(lineGetReq);
            });
            Assert.That(exLine.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}


