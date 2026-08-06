using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.JournalSets;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JournalSetDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;
using JournalSetDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;
using JournalSetDto = JiwaFinancials.Jiwa.JiwaServiceModel.JournalSets.JournalSet;

namespace JiwaAPITests.JournalSets
{
    public class Documents : JournalSetsTestBase
    {
        #region "JournalSets_Documents"
        [Test]
        public async Task JournalSets_Documents_CRUD()
        {
            // Create a journal set to append documents to.
            JournalSetDto journalSetCreateRes = await CreateJournalSetAsync();

            // Read document types and use one for document creation.
            JournalSetDocumentTypesGETManyRequest documentTypesGetManyReq = new JournalSetDocumentTypesGETManyRequest();
            List<JournalSetDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a document to the journal set.
            JournalSetDocumentPOSTRequest documentCreateReq = new JournalSetDocumentPOSTRequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID,
                Description = "Journal Set Document " + RandomString(8),
                PhysicalFileName = "JournalSetDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("Journal set document content"),
                DocumentType = new JournalSetDocumentTypeDto() { DocumentTypeID = documentTypesGetManyRes[0].DocumentTypeID }
            };

            JournalSetDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all documents for the journal set.
            JournalSetDocumentsGETManyRequest documentsGetManyReq = new JournalSetDocumentsGETManyRequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID
            };

            List<JournalSetDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the appended document.
            JournalSetDocumentGETRequest documentGetReq = new JournalSetDocumentGETRequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID,
                DocumentID = documentCreateRes.DocumentID
            };

            JournalSetDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));

            // Update the journal set document.
            JournalSetDocumentPATCHRequest documentPatchReq = new JournalSetDocumentPATCHRequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated Journal Set Document " + RandomString(6)
            };

            JournalSetDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentPatchReq.DocumentID));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Delete the journal set document.
            JournalSetDocumentDELETERequest documentDeleteReq = new JournalSetDocumentDELETERequest()
            {
                JournalSetID = journalSetCreateRes.JournalSetID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the journal set document was deleted.
            WebServiceException documentDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(documentGetReq);
            });
            Assert.That(documentDeleteEx.StatusCode, Is.EqualTo(404));

            documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.False);
        }
        #endregion
    }
}


