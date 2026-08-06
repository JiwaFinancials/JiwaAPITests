using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SalesQuoteDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;
using SalesQuoteDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;
using SalesQuoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuote;

namespace JiwaAPITests.SalesQuotes
{
    public class Document : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task SalesQuoteDocument_CRUD()
        {
            // Create a sales quote document type
            SalesQuoteDocumentTypePOSTRequest documentTypeCreateReq = new SalesQuoteDocumentTypePOSTRequest()
            {
                Description = "Sales Quote Document Type " + RandomString(8),
                DefaultType = false
            };

            SalesQuoteDocumentTypeDto documentTypeCreateRes = await Client.PostAsync(documentTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentTypeCreateRes.DocumentTypeID, Is.Not.Null);

            // Create an item for the sales quote
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Sales Quote Document Item",
                DefaultPrice = 125.67M
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.InventoryID, Is.Not.Null);

            // Create a debtor for the sales quote
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Sales Quote Document Debtor"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, Is.Not.Null);

            // Create a sales quote to append a document to
            SalesQuotePOSTRequest salesQuoteCreateReq = new SalesQuotePOSTRequest()
            {
                DebtorAccountNo = debtorCreateReq.AccountNo,
                InvoiceInitDate = DateTime.Today.Date,
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuoteLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuoteLine()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        QuantityOrdered = 1
                    }
                }
            };

            SalesQuoteDto salesQuoteCreateRes = await Client.PostAsync(salesQuoteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(salesQuoteCreateRes.QuoteID, Is.Not.Null);

            // Append a document to the sales quote
            SalesQuoteDocumentPOSTRequest documentCreateReq = new SalesQuoteDocumentPOSTRequest()
            {
                QuoteID = salesQuoteCreateRes.QuoteID,
                Description = "Sales Quote Document " + RandomString(8),
                PhysicalFileName = "SalesQuoteDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("Sales quote document content"),
                DocumentType = new SalesQuoteDocumentTypeDto() { DocumentTypeID = documentTypeCreateRes.DocumentTypeID }
            };

            SalesQuoteDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all sales quote documents and ensure the created document is returned
            SalesQuoteDocumentsGETManyRequest documentsGetManyReq = new SalesQuoteDocumentsGETManyRequest()
            {
                QuoteID = salesQuoteCreateRes.QuoteID
            };

            List<SalesQuoteDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the created sales quote document using the DocumentID
            SalesQuoteDocumentGETRequest documentGetReq = new SalesQuoteDocumentGETRequest()
            {
                QuoteID = salesQuoteCreateRes.QuoteID,
                DocumentID = documentCreateRes.DocumentID
            };

            SalesQuoteDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Update the sales quote document
            SalesQuoteDocumentPATCHRequest documentPatchReq = new SalesQuoteDocumentPATCHRequest()
            {
                QuoteID = salesQuoteCreateRes.QuoteID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated Sales Quote Document " + RandomString(6)
            };

            SalesQuoteDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentPatchReq.DocumentID));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Read the updated sales quote document and confirm the description was changed
            documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Remove the created sales quote document
            SalesQuoteDocumentDELETERequest documentDeleteReq = new SalesQuoteDocumentDELETERequest()
            {
                QuoteID = salesQuoteCreateRes.QuoteID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted sales quote document is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                SalesQuoteDocumentDto deletedDocumentGetRes = await Client.GetAsync(documentGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all sales quote documents and ensure the deleted document is no longer returned
            documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.False);
        }
        #endregion
    }
}


