using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Bills;
using ServiceStack;
using BillNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Bills
{
    public class NoteType : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task BillNoteType_CRUD()
        {
            // Create a note type
            BillNoteTypePOSTRequest noteTypeCreateReq = new BillNoteTypePOSTRequest()
            {
                Description = RandomString(12),
                DefaultType = false
            };

            BillNoteTypeDto noteTypeCreateRes = await Client.PostAsync(noteTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteTypeCreateRes.NoteTypeID, Is.Not.Null);
            Assert.That(noteTypeCreateRes.Description, Is.EqualTo(noteTypeCreateReq.Description));

            // Read all note types and ensure the created note type is returned
            BillNoteTypesGETManyRequest noteTypesGetManyReq = new BillNoteTypesGETManyRequest();
            List<BillNoteTypeDto> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Any(x => x.NoteTypeID == noteTypeCreateRes.NoteTypeID), Is.True);

            // Read the created note type using the NoteTypeID
            BillNoteTypeGETRequest noteTypeGetReq = new BillNoteTypeGETRequest()
            {
                NoteTypeID = noteTypeCreateRes.NoteTypeID
            };

            BillNoteTypeDto noteTypeGetRes = await Client.GetAsync(noteTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypeGetRes.NoteTypeID, Is.EqualTo(noteTypeCreateRes.NoteTypeID));
            Assert.That(noteTypeGetRes.Description, Is.EqualTo(noteTypeCreateReq.Description));

            // Update the note type
            BillNoteTypePATCHRequest noteTypePatchReq = new BillNoteTypePATCHRequest()
            {
                NoteTypeID = noteTypeCreateRes.NoteTypeID,
                Description = "Updated " + RandomString(10)
            };

            BillNoteTypeDto noteTypePatchRes = await Client.PatchAsync(noteTypePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypePatchRes.NoteTypeID, Is.EqualTo(noteTypeCreateRes.NoteTypeID));
            Assert.That(noteTypePatchRes.Description, Is.EqualTo(noteTypePatchReq.Description));

            // Read the updated note type using the NoteTypeID
            noteTypeGetRes = await Client.GetAsync(noteTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypeGetRes.Description, Is.EqualTo(noteTypePatchReq.Description));

            // Remove the created note type
            BillNoteTypeDELETERequest noteTypeDeleteReq = new BillNoteTypeDELETERequest()
            {
                NoteTypeID = noteTypeCreateRes.NoteTypeID
            };

            await Client.DeleteAsync(noteTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted note type is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                BillNoteTypeDto getDeletedRes = await Client.GetAsync(noteTypeGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all note types and ensure the deleted note type is no longer returned
            noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Any(x => x.NoteTypeID == noteTypeCreateRes.NoteTypeID), Is.False);
        }
        #endregion
    }
}

