using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.JournalSets;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Threading.Tasks;
using JournalSetDto = JiwaFinancials.Jiwa.JiwaServiceModel.JournalSets.JournalSet;

namespace JiwaAPITests.JournalSets
{
    public abstract class JournalSetsTestBase : JiwaAPITest
    {
        protected async Task<JournalSetDto> CreateJournalSetAsync()
        {
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

            JiwaFinancials.Jiwa.JiwaServiceModel.Tables.GL_Ledger secondLedger = queryResponse.Results.Skip(1).First();
            Assert.That(secondLedger.GLLedgerID, Is.Not.Null.And.Not.Empty);
            Assert.That(secondLedger.AccountNo, Is.Not.Null.And.Not.Empty);

            // Create a journal set.
            JournalSetPOSTRequest journalSetCreateReq = new JournalSetPOSTRequest()
            {
                Description = "Journal Set " + RandomString(8),
                SetType = SetTypes.Pending,
                PostedDate = DateTime.Today,
                Lines = new System.Collections.Generic.List<JournalSetLine>()
                {
                    new JournalSetLine()
                    {
                        GeneralLedgerAccountRecID = firstLedger.GLLedgerID,
                        DebitAmount = 1.00m
                    },
                    new JournalSetLine()
                    {
                        GeneralLedgerAccountRecID = secondLedger.GLLedgerID,
                        CreditAmount = 1.00m
                    }
                }
            };

            JournalSetDto journalSetCreateRes = await Client.PostAsync(journalSetCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(journalSetCreateRes.JournalSetID, Is.Not.Null);
            Assert.That(journalSetCreateRes.Description, Is.EqualTo(journalSetCreateReq.Description));
            return journalSetCreateRes;
        }
    }
}

