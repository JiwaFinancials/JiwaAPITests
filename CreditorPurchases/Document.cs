using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.CRBatchTX;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using ServiceStack;
using CreditorPurchaseDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;
using CreditorPurchaseDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.CreditorPurchases
{
    public class Document : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task CreditorPurchaseDocument_CRUD()
        {
            // Create a creditor to use for a creditor purchase
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Creditor Test"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);
            Assert.That(creditorCreateRes.AccountNo, Is.EqualTo(creditorCreateReq.AccountNo));

            // Create a creditor purchase to append a document to
            CreditorPurchasePOSTRequest purchaseCreateReq = new CreditorPurchasePOSTRequest()
            {
                Description = "Creditor Purchase Document Test",
                BatchDate = DateTime.Today,
                TransLines = new List<CRBatchTranLine>()
                {
                    new CRBatchTranLine()
                    {
                        RemitNo = RandomString(8),
                        CreditorAccountNo = creditorCreateReq.AccountNo,
                        HomeTransAmount = 123.45M,
                        SupplierTransAmount = 123.45M,
                        ReceiptDate = DateTime.Today,
                        DueDate = DateTime.Today.AddDays(30)
                    }
                }
            };

            CreditorBatchTrans purchaseCreateRes = await Client.PostAsync(purchaseCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseCreateRes.BatchID, Is.Not.Null);

            // Read document types and use one for the document creation request
            CreditorPurchaseDocumentTypesGETManyRequest documentTypesGetManyReq = new CreditorPurchaseDocumentTypesGETManyRequest();
            List<CreditorPurchaseDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a document to the creditor purchase
            CreditorPurchaseDocumentPOSTRequest documentCreateReq = new CreditorPurchaseDocumentPOSTRequest()
            {
                BatchID = purchaseCreateRes.BatchID,
                Description = "Creditor Purchase Document " + RandomString(8),
                PhysicalFileName = "CreditorPurchaseDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("Creditor purchase document content"),
                DocumentType = new CreditorPurchaseDocumentTypeDto() { DocumentTypeID = documentTypesGetManyRes[0].DocumentTypeID }
            };

            CreditorPurchaseDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all creditor purchase documents and ensure the created document is returned
            CreditorPurchaseDocumentsGETManyRequest documentsGetManyReq = new CreditorPurchaseDocumentsGETManyRequest()
            {
                BatchID = purchaseCreateRes.BatchID
            };

            List<CreditorPurchaseDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the created creditor purchase document using the DocumentID
            CreditorPurchaseDocumentGETRequest documentGetReq = new CreditorPurchaseDocumentGETRequest()
            {
                BatchID = purchaseCreateRes.BatchID,
                DocumentID = documentCreateRes.DocumentID
            };

            CreditorPurchaseDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Update the creditor purchase document
            CreditorPurchaseDocumentPATCHRequest documentPatchReq = new CreditorPurchaseDocumentPATCHRequest()
            {
                BatchID = purchaseCreateRes.BatchID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated Creditor Purchase Document " + RandomString(6)
            };

            CreditorPurchaseDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentPatchReq.DocumentID));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Read the updated creditor purchase document using the DocumentID
            documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Remove the created creditor purchase document
            CreditorPurchaseDocumentDELETERequest documentDeleteReq = new CreditorPurchaseDocumentDELETERequest()
            {
                BatchID = purchaseCreateRes.BatchID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted creditor purchase document is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                CreditorPurchaseDocumentDto getDeletedRes = await Client.GetAsync(documentGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all creditor purchase documents and ensure the deleted document is no longer returned
            documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.False);

            // Remove the created creditor purchase
            CreditorPurchaseDELETERequest purchaseDeleteReq = new CreditorPurchaseDELETERequest()
            {
                BatchID = purchaseCreateRes.BatchID
            };

            await Client.DeleteAsync(purchaseDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}


