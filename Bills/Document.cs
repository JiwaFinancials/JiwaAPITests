using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.Bills;
using ServiceStack;
using BillDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;
using BillDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Bills
{
    public class Document : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task BillDocument_CRUD()
        {
            // Create bill items
            InventoryPOSTRequest outputItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Output Item Test",
                DefaultPrice = 99.99M
            };

            InventoryItem outputItemCreateRes = await Client.PostAsync(outputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(outputItemCreateRes.InventoryID, !Is.Null);

            InventoryPOSTRequest inputItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Input Item Test",
                DefaultPrice = 12.75M
            };

            InventoryItem inputItemCreateRes = await Client.PostAsync(inputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inputItemCreateRes.InventoryID, !Is.Null);

            // Create a bill
            BillPOSTRequest billCreateReq = new BillPOSTRequest()
            {
                Stages = new List<JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillStage>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillStage()
                    {
                        Name = "Stage 1",
                        Inputs = new List<JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillInput>()
                        {
                            new JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillInput()
                            {
                                PartNo = inputItemCreateRes.PartNo, Quantity = 1, IsRatio = true
                            }
                        }
                    }
                },
                Outputs = new List<JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillOutput>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillOutput() { PartNo = outputItemCreateRes.PartNo, Quantity = 1, IsRatio = true }
                }
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Bills.Bill billCreateRes = await Client.PostAsync(billCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(billCreateRes.BillID, !Is.Null);

            // Read document types and use one for the document creation request
            BillDocumentTypesGETManyRequest documentTypesGetManyReq = new BillDocumentTypesGETManyRequest();
            List<BillDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a document to the bill
            BillDocumentPOSTRequest documentCreateReq = new BillDocumentPOSTRequest()
            {
                BillID = billCreateRes.BillID,
                Description = "Bill Document " + RandomString(8),
                PhysicalFileName = "BillDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("Bill document content"),
                DocumentType = new BillDocumentTypeDto() { DocumentTypeID = documentTypesGetManyRes[0].DocumentTypeID }
            };

            BillDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all bill documents and ensure the created document is returned
            BillDocumentsGETManyRequest documentsGetManyReq = new BillDocumentsGETManyRequest()
            {
                BillID = billCreateRes.BillID
            };

            List<BillDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the created bill document using the DocumentID
            BillDocumentGETRequest documentGetReq = new BillDocumentGETRequest()
            {
                BillID = billCreateRes.BillID,
                DocumentID = documentCreateRes.DocumentID
            };

            BillDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Update the bill document
            BillDocumentPATCHRequest documentPatchReq = new BillDocumentPATCHRequest()
            {
                BillID = billCreateRes.BillID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated Bill Document " + RandomString(6)
            };

            BillDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Read the updated bill document using the DocumentID
            documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Remove the created bill document
            BillDocumentDELETERequest documentDeleteReq = new BillDocumentDELETERequest()
            {
                BillID = billCreateRes.BillID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted bill document is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                BillDocumentDto getDeletedRes = await Client.GetAsync(documentGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all bill documents and ensure the deleted document is no longer returned
            documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.False);

            // Remove the created bill
            BillDELETERequest billDeleteReq = new BillDELETERequest()
            {
                BillID = billCreateRes.BillID
            };

            await Client.DeleteAsync(billDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

