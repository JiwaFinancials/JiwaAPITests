using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseInvoiceDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseInvoices.PurchaseInvoice;
using PurchaseInvoiceNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.Note;
using PurchaseInvoiceNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;

namespace JiwaAPITests.PurchaseInvoices
{
    public class Note : PurchaseInvoiceTestBase
    {
        #region "PurchaseInvoices_Notes"
        [Test]
        public async Task PurchaseInvoices_Notes_CRUD()
        {
            // Create dependencies used to create a purchase invoice.
            (_, _, _, var goodsReceivedNote) = await CreateGoodsReceivedNoteWithDependenciesAsync();

            // Create a purchase invoice to append notes to.
            PurchaseInvoiceDto purchaseInvoiceCreateRes = await CreatePurchaseInvoiceFromGoodsReceivedNoteAsync(goodsReceivedNote);

            // Read note types and use one for note creation.
            PurchaseInvoiceNoteTypesGETManyRequest noteTypesGetManyReq = new PurchaseInvoiceNoteTypesGETManyRequest();
            List<PurchaseInvoiceNoteTypeDto> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a note to the purchase invoice.
            PurchaseInvoiceNotePOSTRequest noteCreateReq = new PurchaseInvoiceNotePOSTRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID,
                NoteText = "Purchase Invoice Note " + RandomString(12),
                NoteType = new PurchaseInvoiceNoteTypeDto() { NoteTypeID = noteTypesGetManyRes[0].NoteTypeID }
            };

            PurchaseInvoiceNoteDto noteCreateRes = await Client.PostAsync(noteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteCreateRes.NoteID, Is.Not.Null);
            Assert.That(noteCreateRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Read all purchase invoice notes and ensure the created note is returned.
            PurchaseInvoiceNotesGETManyRequest notesGetManyReq = new PurchaseInvoiceNotesGETManyRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID
            };

            List<PurchaseInvoiceNoteDto> notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.True);

            // Read the created purchase invoice note.
            PurchaseInvoiceNoteGETRequest noteGetReq = new PurchaseInvoiceNoteGETRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID,
                NoteID = noteCreateRes.NoteID
            };

            PurchaseInvoiceNoteDto noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Update the purchase invoice note.
            PurchaseInvoiceNotePATCHRequest notePatchReq = new PurchaseInvoiceNotePATCHRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID,
                NoteID = noteCreateRes.NoteID,
                NoteText = "Updated Purchase Invoice Note " + RandomString(8)
            };

            PurchaseInvoiceNoteDto notePatchRes = await Client.PatchAsync(notePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(notePatchRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Read the updated purchase invoice note.
            noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Delete the purchase invoice note.
            PurchaseInvoiceNoteDELETERequest noteDeleteReq = new PurchaseInvoiceNoteDELETERequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID,
                NoteID = noteCreateRes.NoteID
            };

            await Client.DeleteAsync<object>(noteDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the purchase invoice note was deleted.
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

