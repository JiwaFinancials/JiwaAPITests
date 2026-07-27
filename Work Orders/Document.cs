using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Documents;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder;
using WorkOrderDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;
using WorkOrderDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;

namespace JiwaAPITests.WorkOrders
{
    public class Document : WorkOrderTestBase
    {
        #region "WorkOrders_Documents"
        [Test]
        public async Task WorkOrders_Documents_CRUD()
        {
            // Create a work order to append a document to
            WorkOrderDto workOrderCreateRes = await CreateWorkOrderAsync();

            // Read document types and use one for the document creation request
            WorkOrderDocumentTypesGETManyRequest documentTypesGetManyReq = new WorkOrderDocumentTypesGETManyRequest();
            List<WorkOrderDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a document to the work order
            WorkOrderDocumentPOSTRequest documentCreateReq = new WorkOrderDocumentPOSTRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                Description = "Work Order Document " + RandomString(8),
                PhysicalFileName = "WorkOrderDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("Work order document content"),
                DocumentType = new WorkOrderDocumentTypeDto() { DocumentTypeID = documentTypesGetManyRes[0].DocumentTypeID }
            };

            WorkOrderDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all work order documents and ensure the created document is returned
            WorkOrderDocumentsGETManyRequest documentsGetManyReq = new WorkOrderDocumentsGETManyRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID
            };

            List<WorkOrderDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the created work order document
            WorkOrderDocumentGETRequest documentGetReq = new WorkOrderDocumentGETRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                DocumentID = documentCreateRes.DocumentID
            };

            WorkOrderDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Update the work order document
            WorkOrderDocumentPATCHRequest documentPatchReq = new WorkOrderDocumentPATCHRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated Work Order Document " + RandomString(8)
            };

            WorkOrderDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Read the updated work order document
            documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Delete the work order document
            WorkOrderDocumentDELETERequest documentDeleteReq = new WorkOrderDocumentDELETERequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the work order document was deleted
            WebServiceException documentDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(documentGetReq);
            });
            Assert.That(documentDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all documents and ensure the deleted document is no longer returned
            documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.False);
        }
        #endregion
    }
}

