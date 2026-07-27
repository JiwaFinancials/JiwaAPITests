using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.JournalSets;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JournalSetNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;

namespace JiwaAPITests.JournalSets
{
    public class NoteTypes : JournalSetsTestBase
    {
        #region "JournalSets_NoteTypes"
        [Test]
        public async Task JournalSets_NoteTypes_CRUD()
        {
            // Create a journal set note type.
            JournalSetNoteTypePOSTRequest noteTypeCreateReq = new JournalSetNoteTypePOSTRequest()
            {
                Description = RandomString(12),
                DefaultType = false
            };

            JournalSetNoteTypeDto noteTypeCreateRes = await Client.PostAsync(noteTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteTypeCreateRes.NoteTypeID, Is.Not.Null);
            Assert.That(noteTypeCreateRes.Description, Is.EqualTo(noteTypeCreateReq.Description));

            // Read all journal set note types.
            JournalSetNoteTypesGETManyRequest noteTypesGetManyReq = new JournalSetNoteTypesGETManyRequest();
            List<JournalSetNoteTypeDto> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Any(x => x.NoteTypeID == noteTypeCreateRes.NoteTypeID), Is.True);

            // Read the created journal set note type.
            JournalSetNoteTypeGETRequest noteTypeGetReq = new JournalSetNoteTypeGETRequest()
            {
                NoteTypeID = noteTypeCreateRes.NoteTypeID
            };

            JournalSetNoteTypeDto noteTypeGetRes = await Client.GetAsync(noteTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypeGetRes.NoteTypeID, Is.EqualTo(noteTypeCreateRes.NoteTypeID));
            Assert.That(noteTypeGetRes.Description, Is.EqualTo(noteTypeCreateReq.Description));

            // Update the journal set note type.
            JournalSetNoteTypePATCHRequest noteTypePatchReq = new JournalSetNoteTypePATCHRequest()
            {
                NoteTypeID = noteTypeCreateRes.NoteTypeID,
                Description = "Updated " + RandomString(10)
            };

            JournalSetNoteTypeDto noteTypePatchRes = await Client.PatchAsync(noteTypePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypePatchRes.NoteTypeID, Is.EqualTo(noteTypeCreateRes.NoteTypeID));
            Assert.That(noteTypePatchRes.Description, Is.EqualTo(noteTypePatchReq.Description));

            // Delete the journal set note type.
            JournalSetNoteTypeDELETERequest noteTypeDeleteReq = new JournalSetNoteTypeDELETERequest()
            {
                NoteTypeID = noteTypeCreateRes.NoteTypeID
            };

            await Client.DeleteAsync(noteTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the journal set note type was deleted.
            WebServiceException noteTypeDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(noteTypeGetReq);
            });
            Assert.That(noteTypeDeleteEx.StatusCode, Is.EqualTo(404));

            noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Any(x => x.NoteTypeID == noteTypeCreateRes.NoteTypeID), Is.False);
        }
        #endregion
    }
}

