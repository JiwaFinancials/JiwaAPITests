using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using JiwaFinancials.Jiwa.JiwaServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.SalesQuotes
{
    [TestFixture]
    class SalesQuote : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task SalesQuote_CRUD()
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

            // Create a sales quote
            SalesQuotePOSTRequest salesQuoteCreateReq = new SalesQuotePOSTRequest()
            {
                DebtorAccountNo = accountCreateReq.AccountNo,
                InvoiceInitDate = DateTime.Today.Date,
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuoteLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuoteLine()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        QuantityOrdered = 5
                    }
                }
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuote salesQuoteCreateRes = await Client.PostAsync(salesQuoteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(salesQuoteCreateRes.QuoteID, !Is.Null);

            // Read the created sales quote using the QuoteID
            SalesQuoteGETRequest salesQuoteGetReq = new SalesQuoteGETRequest() { QuoteID = salesQuoteCreateRes.QuoteID };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuote salesQuoteGetRes = await Client.GetAsync(salesQuoteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(salesQuoteGetRes.QuoteNo, Is.EqualTo(salesQuoteCreateRes.QuoteNo));
            Assert.That(salesQuoteGetRes.Lines.Count, Is.EqualTo(1));

            // Update the sales quote - including adding another line
            SalesQuotePATCHRequest salesQuotePatchReq = new SalesQuotePATCHRequest()
            {
                QuoteID = salesQuoteCreateRes.QuoteID,
                SOReference = "Updated sales quote Test",
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuoteLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuoteLine()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        Description = "second line added of same part",
                        QuantityOrdered = 11,
                        DiscountedPrice = 100.00M
                    }
                }
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuote salesQuotePatchRes = await Client.PatchAsync(salesQuotePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(salesQuotePatchRes.Lines.Count, Is.EqualTo(2));
            Assert.That(salesQuotePatchRes.Lines[1].DiscountedPrice, Is.EqualTo(salesQuotePatchReq.Lines[0].DiscountedPrice));

            // Patch the second sales quote line
            SalesQuoteLinePATCHRequest salesQuoteLinePatchReq = new SalesQuoteLinePATCHRequest()
            {
                QuoteID = salesQuotePatchRes.QuoteID,
                QuoteHistoryID = salesQuotePatchRes.Histories[0].QuoteHistoryID,
                QuoteLineID = salesQuotePatchRes.Lines[1].QuoteLineID,
                QuantityOrdered = 123,
                DiscountedPrice = 456.78M
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuoteLine salesQuoteLinePatchRes = await Client.PatchAsync(salesQuoteLinePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(salesQuoteLinePatchRes.DiscountedPrice, Is.EqualTo(salesQuoteLinePatchReq.DiscountedPrice));

            // Remove the second sales quote line
            SalesQuoteLineDELETERequest salesQuoteLineDeleteReq = new SalesQuoteLineDELETERequest()
            {
                QuoteID = salesQuotePatchRes.QuoteID,
                QuoteHistoryID = salesQuotePatchRes.Histories[0].QuoteHistoryID,
                QuoteLineID = salesQuotePatchRes.Lines[1].QuoteLineID
            };
            await Client.DeleteAsync(salesQuoteLineDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // ensure the deleted item is not there anymore            
            salesQuoteGetRes = await Client.GetAsync(salesQuoteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(salesQuoteGetRes.QuoteNo, Is.EqualTo(salesQuoteCreateRes.QuoteNo));
            Assert.That(salesQuoteGetRes.Lines.Count, Is.EqualTo(1));
            Assert.That(salesQuoteGetRes.Lines.Where(x => x.QuoteLineID == salesQuotePatchRes.Lines[1].QuoteLineID).FirstOrDefault(), Is.EqualTo(null));

            // ensure attempting to get the deleted item throws a 404
            SalesQuoteLineGETRequest salesQuoteLineGetReq = new SalesQuoteLineGETRequest()
            {
                QuoteID = salesQuotePatchRes.QuoteID,
                QuoteHistoryID = salesQuotePatchRes.Histories[0].QuoteHistoryID,
                QuoteLineID = salesQuotePatchRes.Lines[1].QuoteLineID
            };
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuoteLine salesQuoteLineGetRes = await Client.GetAsync(salesQuoteLineGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));// ensure the deleted item is not there anymore            

            // Try to GET non-existent sales quote to make sure we get a 404
            salesQuoteGetReq.QuoteID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuote salesQuoteGetRes = await Client.GetAsync(salesQuoteGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "Queries"
        [Test]
        public async Task QO_MainQuery()
        {
            QO_MainQuery QO_MainQueryRequest = new QO_MainQuery();
            ServiceStack.QueryResponse<QO_Main> QO_MainQueryResponse;

            //Read all inventory items            
            QO_MainQueryResponse = await Client.GetAsync(QO_MainQueryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Let's assume we expect to get at least one sales quote back - demo data has many sales quotes.
            Assert.That(QO_MainQueryResponse.Results.Count > 0);

            // Try with an invalid APIKey on to make sure we get a 401
            // Need to use a new client for this, as existing session Id's cookied will bind us to the session from
            // previous requests
            using (ServiceStack.JsonApiClient tempClient = new ServiceStack.JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                var ex = Assert.Throws<ServiceStack.WebServiceException>(() => QO_MainQueryResponse = tempClient.Get(QO_MainQueryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }
        }

        [Test]
        public async Task v_Jiwa_SalesQuote_ListQuery()
        {
            // get first 10 sales quotes
            v_Jiwa_SalesQuote_ListQuery v_Jiwa_SalesQuote_ListQueryRequest = new v_Jiwa_SalesQuote_ListQuery()
            {
                Take = 10,
                OrderBy = "InvoiceNo" //InvoiceNo is actually the QuoteNo.  For reasons of legacy.
            };
            ServiceStack.QueryResponse<v_Jiwa_SalesQuote_List> v_Jiwa_SalesQuote_ListQueryResponse;

            v_Jiwa_SalesQuote_ListQueryResponse = await Client.GetAsync(v_Jiwa_SalesQuote_ListQueryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Ensure we got only the 10 we asked for
            Assert.That(v_Jiwa_SalesQuote_ListQueryResponse.Results.Count == 10);

            // Try with an invalid APIKey on to make sure we get a 401
            // Need to use a new client for this, as existing session Id's cookied will bind us to the session from
            // previous requests
            using (ServiceStack.JsonApiClient tempClient = new ServiceStack.JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                var ex = Assert.Throws<ServiceStack.WebServiceException>(() => v_Jiwa_SalesQuote_ListQueryResponse = tempClient.Get(v_Jiwa_SalesQuote_ListQueryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }
        }

        #endregion
    }
}
