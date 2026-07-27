using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookInDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;
using BookInDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;
using BookInDto = JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.BookIns.BookIn;

namespace JiwaAPITests.BookIns
{
    public class Document : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task BookInDocument_CRUD()
        {
            // Create a shipment to use for the book in
            LandedCostShipmentPOSTRequest shipmentCreateReq = new LandedCostShipmentPOSTRequest();
            Shipment shipmentCreateRes = await Client.PostAsync(shipmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentCreateRes.ShipmentID, Is.Not.Null);

            // Create a book in from the shipment
            LandedCostBookInCREATEFromShipmentIDRequest bookInCreateReq = new LandedCostBookInCREATEFromShipmentIDRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID
            };

            BookInDto bookInCreateRes = await Client.PostAsync(bookInCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(bookInCreateRes.BookInID, Is.Not.Null);

            // Read document types and use one for the document creation request
            LandedCostBookInDocumentTypesGETManyRequest documentTypesGetManyReq = new LandedCostBookInDocumentTypesGETManyRequest();
            List<BookInDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a document to the book in
            LandedCostBookInDocumentPOSTRequest documentCreateReq = new LandedCostBookInDocumentPOSTRequest()
            {
                BookInID = bookInCreateRes.BookInID,
                Description = "Book In Document " + RandomString(8),
                PhysicalFileName = "BookInDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("Book in document content"),
                DocumentType = new BookInDocumentTypeDto() { DocumentTypeID = documentTypesGetManyRes[0].DocumentTypeID }
            };

            BookInDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all book in documents and ensure the created document is returned
            LandedCostBookInDocumentsGETManyRequest documentsGetManyReq = new LandedCostBookInDocumentsGETManyRequest()
            {
                BookInID = bookInCreateRes.BookInID
            };

            List<BookInDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the created book in document using the DocumentID
            LandedCostBookInDocumentGETRequest documentGetReq = new LandedCostBookInDocumentGETRequest()
            {
                BookInID = bookInCreateRes.BookInID,
                DocumentID = documentCreateRes.DocumentID
            };

            BookInDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Update the book in document
            LandedCostBookInDocumentPATCHRequest documentPatchReq = new LandedCostBookInDocumentPATCHRequest()
            {
                BookInID = bookInCreateRes.BookInID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated Book In Document " + RandomString(6)
            };

            BookInDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Read the updated book in document and confirm the description was changed
            documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Remove the created book in document
            LandedCostBookInDocumentDELETERequest documentDeleteReq = new LandedCostBookInDocumentDELETERequest()
            {
                BookInID = bookInCreateRes.BookInID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted book in document is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                BookInDocumentDto getDeletedRes = await Client.GetAsync(documentGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all book in documents and ensure the deleted document is no longer returned
            documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.False);
        }
        #endregion
    }
}

