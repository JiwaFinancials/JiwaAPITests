using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder;
using PurchaseOrderNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.Note;
using PurchaseOrderNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;

namespace JiwaAPITests.PurchaseOrders
{
    public class Note : PurchaseOrderTestBase
    {
        #region "PurchaseOrders_Notes"
        [Test]
        public async Task PurchaseOrders_Notes_CRUD()
        {
            // Create the required creditor, inventory item and purchase order.
            (_, _, PurchaseOrderDto purchaseOrderCreateRes) = await CreatePurchaseOrderWithLineAsync();

            // Read note types and use one for the note creation request.
            PurchaseOrderNoteTypesGETManyRequest noteTypesGetManyReq = new PurchaseOrderNoteTypesGETManyRequest();
            List<PurchaseOrderNoteTypeDto> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a note to the purchase order.
            PurchaseOrderNotePOSTRequest noteCreateReq = new PurchaseOrderNotePOSTRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID,
                NoteText = "Purchase Order Note " + RandomString(12),
                NoteType = new PurchaseOrderNoteTypeDto() { NoteTypeID = noteTypesGetManyRes[0].NoteTypeID }
            };

            PurchaseOrderNoteDto noteCreateRes = await Client.PostAsync(noteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteCreateRes.NoteID, Is.Not.Null);
            Assert.That(noteCreateRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Read all purchase order notes and ensure the created note is returned.
            PurchaseOrderNotesGETManyRequest notesGetManyReq = new PurchaseOrderNotesGETManyRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID
            };

            List<PurchaseOrderNoteDto> notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.True);

            // Read the created purchase order note.
            PurchaseOrderNoteGETRequest noteGetReq = new PurchaseOrderNoteGETRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID,
                NoteID = noteCreateRes.NoteID
            };

            PurchaseOrderNoteDto noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Update the purchase order note.
            PurchaseOrderNotePATCHRequest notePatchReq = new PurchaseOrderNotePATCHRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID,
                NoteID = noteCreateRes.NoteID,
                NoteText = "Updated Purchase Order Note " + RandomString(8)
            };

            PurchaseOrderNoteDto notePatchRes = await Client.PatchAsync(notePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(notePatchRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Read the updated purchase order note.
            noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Delete the purchase order note.
            PurchaseOrderNoteDELETERequest noteDeleteReq = new PurchaseOrderNoteDELETERequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID,
                NoteID = noteCreateRes.NoteID
            };

            await Client.DeleteAsync(noteDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the purchase order note was deleted.
            WebServiceException noteDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(noteGetReq);
            });
            Assert.That(noteDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all notes and ensure the deleted note is no longer returned.
            notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.False);
        }
        #endregion
    }
}

