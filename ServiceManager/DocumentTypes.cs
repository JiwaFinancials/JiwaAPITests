using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceManagerDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;

namespace JiwaAPITests.ServiceManager
{
    public class DocumentTypes : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_DocumentTypes_CRUD()
        {
            // Create a service manager document type.
            ServiceManagerDocumentTypePOSTRequest documentTypeCreateReq = new ServiceManagerDocumentTypePOSTRequest()
            {
                Description = "Document Type " + RandomString(8),
                DefaultType = false
            };

            ServiceManagerDocumentTypeDto documentTypeCreateRes = await Client.PostAsync(documentTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentTypeCreateRes.DocumentTypeID, Is.Not.Null);
            Assert.That(documentTypeCreateRes.Description, Is.EqualTo(documentTypeCreateReq.Description));

            // Read all service manager document types.
            ServiceManagerDocumentTypesGETManyRequest documentTypesGetManyReq = new ServiceManagerDocumentTypesGETManyRequest();
            List<ServiceManagerDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Any(x => x.DocumentTypeID == documentTypeCreateRes.DocumentTypeID), Is.True);

            // Read the created service manager document type.
            ServiceManagerDocumentTypeGETRequest documentTypeGetReq = new ServiceManagerDocumentTypeGETRequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            ServiceManagerDocumentTypeDto documentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypeGetRes.DocumentTypeID, Is.EqualTo(documentTypeCreateRes.DocumentTypeID));
            Assert.That(documentTypeGetRes.Description, Is.EqualTo(documentTypeCreateReq.Description));

            // Update the service manager document type.
            ServiceManagerDocumentTypePATCHRequest documentTypePatchReq = new ServiceManagerDocumentTypePATCHRequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID,
                Description = "Updated Document Type " + RandomString(8),
                DefaultType = false
            };

            ServiceManagerDocumentTypeDto documentTypePatchRes = await Client.PatchAsync(documentTypePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypePatchRes.DocumentTypeID, Is.EqualTo(documentTypeCreateRes.DocumentTypeID));
            Assert.That(documentTypePatchRes.Description, Is.EqualTo(documentTypePatchReq.Description));

            // Verify the service manager document type was updated.
            documentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypeGetRes.Description, Is.EqualTo(documentTypePatchReq.Description));

            // Delete the service manager document type.
            ServiceManagerDocumentTypeDELETERequest documentTypeDeleteReq = new ServiceManagerDocumentTypeDELETERequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            await Client.DeleteAsync(documentTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the service manager document type was deleted.
            WebServiceException documentTypeDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(documentTypeGetReq);
            });
            Assert.That(documentTypeDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all service manager document types and ensure the deleted type is no longer returned.
            documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Any(x => x.DocumentTypeID == documentTypeCreateRes.DocumentTypeID), Is.False);
        }
        #endregion
    }
}

