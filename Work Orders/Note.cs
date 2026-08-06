using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Notes;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder;
using WorkOrderNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.Note;
using WorkOrderNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;

namespace JiwaAPITests.WorkOrders
{
    public class Note : WorkOrderTestBase
    {
        #region "WorkOrders_Notes"
        [Test]
        public async Task WorkOrders_Notes_CRUD()
        {
            // Create a work order to append a note to
            WorkOrderDto workOrderCreateRes = await CreateWorkOrderAsync();

            // Read note types and use one for the note creation request
            WorkOrderNoteTypesGETManyRequest noteTypesGetManyReq = new WorkOrderNoteTypesGETManyRequest();
            List<WorkOrderNoteTypeDto> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a note to the work order
            WorkOrderNotePOSTRequest noteCreateReq = new WorkOrderNotePOSTRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                NoteText = "Work Order Note " + RandomString(12),
                NoteType = new WorkOrderNoteTypeDto() { NoteTypeID = noteTypesGetManyRes[0].NoteTypeID }
            };

            WorkOrderNoteDto noteCreateRes = await Client.PostAsync(noteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteCreateRes.NoteID, Is.Not.Null);
            Assert.That(noteCreateRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Read all work order notes and ensure the created note is returned
            WorkOrderNotesGETManyRequest notesGetManyReq = new WorkOrderNotesGETManyRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID
            };

            List<WorkOrderNoteDto> notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.True);

            // Read the created work order note
            WorkOrderNoteGETRequest noteGetReq = new WorkOrderNoteGETRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                NoteID = noteCreateRes.NoteID
            };

            WorkOrderNoteDto noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Update the work order note
            WorkOrderNotePATCHRequest notePatchReq = new WorkOrderNotePATCHRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                NoteID = noteCreateRes.NoteID,
                NoteText = "Updated Work Order Note " + RandomString(8)
            };

            WorkOrderNoteDto notePatchRes = await Client.PatchAsync(notePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(notePatchReq.NoteID));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(notePatchRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Read the updated work order note
            noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Delete the work order note
            WorkOrderNoteDELETERequest noteDeleteReq = new WorkOrderNoteDELETERequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                NoteID = noteCreateRes.NoteID
            };

            await Client.DeleteAsync(noteDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the work order note was deleted
            WebServiceException noteDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(noteGetReq);
            });
            Assert.That(noteDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all notes and ensure the deleted note is no longer returned
            notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.False);
        }
        #endregion
    }
}


