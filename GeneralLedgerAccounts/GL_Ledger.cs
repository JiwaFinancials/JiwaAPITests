using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.GeneralLedgerAccounts
{
    public class GL_Ledger : JiwaAPITest
    {
        #region "Queries_GL_Ledger"
        [Test]
        public async Task GL_LedgerQuery()
        {
            GL_LedgerQuery queryRequest = new GL_LedgerQuery()
            {
                Take = 10,
                OrderBy = "AccountNo"
            };

            QueryResponse<JiwaFinancials.Jiwa.JiwaServiceModel.Tables.GL_Ledger> queryResponse;

            // Read general ledger accounts.
            queryResponse = await Client.GetAsync(queryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(queryResponse, Is.Not.Null);
            Assert.That(queryResponse.Results, Is.Not.Null);
            Assert.That(queryResponse.Results.Count, Is.GreaterThan(0));

            JiwaFinancials.Jiwa.JiwaServiceModel.Tables.GL_Ledger firstLedger = queryResponse.Results.First();
            Assert.That(firstLedger.GLLedgerID, Is.Not.Null.And.Not.Empty);
            Assert.That(firstLedger.AccountNo, Is.Not.Null.And.Not.Empty);

            // Read a known general ledger account using a filter.
            queryRequest = new GL_LedgerQuery()
            {
                GLLedgerID = firstLedger.GLLedgerID
            };

            queryResponse = await Client.GetAsync(queryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(queryResponse.Results, Is.Not.Null);
            Assert.That(queryResponse.Results.Any(x => x.GLLedgerID == firstLedger.GLLedgerID), Is.True);

            // Verify an invalid API key is rejected.
            using (JsonApiClient tempClient = new JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                WebServiceException ex = Assert.Throws<WebServiceException>(() => queryResponse = tempClient.Get(queryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }
        }
        #endregion
    }
}
