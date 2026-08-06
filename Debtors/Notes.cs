using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Notes;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Debtors
{
    public class Notes : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Debtor_Notes_CRUD()
        {
            // Create a debtor we can operate on
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

            try
            {
                // Get the list of notes (initially empty)
                DebtorNotesGETManyRequest notesGetListReq = new DebtorNotesGETManyRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID
                };

                List<Note> notesGetListRes = await Client.GetAsync(notesGetListReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(notesGetListRes, Is.Not.Null);

                // Add a note to the debtor
                DebtorNotePOSTRequest notePOSTReq = new DebtorNotePOSTRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    NoteText = "Test Note"
                };

                Note notePOSTRes = await Client.PostAsync(notePOSTReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                Assert.That(notePOSTRes.NoteID, !Is.Null);
                Assert.That(notePOSTRes.NoteText, Is.EqualTo(notePOSTReq.NoteText));

                string noteID = notePOSTRes.NoteID;

                // Read the created note
                DebtorNoteGETRequest noteGETReq = new DebtorNoteGETRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    NoteID = noteID
                };

                Note noteGETRes = await Client.GetAsync(noteGETReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(noteGETRes.NoteID, Is.EqualTo(noteID));
                Assert.That(noteGETRes.NoteText, Is.EqualTo(notePOSTReq.NoteText));

                // Update the note
                DebtorNotePATCHRequest notePATCHReq = new DebtorNotePATCHRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    NoteID = noteID,
                    NoteText = "Updated Test Note"
                };

                Note notePATCHRes = await Client.PatchAsync(notePATCHReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(notePATCHRes.NoteID, Is.EqualTo(notePATCHReq.NoteID));
                Assert.That(notePATCHRes.NoteText, Is.EqualTo(notePATCHReq.NoteText));

                // Delete the note
                DebtorNoteDELETERequest noteDELETEReq = new DebtorNoteDELETERequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    NoteID = noteID
                };

                await Client.DeleteAsync(noteDELETEReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

                // Verify the deleted note is not there anymore
                WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
                {
                    Note getDeletedRes = await Client.GetAsync(noteGETReq);
                });
                Assert.That(ex.StatusCode, Is.EqualTo(404));
            }
            finally
            {
                // Clean up: Remove the created debtor
                DebtorDELETERequest debtorDeleteReq = new DebtorDELETERequest() { DebtorID = debtorCreateRes.DebtorID };
                await Client.DeleteAsync(debtorDeleteReq);
            }
        }
        #endregion
    }
}


