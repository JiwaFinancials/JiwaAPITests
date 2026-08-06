using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Documents;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceManagerTaskDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;

namespace JiwaAPITests.ServiceManager
{
    public class Documents : ServiceManagerTestBase
    {
        #region "{Main}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_Documents_CRUD()
        {
            // Create a service manager job.
            Job jobCreateRes = await CreateJobAsync();

            // Create a service manager task.
            ServiceManagerTask taskCreateRes = await CreateTaskAsync(jobCreateRes.JobID);

            // Read document types and use one for document creation.
            ServiceManagerDocumentTypesGETManyRequest documentTypesGetManyReq = new ServiceManagerDocumentTypesGETManyRequest();
            List<DocumentType> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a document to the service manager task.
            ServiceManagerTaskDocumentPOSTRequest documentCreateReq = new ServiceManagerTaskDocumentPOSTRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                Description = "Task Document " + RandomString(8),
                PhysicalFileName = "ServiceManagerTaskDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("Service manager task document content"),
                DocumentType = new DocumentType() { DocumentTypeID = documentTypesGetManyRes[0].DocumentTypeID }
            };

            ServiceManagerTaskDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all documents for the service manager task.
            ServiceManagerTaskDocumentsGETManyRequest documentsGetManyReq = new ServiceManagerTaskDocumentsGETManyRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID
            };

            List<ServiceManagerTaskDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the created service manager task document.
            ServiceManagerTaskDocumentGETRequest documentGetReq = new ServiceManagerTaskDocumentGETRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                DocumentID = documentCreateRes.DocumentID
            };

            ServiceManagerTaskDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));

            // Update the created service manager task document.
            ServiceManagerTaskDocumentPATCHRequest documentPatchReq = new ServiceManagerTaskDocumentPATCHRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated Task Document " + RandomString(6)
            };

            ServiceManagerTaskDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentPatchReq.DocumentID));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Verify the service manager task document was updated.
            documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Delete the created service manager task document.
            ServiceManagerTaskDocumentDELETERequest documentDeleteReq = new ServiceManagerTaskDocumentDELETERequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the service manager task document was deleted.
            WebServiceException documentDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(documentGetReq);
            });
            Assert.That(documentDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all documents again and ensure the deleted document is no longer returned.
            documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.False);

            // Clean up the created service manager task.
            await Client.DeleteAsync(new ServiceManagerTasksDELETERequest() { JobID = jobCreateRes.JobID, TaskID = taskCreateRes.TaskID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Clean up the created service manager job.
            await Client.DeleteAsync(new ServiceManagerJobDELETERequest() { JobID = jobCreateRes.JobID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}


