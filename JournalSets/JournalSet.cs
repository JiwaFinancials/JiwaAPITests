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
    public class JournalSet : JournalSetsTestBase
    {
        #region "JournalSets_Core"
        [Test]
        public async Task JournalSets_CRUD()
        {
            // Create a journal set.
            JournalSetDto journalSetCreateRes = await CreateJournalSetAsync();

            // Read the created journal set.
            JournalSetGETRequest journalSetGetReq = new JournalSetGETRequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID
            };

            JournalSetDto journalSetGetRes = await Client.GetAsync(journalSetGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(journalSetGetRes.JournalSetID, Is.EqualTo(journalSetCreateRes.JournalSetID));

            // Update the journal set.
            JournalSetPATCHRequest journalSetPatchReq = new JournalSetPATCHRequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID,
                Description = "Updated Journal Set " + RandomString(6),
                SetType = SetTypes.Template
            };

            JournalSetDto journalSetPatchRes = await Client.PatchAsync(journalSetPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(journalSetPatchRes.JournalSetID, Is.EqualTo(journalSetPatchReq.JournalSetID));
            Assert.That(journalSetPatchRes.JournalSetID, Is.EqualTo(journalSetCreateRes.JournalSetID));
            Assert.That(journalSetPatchRes.Description, Is.EqualTo(journalSetPatchReq.Description));

            // Verify the journal set was updated.
            journalSetGetRes = await Client.GetAsync(journalSetGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(journalSetGetRes.Description, Is.EqualTo(journalSetPatchReq.Description));
            Assert.That(journalSetGetRes.SetType, Is.EqualTo(journalSetPatchReq.SetType));

            // Delete the journal set.
            JournalSetDELETERequest journalSetDeleteReq = new JournalSetDELETERequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID
            };

            await Client.DeleteAsync(journalSetDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the journal set was deleted.
            WebServiceException journalSetDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(journalSetGetReq);
            });
            Assert.That(journalSetDeleteEx.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}

