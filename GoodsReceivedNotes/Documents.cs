using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodsReceivedNoteDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;
using GoodsReceivedNoteDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;
using GoodsReceivedNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes.GoodsReceivedNote;

namespace JiwaAPITests.GoodsReceivedNotes
{
    public class Documents : GoodsReceivedNotesTestBase
    {
        #region "GoodsReceivedNotes_Documents"
        [Test]
        public async Task GoodsReceivedNotes_Documents_CRUD()
        {
            // Create the required creditor and purchase order dependencies.
            (Creditor creditor, JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrder) = await CreatePurchaseOrderWithLineAsync();

            // Create a goods received note to append documents to.
            GoodsReceivedNoteDto grnCreateRes = await CreateGoodsReceivedNoteAsync(creditor);

            // Read document types and use one for document creation.
            GoodsReceivedNoteDocumentTypesGETManyRequest documentTypesGetManyReq = new GoodsReceivedNoteDocumentTypesGETManyRequest();
            List<GoodsReceivedNoteDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a document to the goods received note.
            GoodsReceivedNoteDocumentPOSTRequest documentCreateReq = new GoodsReceivedNoteDocumentPOSTRequest()
            {
                GRNID = grnCreateRes.GRNID,
                Description = "Goods Received Note Document " + RandomString(8),
                PhysicalFileName = "GoodsReceivedNoteDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("Goods received note document content"),
                DocumentType = new GoodsReceivedNoteDocumentTypeDto() { DocumentTypeID = documentTypesGetManyRes[0].DocumentTypeID }
            };

            GoodsReceivedNoteDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all documents for the goods received note.
            GoodsReceivedNoteDocumentsGETManyRequest documentsGetManyReq = new GoodsReceivedNoteDocumentsGETManyRequest()
            {
                GRNID = grnCreateRes.GRNID
            };

            List<GoodsReceivedNoteDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the appended document for the goods received note.
            GoodsReceivedNoteDocumentGETRequest documentGetReq = new GoodsReceivedNoteDocumentGETRequest()
            {
                GRNID = grnCreateRes.GRNID,
                DocumentID = documentCreateRes.DocumentID
            };

            GoodsReceivedNoteDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));

            // Update the goods received note document.
            GoodsReceivedNoteDocumentPATCHRequest documentPatchReq = new GoodsReceivedNoteDocumentPATCHRequest()
            {
                GRNID = grnCreateRes.GRNID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated Goods Received Note Document " + RandomString(6)
            };

            GoodsReceivedNoteDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Delete the goods received note document.
            GoodsReceivedNoteDocumentDELETERequest documentDeleteReq = new GoodsReceivedNoteDocumentDELETERequest()
            {
                GRNID = grnCreateRes.GRNID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the goods received note document was deleted.
            WebServiceException documentDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(documentGetReq);
            });
            Assert.That(documentDeleteEx.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}

