using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesOrderDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;
using SalesOrderDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;
using SalesOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder;

namespace JiwaAPITests.SalesOrders
{
    public class Document : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task SalesOrderDocument_CRUD()
        {
            // Create a sales order document type
            SalesOrderDocumentTypePOSTRequest documentTypeCreateReq = new SalesOrderDocumentTypePOSTRequest()
            {
                Description = "Sales Order Document Type " + RandomString(8),
                DefaultType = false
            };

            SalesOrderDocumentTypeDto documentTypeCreateRes = await Client.PostAsync(documentTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentTypeCreateRes.DocumentTypeID, Is.Not.Null);

            // Create an item for the sales order
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Sales Order Document Item",
                DefaultPrice = 125.67M
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.InventoryID, Is.Not.Null);

            // Create a debtor for the sales order
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Sales Order Document Debtor"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, Is.Not.Null);

            // Create a sales order to append a document to
            SalesOrderPOSTRequest salesOrderCreateReq = new SalesOrderPOSTRequest()
            {
                DebtorAccountNo = debtorCreateReq.AccountNo,
                InvoiceInitDate = DateTime.Today.Date,
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        QuantityOrdered = 1
                    }
                }
            };

            SalesOrderDto salesOrderCreateRes = await Client.PostAsync(salesOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(salesOrderCreateRes.InvoiceID, Is.Not.Null);

            // Append a document to the sales order
            SalesOrderDocumentPOSTRequest documentCreateReq = new SalesOrderDocumentPOSTRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                Description = "Sales Order Document " + RandomString(8),
                PhysicalFileName = "SalesOrderDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("Sales order document content"),
                DocumentType = new SalesOrderDocumentTypeDto() { DocumentTypeID = documentTypeCreateRes.DocumentTypeID }
            };

            SalesOrderDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all sales order documents and ensure the created document is returned
            SalesOrderDocumentsGETManyRequest documentsGetManyReq = new SalesOrderDocumentsGETManyRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID
            };

            List<SalesOrderDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the created sales order document using the DocumentID
            SalesOrderDocumentGETRequest documentGetReq = new SalesOrderDocumentGETRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                DocumentID = documentCreateRes.DocumentID
            };

            SalesOrderDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Update the sales order document
            SalesOrderDocumentPATCHRequest documentPatchReq = new SalesOrderDocumentPATCHRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated Sales Order Document " + RandomString(6)
            };

            SalesOrderDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentPatchReq.DocumentID));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Read the updated sales order document and confirm the description was changed
            documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Remove the created sales order document
            SalesOrderDocumentDELETERequest documentDeleteReq = new SalesOrderDocumentDELETERequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted sales order document is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                SalesOrderDocumentDto deletedDocumentGetRes = await Client.GetAsync(documentGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all sales order documents and ensure the deleted document is no longer returned
            documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.False);
        }
        #endregion
    }
}


