using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookInNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.Note;
using BookInNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;
using BookInDto = JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.BookIns.BookIn;

namespace JiwaAPITests.BookIns
{
    public class Note : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task BookInNote_CRUD()
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

            // Read note types and use one for the note creation request
            LandedCostBookInNoteTypesGETManyRequest noteTypesGetManyReq = new LandedCostBookInNoteTypesGETManyRequest();
            List<BookInNoteTypeDto> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a note to the book in
            LandedCostBookInNotePOSTRequest noteCreateReq = new LandedCostBookInNotePOSTRequest()
            {
                BookInID = bookInCreateRes.BookInID,
                NoteText = "Book In Note " + RandomString(12),
                NoteType = new BookInNoteTypeDto() { NoteTypeID = noteTypesGetManyRes[0].NoteTypeID }
            };

            BookInNoteDto noteCreateRes = await Client.PostAsync(noteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteCreateRes.NoteID, Is.Not.Null);
            Assert.That(noteCreateRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Read all book in notes and ensure the created note is returned
            LandedCostBookInNotesGETManyRequest notesGetManyReq = new LandedCostBookInNotesGETManyRequest()
            {
                BookInID = bookInCreateRes.BookInID
            };

            List<BookInNoteDto> notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.True);

            // Read the created book in note using the NoteID
            LandedCostBookInNoteGETRequest noteGetReq = new LandedCostBookInNoteGETRequest()
            {
                BookInID = bookInCreateRes.BookInID,
                NoteID = noteCreateRes.NoteID
            };

            BookInNoteDto noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Update the book in note
            LandedCostBookInNotePATCHRequest notePatchReq = new LandedCostBookInNotePATCHRequest()
            {
                BookInID = bookInCreateRes.BookInID,
                NoteID = noteCreateRes.NoteID,
                NoteText = "Updated Book In Note " + RandomString(8)
            };

            BookInNoteDto notePatchRes = await Client.PatchAsync(notePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(notePatchRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Read the updated book in note and confirm the text was changed
            noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Remove the created book in note
            LandedCostBookInNoteDELETERequest noteDeleteReq = new LandedCostBookInNoteDELETERequest()
            {
                BookInID = bookInCreateRes.BookInID,
                NoteID = noteCreateRes.NoteID
            };

            await Client.DeleteAsync(noteDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted book in note is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                BookInNoteDto getDeletedRes = await Client.GetAsync(noteGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all book in notes and ensure the deleted note is no longer returned
            notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.False);
        }
        #endregion
    }
}

