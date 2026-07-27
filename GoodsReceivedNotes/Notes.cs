using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoodsReceivedNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes.GoodsReceivedNote;
using GoodsReceivedNoteItemNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.Note;
using GoodsReceivedNoteItemNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;

namespace JiwaAPITests.GoodsReceivedNotes
{
    public class Notes : GoodsReceivedNotesTestBase
    {
        #region "GoodsReceivedNotes_Notes"
        [Test]
        public async Task GoodsReceivedNotes_Notes_CRUD()
        {
            // Create the required creditor and purchase order dependencies.
            (Creditor creditor, JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrder) = await CreatePurchaseOrderWithLineAsync();

            // Create a goods received note to append notes to.
            GoodsReceivedNoteDto grnCreateRes = await CreateGoodsReceivedNoteAsync(creditor);

            // Read note types and use one for note creation.
            GoodsReceivedNoteNoteTypesGETManyRequest noteTypesGetManyReq = new GoodsReceivedNoteNoteTypesGETManyRequest();
            List<GoodsReceivedNoteItemNoteTypeDto> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a note to the goods received note.
            GoodsReceivedNoteNotePOSTRequest noteCreateReq = new GoodsReceivedNoteNotePOSTRequest()
            {
                GRNID = grnCreateRes.GRNID,
                NoteText = "Goods Received Note Note " + RandomString(12),
                NoteType = new GoodsReceivedNoteItemNoteTypeDto() { NoteTypeID = noteTypesGetManyRes[0].NoteTypeID }
            };

            GoodsReceivedNoteItemNoteDto noteCreateRes = await Client.PostAsync(noteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteCreateRes.NoteID, Is.Not.Null);
            Assert.That(noteCreateRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Read all notes for the goods received note.
            GoodsReceivedNoteNotesGETManyRequest notesGetManyReq = new GoodsReceivedNoteNotesGETManyRequest()
            {
                GRNID = grnCreateRes.GRNID
            };

            List<GoodsReceivedNoteItemNoteDto> notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.True);

            // Read the appended goods received note note.
            GoodsReceivedNoteNoteGETRequest noteGetReq = new GoodsReceivedNoteNoteGETRequest()
            {
                GRNID = grnCreateRes.GRNID,
                NoteID = noteCreateRes.NoteID
            };

            GoodsReceivedNoteItemNoteDto noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Update the goods received note note.
            GoodsReceivedNoteNotePATCHRequest notePatchReq = new GoodsReceivedNoteNotePATCHRequest()
            {
                GRNID = grnCreateRes.GRNID,
                NoteID = noteCreateRes.NoteID,
                NoteText = "Updated Goods Received Note " + RandomString(8)
            };

            GoodsReceivedNoteItemNoteDto notePatchRes = await Client.PatchAsync(notePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(notePatchRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Delete the goods received note note.
            GoodsReceivedNoteNoteDELETERequest noteDeleteReq = new GoodsReceivedNoteNoteDELETERequest()
            {
                GRNID = grnCreateRes.GRNID,
                NoteID = noteCreateRes.NoteID
            };

            await Client.DeleteAsync(noteDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the note was deleted.
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

