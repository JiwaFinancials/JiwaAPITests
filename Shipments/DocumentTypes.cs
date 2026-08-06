using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Documents;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiwaAPITests.Shipments
{
    public class DocumentTypes : JiwaAPITest
    {
        #region "{DocumentTypes}"
        [Test]
        public async Task ShipmentDocumentTypes_CRUD()
        {
            // Create a shipment document type.
            ShipmentDocumentTypePOSTRequest documentTypeCreateReq = new ShipmentDocumentTypePOSTRequest()
            {
                Description = "Document Type " + RandomString(8)
            };

            DocumentType documentTypeCreateRes = await Client.PostAsync(documentTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentTypeCreateRes.DocumentTypeID, Is.Not.Null);

            // Read the created document type.
            ShipmentDocumentTypeGETRequest documentTypeGetReq = new ShipmentDocumentTypeGETRequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            DocumentType documentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypeGetRes.DocumentTypeID, Is.EqualTo(documentTypeCreateRes.DocumentTypeID));
            Assert.That(documentTypeGetRes.Description, Is.EqualTo(documentTypeCreateReq.Description));

            // Update the document type.
            ShipmentDocumentTypePATCHRequest documentTypePatchReq = new ShipmentDocumentTypePATCHRequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID,
                Description = "Updated Document Type " + RandomString(8),
                RowHash = documentTypeCreateRes.RowHash
            };

            DocumentType documentTypePatchRes = await Client.PatchAsync(documentTypePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypePatchRes.DocumentTypeID, Is.EqualTo(documentTypePatchReq.DocumentTypeID));
            Assert.That(documentTypePatchRes.DocumentTypeID, Is.EqualTo(documentTypeCreateRes.DocumentTypeID));
            Assert.That(documentTypePatchRes.Description, Is.EqualTo(documentTypePatchReq.Description));

            // Verify the document type was updated.
            documentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypeGetRes.Description, Is.EqualTo(documentTypePatchReq.Description));

            // Read all document types and ensure the created document type is returned.
            ShipmentDocumentTypesGETManyRequest documentTypesGetManyReq = new ShipmentDocumentTypesGETManyRequest();
            List<DocumentType> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Count, Is.GreaterThan(0));
            Assert.That(documentTypesGetManyRes.Exists(x => x.DocumentTypeID == documentTypeCreateRes.DocumentTypeID), Is.True);

            // Delete the document type.
            ShipmentDocumentTypeDELETERequest documentTypeDeleteReq = new ShipmentDocumentTypeDELETERequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            await Client.DeleteAsync(documentTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the document type was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                DocumentType deletedDocumentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all document types and ensure the deleted document type is no longer returned.
            documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Exists(x => x.DocumentTypeID == documentTypeCreateRes.DocumentTypeID), Is.False);
        }
        #endregion
    }
}

