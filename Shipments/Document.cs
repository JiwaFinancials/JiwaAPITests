using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShipmentDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;
using ShipmentDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;
using ShipmentDto = JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments.Shipment;

namespace JiwaAPITests.Shipments
{
    public class Document : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task ShipmentDocument_CRUD()
        {
            // Create a shipment to append a document to.
            LandedCostShipmentPOSTRequest shipmentCreateReq = new LandedCostShipmentPOSTRequest();
            ShipmentDto shipmentCreateRes = await Client.PostAsync(shipmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentCreateRes.ShipmentID, Is.Not.Null);

            // Read document types and use one for document creation.
            ShipmentDocumentTypesGETManyRequest documentTypesGetManyReq = new ShipmentDocumentTypesGETManyRequest();
            List<ShipmentDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a document to the shipment.
            ShipmentDocumentPOSTRequest documentCreateReq = new ShipmentDocumentPOSTRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID,
                Description = "Shipment Document " + RandomString(8),
                PhysicalFileName = "ShipmentDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("Shipment document content"),
                DocumentType = new ShipmentDocumentTypeDto() { DocumentTypeID = documentTypesGetManyRes[0].DocumentTypeID }
            };

            ShipmentDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all shipment documents and ensure the created document is returned.
            ShipmentDocumentsGETManyRequest documentsGetManyReq = new ShipmentDocumentsGETManyRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID
            };

            List<ShipmentDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the created shipment document.
            ShipmentDocumentGETRequest documentGetReq = new ShipmentDocumentGETRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID,
                DocumentID = documentCreateRes.DocumentID
            };

            ShipmentDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Update the shipment document.
            ShipmentDocumentPATCHRequest documentPatchReq = new ShipmentDocumentPATCHRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated Shipment Document " + RandomString(6)
            };

            ShipmentDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Verify the shipment document was updated.
            documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Delete the shipment document.
            ShipmentDocumentDELETERequest documentDeleteReq = new ShipmentDocumentDELETERequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the shipment document was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                ShipmentDocumentDto deletedDocumentGetRes = await Client.GetAsync(documentGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all shipment documents and ensure the deleted document is no longer returned.
            documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.False);
        }
        #endregion
    }
}

