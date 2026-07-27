using JiwaFinancials.Jiwa.JiwaServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesQuoteDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;

namespace JiwaAPITests.SalesQuotes
{
    public class DocumentType : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task SalesQuoteDocumentType_CRUD()
        {
            // Create a sales quote document type
            SalesQuoteDocumentTypePOSTRequest documentTypeCreateReq = new SalesQuoteDocumentTypePOSTRequest()
            {
                Description = "Sales Quote Document Type " + RandomString(8),
                DefaultType = false
            };

            SalesQuoteDocumentTypeDto documentTypeCreateRes = await Client.PostAsync(documentTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentTypeCreateRes.DocumentTypeID, Is.Not.Null);
            Assert.That(documentTypeCreateRes.Description, Is.EqualTo(documentTypeCreateReq.Description));

            // Read all sales quote document types and ensure the created document type is returned
            SalesQuoteDocumentTypesGETManyRequest documentTypesGetManyReq = new SalesQuoteDocumentTypesGETManyRequest();
            List<SalesQuoteDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Any(x => x.DocumentTypeID == documentTypeCreateRes.DocumentTypeID), Is.True);

            // Read the created sales quote document type using the DocumentTypeID
            SalesQuoteDocumentTypeGETRequest documentTypeGetReq = new SalesQuoteDocumentTypeGETRequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            SalesQuoteDocumentTypeDto documentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypeGetRes.DocumentTypeID, Is.EqualTo(documentTypeCreateRes.DocumentTypeID));
            Assert.That(documentTypeGetRes.Description, Is.EqualTo(documentTypeCreateReq.Description));

            // Update the sales quote document type
            SalesQuoteDocumentTypePATCHRequest documentTypePatchReq = new SalesQuoteDocumentTypePATCHRequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID,
                Description = "Updated Sales Quote Document Type " + RandomString(6)
            };

            SalesQuoteDocumentTypeDto documentTypePatchRes = await Client.PatchAsync(documentTypePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypePatchRes.DocumentTypeID, Is.EqualTo(documentTypeCreateRes.DocumentTypeID));
            Assert.That(documentTypePatchRes.Description, Is.EqualTo(documentTypePatchReq.Description));

            // Read the updated sales quote document type and confirm the changes were saved
            documentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypeGetRes.Description, Is.EqualTo(documentTypePatchReq.Description));

            // Remove the created sales quote document type
            SalesQuoteDocumentTypeDELETERequest documentTypeDeleteReq = new SalesQuoteDocumentTypeDELETERequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            await Client.DeleteAsync(documentTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted sales quote document type is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                SalesQuoteDocumentTypeDto deletedDocumentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all sales quote document types and ensure the deleted document type is no longer returned
            documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Any(x => x.DocumentTypeID == documentTypeCreateRes.DocumentTypeID), Is.False);
        }
        #endregion
    }
}

