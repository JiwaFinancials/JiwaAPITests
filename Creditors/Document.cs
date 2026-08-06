using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using ServiceStack;
using CreditorDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;
using CreditorDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Creditors
{
    public class Document : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task CreditorDocument_CRUD()
        {
            // Create a creditor to append a document to
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Creditor Document Test"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);
            Assert.That(creditorCreateRes.AccountNo, Is.EqualTo(creditorCreateReq.AccountNo));

            // Read document types and use one for the document creation request
            CreditorDocumentTypesGETManyRequest documentTypesGetManyReq = new CreditorDocumentTypesGETManyRequest();
            List<CreditorDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a document to the creditor
            CreditorDocumentPOSTRequest documentCreateReq = new CreditorDocumentPOSTRequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                Description = "Creditor Document " + RandomString(8),
                PhysicalFileName = "CreditorDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("Creditor document content"),
                DocumentType = new CreditorDocumentTypeDto() { DocumentTypeID = documentTypesGetManyRes[0].DocumentTypeID }
            };

            CreditorDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all creditor documents and ensure the created document is returned
            CreditorDocumentsGETManyRequest documentsGetManyReq = new CreditorDocumentsGETManyRequest()
            {
                CreditorID = creditorCreateRes.CreditorID
            };

            List<CreditorDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the created creditor document using the DocumentID
            CreditorDocumentGETRequest documentGetReq = new CreditorDocumentGETRequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                DocumentID = documentCreateRes.DocumentID
            };

            CreditorDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Update the creditor document
            CreditorDocumentPATCHRequest documentPatchReq = new CreditorDocumentPATCHRequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated Creditor Document " + RandomString(6)
            };

            CreditorDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentPatchReq.DocumentID));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Read the updated creditor document using the DocumentID
            documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Remove the created creditor document
            CreditorDocumentDELETERequest documentDeleteReq = new CreditorDocumentDELETERequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted creditor document is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                CreditorDocumentDto getDeletedRes = await Client.GetAsync(documentGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all creditor documents and ensure the deleted document is no longer returned
            documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.False);

            // Remove the created creditor
            CreditorDELETERequest creditorDeleteReq = new CreditorDELETERequest()
            {
                CreditorID = creditorCreateRes.CreditorID
            };

            await Client.DeleteAsync(creditorDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}


