using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseOrderDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;

namespace JiwaAPITests.PurchaseOrders
{
    public class DocumentType : PurchaseOrderTestBase
    {
        #region "PurchaseOrders_DocumentTypes"
        [Test]
        public async Task PurchaseOrders_DocumentTypes_CRUD()
        {
            // Create a purchase order document type.
            PurchaseOrderDocumentTypePOSTRequest documentTypeCreateReq = new PurchaseOrderDocumentTypePOSTRequest()
            {
                Description = RandomString(12),
                DefaultType = false
            };

            PurchaseOrderDocumentTypeDto documentTypeCreateRes = await Client.PostAsync(documentTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentTypeCreateRes.DocumentTypeID, Is.Not.Null);
            Assert.That(documentTypeCreateRes.Description, Is.EqualTo(documentTypeCreateReq.Description));

            // Read all purchase order document types.
            PurchaseOrderDocumentTypesGETManyRequest documentTypesGetManyReq = new PurchaseOrderDocumentTypesGETManyRequest();
            List<PurchaseOrderDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Any(x => x.DocumentTypeID == documentTypeCreateRes.DocumentTypeID), Is.True);

            // Read the created purchase order document type.
            PurchaseOrderDocumentTypeGETRequest documentTypeGetReq = new PurchaseOrderDocumentTypeGETRequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            PurchaseOrderDocumentTypeDto documentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypeGetRes.DocumentTypeID, Is.EqualTo(documentTypeCreateRes.DocumentTypeID));
            Assert.That(documentTypeGetRes.Description, Is.EqualTo(documentTypeCreateReq.Description));

            // Update the purchase order document type.
            PurchaseOrderDocumentTypePATCHRequest documentTypePatchReq = new PurchaseOrderDocumentTypePATCHRequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID,
                Description = "Updated " + RandomString(10)
            };

            PurchaseOrderDocumentTypeDto documentTypePatchRes = await Client.PatchAsync(documentTypePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypePatchRes.DocumentTypeID, Is.EqualTo(documentTypeCreateRes.DocumentTypeID));
            Assert.That(documentTypePatchRes.Description, Is.EqualTo(documentTypePatchReq.Description));

            // Read the updated purchase order document type.
            documentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypeGetRes.Description, Is.EqualTo(documentTypePatchReq.Description));

            // Delete the purchase order document type.
            PurchaseOrderDocumentTypeDELETERequest documentTypeDeleteReq = new PurchaseOrderDocumentTypeDELETERequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            await Client.DeleteAsync(documentTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the purchase order document type was deleted.
            WebServiceException documentTypeDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(documentTypeGetReq);
            });
            Assert.That(documentTypeDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all document types and ensure the deleted type is no longer returned.
            documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Any(x => x.DocumentTypeID == documentTypeCreateRes.DocumentTypeID), Is.False);
        }
        #endregion
    }
}

