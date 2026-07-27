using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Documents;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Debtors
{
    public class Documents : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task DebtorDocument_CRUD()
        {
            // Create a debtor to associate with the document
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

            // Create a document
            DebtorDocumentPOSTRequest documentCreateReq = new DebtorDocumentPOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                Description = RandomString(10)
            };

            Document documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, !Is.Null);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read the created document using the DocumentID
            DebtorDocumentGETRequest documentGetReq = new DebtorDocumentGETRequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID,
                DocumentID = documentCreateRes.DocumentID 
            };
            Document documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Update the document
            DebtorDocumentPATCHRequest documentPatchReq = new DebtorDocumentPATCHRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DocumentID = documentCreateRes.DocumentID,
                Description = RandomString(10)
            };
            Document documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Delete the document
            DebtorDocumentDELETERequest documentDeleteReq = new DebtorDocumentDELETERequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID,
                DocumentID = documentCreateRes.DocumentID 
            };
            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted document is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                Document getDeletedRes = await Client.GetAsync(documentGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Clean up the test debtor
            DebtorDELETERequest debtorDeleteReq = new DebtorDELETERequest() { DebtorID = debtorCreateRes.DebtorID };
            await Client.DeleteAsync(debtorDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }

        [Test]
        public async Task DebtorDocuments_GetMany()
        {
            // Create a debtor to associate with documents
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a few documents
            List<Document> createdDocuments = new List<Document>();
            for (int i = 0; i < 2; i++)
            {
                DebtorDocumentPOSTRequest documentCreateReq = new DebtorDocumentPOSTRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    Description = RandomString(10)
                };

                Document documentCreateRes = await Client.PostAsync(documentCreateReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                createdDocuments.Add(documentCreateRes);
            }

            // Get the list of documents
            DebtorDocumentsGETManyRequest documentsGetManyReq = new DebtorDocumentsGETManyRequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID 
            };
            List<Document> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Count, Is.GreaterThanOrEqualTo(2));

            // Clean up - delete the documents
            foreach (var document in createdDocuments)
            {
                DebtorDocumentDELETERequest documentDeleteReq = new DebtorDocumentDELETERequest() 
                { 
                    DebtorID = debtorCreateRes.DebtorID,
                    DocumentID = document.DocumentID 
                };
                await Client.DeleteAsync(documentDeleteReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
            }

            // Clean up - delete the debtor
            DebtorDELETERequest debtorDeleteReq = new DebtorDELETERequest() { DebtorID = debtorCreateRes.DebtorID };
            await Client.DeleteAsync(debtorDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

