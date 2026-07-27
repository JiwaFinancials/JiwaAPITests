using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceManagerTaskNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;

namespace JiwaAPITests.ServiceManager
{
    public class NoteTypes : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_NoteTypes_CRUD()
        {
            // Create a service manager task note type.
            ServiceManagerTaskNoteTypePOSTRequest noteTypeCreateReq = new ServiceManagerTaskNoteTypePOSTRequest()
            {
                Description = "Task Note Type " + RandomString(8),
                DefaultType = false
            };

            ServiceManagerTaskNoteTypeDto noteTypeCreateRes = await Client.PostAsync(noteTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteTypeCreateRes.NoteTypeID, Is.Not.Null);
            Assert.That(noteTypeCreateRes.Description, Is.EqualTo(noteTypeCreateReq.Description));

            // Read all service manager task note types.
            ServiceManagerTaskNoteTypesGETManyRequest noteTypesGetManyReq = new ServiceManagerTaskNoteTypesGETManyRequest();
            List<ServiceManagerTaskNoteTypeDto> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Any(x => x.NoteTypeID == noteTypeCreateRes.NoteTypeID), Is.True);

            // Read the created service manager task note type.
            ServiceManagerTaskNoteTypeGETRequest noteTypeGetReq = new ServiceManagerTaskNoteTypeGETRequest()
            {
                NoteTypeID = noteTypeCreateRes.NoteTypeID
            };

            ServiceManagerTaskNoteTypeDto noteTypeGetRes = await Client.GetAsync(noteTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypeGetRes.NoteTypeID, Is.EqualTo(noteTypeCreateRes.NoteTypeID));
            Assert.That(noteTypeGetRes.Description, Is.EqualTo(noteTypeCreateReq.Description));

            // Update the service manager task note type.
            ServiceManagerTaskNoteTypePATCHRequest noteTypePatchReq = new ServiceManagerTaskNoteTypePATCHRequest()
            {
                NoteTypeID = noteTypeCreateRes.NoteTypeID,
                Description = "Updated Task Note Type " + RandomString(8),
                DefaultType = false
            };

            ServiceManagerTaskNoteTypeDto noteTypePatchRes = await Client.PatchAsync(noteTypePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypePatchRes.NoteTypeID, Is.EqualTo(noteTypeCreateRes.NoteTypeID));
            Assert.That(noteTypePatchRes.Description, Is.EqualTo(noteTypePatchReq.Description));

            // Verify the service manager task note type was updated.
            noteTypeGetRes = await Client.GetAsync(noteTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypeGetRes.Description, Is.EqualTo(noteTypePatchReq.Description));

            // Delete the service manager task note type.
            ServiceManagerTaskNoteTypeDELETERequest noteTypeDeleteReq = new ServiceManagerTaskNoteTypeDELETERequest()
            {
                NoteTypeID = noteTypeCreateRes.NoteTypeID
            };

            await Client.DeleteAsync(noteTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the service manager task note type was deleted.
            WebServiceException noteTypeDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(noteTypeGetReq);
            });
            Assert.That(noteTypeDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all service manager task note types and ensure the deleted type is no longer returned.
            noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Any(x => x.NoteTypeID == noteTypeCreateRes.NoteTypeID), Is.False);
        }
        #endregion
    }
}

