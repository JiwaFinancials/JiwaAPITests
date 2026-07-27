using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PurchaseOrderDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;
using PurchaseOrderDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;
using PurchaseOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder;

namespace JiwaAPITests.PurchaseOrders
{
    public class Document : PurchaseOrderTestBase
    {
        #region "PurchaseOrders_Documents"
        [Test]
        public async Task PurchaseOrders_Documents_CRUD()
        {
            // Create the required creditor, inventory item and purchase order.
            (var creditor, var inventoryItem, PurchaseOrderDto purchaseOrderCreateRes) = await CreatePurchaseOrderWithLineAsync();

            // Read document types and use one for document creation.
            PurchaseOrderDocumentTypesGETManyRequest documentTypesGetManyReq = new PurchaseOrderDocumentTypesGETManyRequest();
            List<PurchaseOrderDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a document to the purchase order.
            PurchaseOrderDocumentPOSTRequest documentCreateReq = new PurchaseOrderDocumentPOSTRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID,
                Description = "Purchase Order Document " + RandomString(8),
                PhysicalFileName = "PurchaseOrderDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("purchase order document content"),
                DocumentType = new PurchaseOrderDocumentTypeDto() { DocumentTypeID = documentTypesGetManyRes[0].DocumentTypeID }
            };

            PurchaseOrderDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all documents for the purchase order.
            PurchaseOrderDocumentsGETManyRequest documentsGetManyReq = new PurchaseOrderDocumentsGETManyRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID
            };

            List<PurchaseOrderDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the created purchase order document.
            PurchaseOrderDocumentGETRequest documentGetReq = new PurchaseOrderDocumentGETRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID,
                DocumentID = documentCreateRes.DocumentID
            };

            PurchaseOrderDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Update the purchase order document.
            PurchaseOrderDocumentPATCHRequest documentPatchReq = new PurchaseOrderDocumentPATCHRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated Purchase Order Document " + RandomString(6)
            };

            PurchaseOrderDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Read the updated purchase order document.
            documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Delete the purchase order document.
            PurchaseOrderDocumentDELETERequest documentDeleteReq = new PurchaseOrderDocumentDELETERequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the purchase order document was deleted.
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

