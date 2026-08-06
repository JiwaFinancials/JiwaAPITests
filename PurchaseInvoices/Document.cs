using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PurchaseInvoiceDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;
using PurchaseInvoiceDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;
using PurchaseInvoiceDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseInvoices.PurchaseInvoice;

namespace JiwaAPITests.PurchaseInvoices
{
    public class Document : PurchaseInvoiceTestBase
    {
        #region "PurchaseInvoices_Documents"
        [Test]
        public async Task PurchaseInvoices_Documents_CRUD()
        {
            // Create dependencies used to create a purchase invoice.
            (_, _, _, var goodsReceivedNote) = await CreateGoodsReceivedNoteWithDependenciesAsync();

            // Create a purchase invoice to append documents to.
            PurchaseInvoiceDto purchaseInvoiceCreateRes = await CreatePurchaseInvoiceFromGoodsReceivedNoteAsync(goodsReceivedNote);

            // Read document types and use one for document creation.
            PurchaseInvoiceDocumentTypesGETManyRequest documentTypesGetManyReq = new PurchaseInvoiceDocumentTypesGETManyRequest();
            List<PurchaseInvoiceDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a document to the purchase invoice.
            PurchaseInvoiceDocumentPOSTRequest documentCreateReq = new PurchaseInvoiceDocumentPOSTRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID,
                Description = "Purchase Invoice Document " + RandomString(8),
                PhysicalFileName = "PurchaseInvoiceDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("purchase invoice document content"),
                DocumentType = new PurchaseInvoiceDocumentTypeDto() { DocumentTypeID = documentTypesGetManyRes[0].DocumentTypeID }
            };

            PurchaseInvoiceDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all documents for the purchase invoice.
            PurchaseInvoiceDocumentsGETManyRequest documentsGetManyReq = new PurchaseInvoiceDocumentsGETManyRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID
            };

            List<PurchaseInvoiceDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the created purchase invoice document.
            PurchaseInvoiceDocumentGETRequest documentGetReq = new PurchaseInvoiceDocumentGETRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID,
                DocumentID = documentCreateRes.DocumentID
            };

            PurchaseInvoiceDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Update the purchase invoice document.
            PurchaseInvoiceDocumentPATCHRequest documentPatchReq = new PurchaseInvoiceDocumentPATCHRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated Purchase Invoice Document " + RandomString(6)
            };

            PurchaseInvoiceDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentPatchReq.DocumentID));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Read the updated purchase invoice document.
            documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Delete the purchase invoice document.
            PurchaseInvoiceDocumentDELETERequest documentDeleteReq = new PurchaseInvoiceDocumentDELETERequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the purchase invoice document was deleted.
            WebServiceException documentDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(documentGetReq);
            });
            Assert.That(documentDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all documents and ensure the deleted document is no longer returned.
            documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.False);
        }
        #endregion
    }
}


