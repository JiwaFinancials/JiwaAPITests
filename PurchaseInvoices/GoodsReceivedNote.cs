using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseInvoiceGoodsReceivedNoteInvoicedDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseInvoices.PurchaseInvoiceGoodsReceivedNoteInvoiced;
using PurchaseInvoiceDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseInvoices.PurchaseInvoice;

namespace JiwaAPITests.PurchaseInvoices
{
    public class GoodsReceivedNote : PurchaseInvoiceTestBase
    {
        #region "PurchaseInvoices_GoodsReceivedNotes"
        [Test]
        public async Task PurchaseInvoices_GoodsReceivedNotes_CRUD()
        {
            // Create dependencies used by the purchase invoice.
            (_, _, _, var goodsReceivedNote) = await CreateGoodsReceivedNoteWithDependenciesAsync();

            // Create a purchase invoice from the goods received note.
            PurchaseInvoiceDto purchaseInvoiceCreateRes = await CreatePurchaseInvoiceFromGoodsReceivedNoteAsync(goodsReceivedNote);

            // Read all goods received notes for the purchase invoice.
            PurchaseInvoiceGoodsReceivedNotesGETManyRequest goodsReceivedNotesGetManyReq = new PurchaseInvoiceGoodsReceivedNotesGETManyRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID
            };

            List<PurchaseInvoiceGoodsReceivedNoteInvoicedDto> goodsReceivedNotesGetManyRes = await Client.GetAsync(goodsReceivedNotesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(goodsReceivedNotesGetManyRes.Any(x => x.GRNID == goodsReceivedNote.GRNID), Is.True);

            // Read the created purchase invoice goods received note.
            PurchaseInvoiceGoodsReceivedNoteGETRequest goodsReceivedNoteGetReq = new PurchaseInvoiceGoodsReceivedNoteGETRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID,
                GRNID = goodsReceivedNote.GRNID
            };

            PurchaseInvoiceGoodsReceivedNoteInvoicedDto goodsReceivedNoteGetRes = await Client.GetAsync(goodsReceivedNoteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(goodsReceivedNoteGetRes.GRNID, Is.EqualTo(goodsReceivedNote.GRNID));

            // Delete the goods received note from the purchase invoice.
            PurchaseInvoiceGoodsReceivedNoteDELETERequest goodsReceivedNoteDeleteReq = new PurchaseInvoiceGoodsReceivedNoteDELETERequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID,
                GRNID = goodsReceivedNote.GRNID
            };

            await Client.DeleteAsync(goodsReceivedNoteDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the purchase invoice goods received note was deleted.
            WebServiceException goodsReceivedNoteDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(goodsReceivedNoteGetReq);
            });
            Assert.That(goodsReceivedNoteDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all goods received notes and ensure the deleted note is no longer returned.
            goodsReceivedNotesGetManyRes = await Client.GetAsync(goodsReceivedNotesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(goodsReceivedNotesGetManyRes.Any(x => x.GRNID == goodsReceivedNote.GRNID), Is.False);

            // Append the goods received note back to the purchase invoice.
            PurchaseInvoiceGoodsReceivedNotePOSTRequest goodsReceivedNoteCreateReq = new PurchaseInvoiceGoodsReceivedNotePOSTRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID,
                GRNID = goodsReceivedNote.GRNID
            };

            PurchaseInvoiceGoodsReceivedNoteInvoicedDto goodsReceivedNoteCreateRes = await Client.PostAsync(goodsReceivedNoteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(goodsReceivedNoteCreateRes.GRNID, Is.EqualTo(goodsReceivedNote.GRNID));

            // Read the re-appended purchase invoice goods received note.
            goodsReceivedNoteGetRes = await Client.GetAsync(goodsReceivedNoteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(goodsReceivedNoteGetRes.GRNID, Is.EqualTo(goodsReceivedNote.GRNID));
        }
        #endregion
    }
}

