using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShipmentDto = JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments.Shipment;
using ShipmentNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.Note;
using ShipmentNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;

namespace JiwaAPITests.Shipments
{
    public class Notes : JiwaAPITest
    {
        #region "{Notes}"
        [Test]
        public async Task ShipmentNotes_CRUD()
        {
            // Create a shipment.
            LandedCostShipmentPOSTRequest shipmentCreateReq = new LandedCostShipmentPOSTRequest()
            {
                DeliveryNotes = "Shipment " + RandomString(8)
            };

            ShipmentDto shipmentCreateRes = await Client.PostAsync(shipmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentCreateRes.ShipmentID, Is.Not.Null);

            // Read shipment note types and use one for note creation.
            ShipmentNoteTypesGETManyRequest noteTypesGetManyReq = new ShipmentNoteTypesGETManyRequest();
            List<ShipmentNoteTypeDto> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a note to the shipment.
            ShipmentNotePOSTRequest noteCreateReq = new ShipmentNotePOSTRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID,
                NoteText = "Shipment Note " + RandomString(12),
                NoteType = new ShipmentNoteTypeDto() { NoteTypeID = noteTypesGetManyRes[0].NoteTypeID }
            };

            ShipmentNoteDto noteCreateRes = await Client.PostAsync(noteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteCreateRes.NoteID, Is.Not.Null);
            Assert.That(noteCreateRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Read all notes for the shipment.
            ShipmentNotesGETManyRequest notesGetManyReq = new ShipmentNotesGETManyRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID
            };

            List<ShipmentNoteDto> notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.True);

            // Read the created shipment note.
            ShipmentNoteGETRequest noteGetReq = new ShipmentNoteGETRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID,
                NoteID = noteCreateRes.NoteID
            };

            ShipmentNoteDto noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Update the shipment note.
            ShipmentNotePATCHRequest notePatchReq = new ShipmentNotePATCHRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID,
                NoteID = noteCreateRes.NoteID,
                NoteText = "Updated Shipment Note " + RandomString(8)
            };

            ShipmentNoteDto notePatchRes = await Client.PatchAsync(notePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(notePatchRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Read the updated shipment note.
            noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Delete the shipment note.
            ShipmentNoteDELETERequest noteDeleteReq = new ShipmentNoteDELETERequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID,
                NoteID = noteCreateRes.NoteID
            };

            await Client.DeleteAsync(noteDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the shipment note was deleted.
            WebServiceException noteDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(noteGetReq);
            });
            Assert.That(noteDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all notes and ensure the deleted note is no longer returned.
            notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.False);

            // Delete the shipment.
            LandedCostShipmentDELETERequest shipmentDeleteReq = new LandedCostShipmentDELETERequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID
            };

            await Client.DeleteAsync(shipmentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}
