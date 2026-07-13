using JiwaFinancials.Jiwa.JiwaServiceModel;
using ServiceStack;
using CreditorPurchaseDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.CreditorPurchases
{
    public class DocumentType : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task CreditorPurchaseDocumentType_CRUD()
        {
            // Create a document type
            CreditorPurchaseDocumentTypePOSTRequest documentTypeCreateReq = new CreditorPurchaseDocumentTypePOSTRequest()
            {
                Description = RandomString(12),
                DefaultType = false
            };

            CreditorPurchaseDocumentTypeDto documentTypeCreateRes = await Client.PostAsync(documentTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentTypeCreateRes.DocumentTypeID, Is.Not.Null);
            Assert.That(documentTypeCreateRes.Description, Is.EqualTo(documentTypeCreateReq.Description));

            // Read all document types and ensure the created document type is returned
            CreditorPurchaseDocumentTypesGETManyRequest documentTypesGetManyReq = new CreditorPurchaseDocumentTypesGETManyRequest();
            List<CreditorPurchaseDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Any(x => x.DocumentTypeID == documentTypeCreateRes.DocumentTypeID), Is.True);

            // Read the created document type using the DocumentTypeID
            CreditorPurchaseDocumentTypeGETRequest documentTypeGetReq = new CreditorPurchaseDocumentTypeGETRequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            CreditorPurchaseDocumentTypeDto documentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypeGetRes.DocumentTypeID, Is.EqualTo(documentTypeCreateRes.DocumentTypeID));
            Assert.That(documentTypeGetRes.Description, Is.EqualTo(documentTypeCreateReq.Description));

            // Update the document type
            CreditorPurchaseDocumentTypePATCHRequest documentTypePatchReq = new CreditorPurchaseDocumentTypePATCHRequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID,
                Description = "Updated " + RandomString(10)
            };

            CreditorPurchaseDocumentTypeDto documentTypePatchRes = await Client.PatchAsync(documentTypePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypePatchRes.DocumentTypeID, Is.EqualTo(documentTypeCreateRes.DocumentTypeID));
            Assert.That(documentTypePatchRes.Description, Is.EqualTo(documentTypePatchReq.Description));

            // Read the updated document type using the DocumentTypeID
            documentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypeGetRes.Description, Is.EqualTo(documentTypePatchReq.Description));

            // Remove the created document type
            CreditorPurchaseDocumentTypeDELETERequest documentTypeDeleteReq = new CreditorPurchaseDocumentTypeDELETERequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            await Client.DeleteAsync(documentTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted document type is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                CreditorPurchaseDocumentTypeDto getDeletedRes = await Client.GetAsync(documentTypeGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all document types and ensure the deleted document type is no longer returned
            documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Any(x => x.DocumentTypeID == documentTypeCreateRes.DocumentTypeID), Is.False);
        }
        #endregion
    }
}
