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
    public class NoteTypes : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Debtor_NoteTypes_CRUD()
        {
            // Get the list of note types
            DebtorNoteTypesGETManyRequest noteTypesGetListReq = new DebtorNoteTypesGETManyRequest();

            List<NoteType> noteTypesGetListRes = await Client.GetAsync(noteTypesGetListReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetListRes, Is.Not.Null);

            // Create a new note type
            DebtorNoteTypePOSTRequest noteTypePOSTReq = new DebtorNoteTypePOSTRequest()
            {
                Description = "Test Note Type " + RandomString(5)
            };

            NoteType noteTypePOSTRes = await Client.PostAsync(noteTypePOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteTypePOSTRes.NoteTypeID, !Is.Null);
            Assert.That(noteTypePOSTRes.Description, Is.EqualTo(noteTypePOSTReq.Description));

            string noteTypeID = noteTypePOSTRes.NoteTypeID;

            // Read the created note type
            DebtorNoteTypeGETRequest noteTypeGETReq = new DebtorNoteTypeGETRequest()
            {
                NoteTypeID = noteTypeID
            };

            NoteType noteTypeGETRes = await Client.GetAsync(noteTypeGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypeGETRes.NoteTypeID, Is.EqualTo(noteTypeID));
            Assert.That(noteTypeGETRes.Description, Is.EqualTo(noteTypePOSTReq.Description));

            // Update the note type
            DebtorNoteTypePATCHRequest noteTypePATCHReq = new DebtorNoteTypePATCHRequest()
            {
                NoteTypeID = noteTypeID,
                Description = "Updated Test Note Type " + RandomString(5)
            };

            NoteType noteTypePATCHRes = await Client.PatchAsync(noteTypePATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypePATCHRes.Description, Is.EqualTo(noteTypePATCHReq.Description));

            // Delete the note type
            DebtorNoteTypeDELETERequest noteTypeDELETEReq = new DebtorNoteTypeDELETERequest()
            {
                NoteTypeID = noteTypeID
            };

            await Client.DeleteAsync(noteTypeDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the deleted note type is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                NoteType getDeletedRes = await Client.GetAsync(noteTypeGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Try to GET non-existent note type to make sure we get a 404
            noteTypeGETReq.NoteTypeID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                NoteType noteTypeGetRes = await Client.GetAsync(noteTypeGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}

