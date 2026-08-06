using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using ServiceStack;
using CreditorNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.Note;
using CreditorNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Creditors
{
    public class Note : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task CreditorNote_CRUD()
        {
            // Create a creditor to append a note to
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Creditor Note Test"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);
            Assert.That(creditorCreateRes.AccountNo, Is.EqualTo(creditorCreateReq.AccountNo));

            // Read note types and use one for the note creation request
            CreditorNoteTypesGETManyRequest noteTypesGetManyReq = new CreditorNoteTypesGETManyRequest();
            List<CreditorNoteTypeDto> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a note to the creditor
            CreditorNotePOSTRequest noteCreateReq = new CreditorNotePOSTRequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                NoteText = "Creditor Note " + RandomString(12),
                NoteType = new CreditorNoteTypeDto() { NoteTypeID = noteTypesGetManyRes[0].NoteTypeID }
            };

            CreditorNoteDto noteCreateRes = await Client.PostAsync(noteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteCreateRes.NoteID, Is.Not.Null);
            Assert.That(noteCreateRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Read all creditor notes and ensure the created note is returned
            CreditorNotesGETManyRequest notesGetManyReq = new CreditorNotesGETManyRequest()
            {
                CreditorID = creditorCreateRes.CreditorID
            };

            List<CreditorNoteDto> notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.True);

            // Read the created creditor note using the NoteID
            CreditorNoteGETRequest noteGetReq = new CreditorNoteGETRequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                NoteID = noteCreateRes.NoteID
            };

            CreditorNoteDto noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Update the creditor note
            CreditorNotePATCHRequest notePatchReq = new CreditorNotePATCHRequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                NoteID = noteCreateRes.NoteID,
                NoteText = "Updated Creditor Note " + RandomString(8)
            };

            CreditorNoteDto notePatchRes = await Client.PatchAsync(notePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(notePatchReq.NoteID));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(notePatchRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Read the updated creditor note using the NoteID
            noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Remove the created creditor note
            CreditorNoteDELETERequest noteDeleteReq = new CreditorNoteDELETERequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                NoteID = noteCreateRes.NoteID
            };

            await Client.DeleteAsync(noteDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted creditor note is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                CreditorNoteDto getDeletedRes = await Client.GetAsync(noteGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all creditor notes and ensure the deleted note is no longer returned
            notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.False);

            // Remove the created creditor
            CreditorDELETERequest creditorDeleteReq = new CreditorDELETERequest()
            {
                CreditorID = creditorCreateRes.CreditorID
            };

            await Client.DeleteAsync(creditorDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}


