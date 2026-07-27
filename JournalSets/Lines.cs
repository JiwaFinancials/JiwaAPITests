using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.JournalSets;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JournalSetDto = JiwaFinancials.Jiwa.JiwaServiceModel.JournalSets.JournalSet;

namespace JiwaAPITests.JournalSets
{
    public class Lines : JournalSetsTestBase
    {
        #region "JournalSets_Lines"
        [Test]
        public async Task JournalSets_Lines_CRUD()
        {
            // Create a journal set to append lines to.
            JournalSetDto journalSetCreateRes = await CreateJournalSetAsync();

            // Read general ledger accounts.
            GL_LedgerQuery queryRequest = new GL_LedgerQuery()
            {
                Take = 2,
                OrderBy = "AccountNo",
                IsEnabled = true,
                PostingAcc = 1
            };

            QueryResponse<JiwaFinancials.Jiwa.JiwaServiceModel.Tables.GL_Ledger> queryResponse;

            queryResponse = await Client.GetAsync(queryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(queryResponse, Is.Not.Null);
            Assert.That(queryResponse.Results, Is.Not.Null);
            Assert.That(queryResponse.Results.Count, Is.GreaterThan(0));

            JiwaFinancials.Jiwa.JiwaServiceModel.Tables.GL_Ledger firstLedger = queryResponse.Results.First();
            Assert.That(firstLedger.GLLedgerID, Is.Not.Null.And.Not.Empty);
            Assert.That(firstLedger.AccountNo, Is.Not.Null.And.Not.Empty);

            // Append a line to the journal set.
            JournalSetLinePOSTRequest lineCreateReq = new JournalSetLinePOSTRequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID,
                Reference = "Journal Line " + RandomString(6),
                Remark = "Journal set line",
                DebitAmount = 1M,
                GeneralLedgerAccountRecID= firstLedger.GLLedgerID,
            };

            JournalSetLine lineCreateRes = await Client.PostAsync(lineCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(lineCreateRes.JournalSetLineID, Is.Not.Null);
            Assert.That(lineCreateRes.Reference, Is.EqualTo(lineCreateReq.Reference));
            Assert.That(lineCreateRes.GeneralLedgerAccountRecID, Is.EqualTo(lineCreateReq.GeneralLedgerAccountRecID));  

            // Read all lines for the journal set.
            JournalSetLinesGETManyRequest linesGetManyReq = new JournalSetLinesGETManyRequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID
            };

            List<JournalSetLine> linesGetManyRes = await Client.GetAsync(linesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linesGetManyRes.Any(x => x.JournalSetLineID == lineCreateRes.JournalSetLineID), Is.True);

            // Read the appended journal set line.
            JournalSetLineGETRequest lineGetReq = new JournalSetLineGETRequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID,
                JournalSetLineID = lineCreateRes.JournalSetLineID
            };

            JournalSetLine lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.JournalSetLineID, Is.EqualTo(lineCreateRes.JournalSetLineID));

            // Update the journal set line.
            JournalSetLinePATCHRequest linePatchReq = new JournalSetLinePATCHRequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID,
                JournalSetLineID = lineCreateRes.JournalSetLineID,
                Reference = "Updated Journal Line " + RandomString(4),
                DebitAmount = 12M,
                CreditAmount = 0M
            };

            JournalSetLine linePatchRes = await Client.PatchAsync(linePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linePatchRes.JournalSetLineID, Is.EqualTo(lineCreateRes.JournalSetLineID));
            Assert.That(linePatchRes.Reference, Is.EqualTo(linePatchReq.Reference));

            // Delete the journal set line.
            JournalSetLineDELETERequest lineDeleteReq = new JournalSetLineDELETERequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID,
                JournalSetLineID = lineCreateRes.JournalSetLineID
            };

            await Client.DeleteAsync(lineDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the journal set line was deleted.
            WebServiceException lineDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(lineGetReq);
            });
            Assert.That(lineDeleteEx.StatusCode, Is.EqualTo(404));

            linesGetManyRes = await Client.GetAsync(linesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linesGetManyRes.Any(x => x.JournalSetLineID == lineCreateRes.JournalSetLineID), Is.False);
        }
        #endregion
    }
}

