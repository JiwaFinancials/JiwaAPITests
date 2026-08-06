using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.JournalSets;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JournalSetDto = JiwaFinancials.Jiwa.JiwaServiceModel.JournalSets.JournalSet;
using JournalSetNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.Note;
using JournalSetNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;

namespace JiwaAPITests.JournalSets
{
    public class Notes : JournalSetsTestBase
    {
        #region "JournalSets_Notes"
        [Test]
        public async Task JournalSets_Notes_CRUD()
        {
            // Create a journal set to append notes to.
            JournalSetDto journalSetCreateRes = await CreateJournalSetAsync();

            // Read note types and use one for note creation.
            JournalSetNoteTypesGETManyRequest noteTypesGetManyReq = new JournalSetNoteTypesGETManyRequest();
            List<JournalSetNoteTypeDto> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a note to the journal set.
            JournalSetNotePOSTRequest noteCreateReq = new JournalSetNotePOSTRequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID,
                NoteText = "Journal Set Note " + RandomString(12),
                NoteType = new JournalSetNoteTypeDto() { NoteTypeID = noteTypesGetManyRes[0].NoteTypeID }
            };

            JournalSetNoteDto noteCreateRes = await Client.PostAsync(noteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteCreateRes.NoteID, Is.Not.Null);
            Assert.That(noteCreateRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Read all notes for the journal set.
            JournalSetNotesGETManyRequest notesGetManyReq = new JournalSetNotesGETManyRequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID
            };

            List<JournalSetNoteDto> notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.True);

            // Read the appended note.
            JournalSetNoteGETRequest noteGetReq = new JournalSetNoteGETRequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID,
                NoteID = noteCreateRes.NoteID
            };

            JournalSetNoteDto noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Update the journal set note.
            JournalSetNotePATCHRequest notePatchReq = new JournalSetNotePATCHRequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID,
                NoteID = noteCreateRes.NoteID,
                NoteText = "Updated Journal Set Note " + RandomString(8)
            };

            JournalSetNoteDto notePatchRes = await Client.PatchAsync(notePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(notePatchReq.NoteID));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(notePatchRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Delete the journal set note.
            JournalSetNoteDELETERequest noteDeleteReq = new JournalSetNoteDELETERequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID,
                NoteID = noteCreateRes.NoteID
            };

            await Client.DeleteAsync(noteDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the journal set note was deleted.
            WebServiceException noteDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(noteGetReq);
            });
            Assert.That(noteDeleteEx.StatusCode, Is.EqualTo(404));

            notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.False);
        }
        #endregion
    }
}


