using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;
using ToDoDto = JiwaFinancials.Jiwa.JiwaServiceModel.ToDos.ToDo;

namespace JiwaAPITests.ToDos
{
    public class Documents : JiwaAPITest
    {
        private async Task<ToDoDto> CreateToDoAsync()
        {
            ToDoPOSTRequest toDoCreateReq = new ToDoPOSTRequest()
            {
                Subject = "To Do " + RandomString(8),
                Body = "To Do for document " + RandomString(10),
                ReminderEnabled = false
            };

            ToDoDto toDoCreateRes = await Client.PostAsync(toDoCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(toDoCreateRes.ToDoID, Is.Not.Null.And.Not.Empty);

            return toDoCreateRes;
        }

        #region "{Documents}"
        [Test]
        public async Task ToDo_Documents_CRUD()
        {
            // Create a to do record.
            ToDoDto toDoCreateRes = await CreateToDoAsync();

            // Append a document to the to do record.
            ToDoDocumentPOSTRequest documentCreateReq = new ToDoDocumentPOSTRequest()
            {
                ToDoID = toDoCreateRes.ToDoID,
                Description = "To Do Document " + RandomString(8),
                PhysicalFileName = "ToDoDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("to do document content")
            };

            ToDoDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null.And.Not.Empty);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all documents for the to do and verify the created document is returned.
            ToDoDocumentsGETManyRequest documentsGetManyReq = new ToDoDocumentsGETManyRequest()
            {
                ToDoID = toDoCreateRes.ToDoID
            };

            List<ToDoDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the created to do document.
            ToDoDocumentGETRequest documentGetReq = new ToDoDocumentGETRequest()
            {
                ToDoID = toDoCreateRes.ToDoID,
                DocumentID = documentCreateRes.DocumentID
            };

            ToDoDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Update the to do document.
            ToDoDocumentPATCHRequest documentPatchReq = new ToDoDocumentPATCHRequest()
            {
                ToDoID = toDoCreateRes.ToDoID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated To Do Document " + RandomString(8)
            };

            ToDoDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentPatchReq.DocumentID));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Verify the to do document was updated.
            documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Delete the to do document.
            ToDoDocumentDELETERequest documentDeleteReq = new ToDoDocumentDELETERequest()
            {
                ToDoID = toDoCreateRes.ToDoID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the to do document was deleted.
            WebServiceException documentDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(documentGetReq);
            });
            Assert.That(documentDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all documents and verify the deleted document is no longer returned.
            documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.False);

            // Delete the to do record.
            await Client.DeleteAsync(new ToDoDELETERequest() { ToDoID = toDoCreateRes.ToDoID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

