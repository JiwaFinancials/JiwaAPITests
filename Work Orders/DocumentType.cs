using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkOrderDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;

namespace JiwaAPITests.WorkOrders
{
    public class DocumentType : JiwaAPITest
    {
        #region "WorkOrders_DocumentTypes"
        [Test]
        public async Task WorkOrders_DocumentTypes_CRUD()
        {
            // Create a work order document type
            WorkOrderDocumentTypePOSTRequest documentTypeCreateReq = new WorkOrderDocumentTypePOSTRequest()
            {
                Description = RandomString(12),
                DefaultType = false
            };

            WorkOrderDocumentTypeDto documentTypeCreateRes = await Client.PostAsync(documentTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentTypeCreateRes.DocumentTypeID, Is.Not.Null);
            Assert.That(documentTypeCreateRes.Description, Is.EqualTo(documentTypeCreateReq.Description));

            // Read all work order document types
            WorkOrderDocumentTypesGETManyRequest documentTypesGetManyReq = new WorkOrderDocumentTypesGETManyRequest();
            List<WorkOrderDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Any(x => x.DocumentTypeID == documentTypeCreateRes.DocumentTypeID), Is.True);

            // Read the created work order document type
            WorkOrderDocumentTypeGETRequest documentTypeGetReq = new WorkOrderDocumentTypeGETRequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            WorkOrderDocumentTypeDto documentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypeGetRes.DocumentTypeID, Is.EqualTo(documentTypeCreateRes.DocumentTypeID));
            Assert.That(documentTypeGetRes.Description, Is.EqualTo(documentTypeCreateReq.Description));

            // Update the work order document type
            WorkOrderDocumentTypePATCHRequest documentTypePatchReq = new WorkOrderDocumentTypePATCHRequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID,
                Description = "Updated " + RandomString(10)
            };

            WorkOrderDocumentTypeDto documentTypePatchRes = await Client.PatchAsync(documentTypePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypePatchRes.DocumentTypeID, Is.EqualTo(documentTypeCreateRes.DocumentTypeID));
            Assert.That(documentTypePatchRes.Description, Is.EqualTo(documentTypePatchReq.Description));

            // Read the updated work order document type
            documentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypeGetRes.Description, Is.EqualTo(documentTypePatchReq.Description));

            // Delete the work order document type
            WorkOrderDocumentTypeDELETERequest documentTypeDeleteReq = new WorkOrderDocumentTypeDELETERequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            await Client.DeleteAsync(documentTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the work order document type was deleted
            WebServiceException documentTypeDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(documentTypeGetReq);
            });
            Assert.That(documentTypeDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all document types and ensure the deleted type is no longer returned
            documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Any(x => x.DocumentTypeID == documentTypeCreateRes.DocumentTypeID), Is.False);
        }
        #endregion
    }
}

