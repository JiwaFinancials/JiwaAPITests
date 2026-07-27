using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.Bills;
using ServiceStack;
using BillNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.Note;
using BillNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Bills
{
    public class Note : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task BillNote_CRUD()
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

            // Read note types and use one for the note creation request
            BillNoteTypesGETManyRequest noteTypesGetManyReq = new BillNoteTypesGETManyRequest();
            List<BillNoteTypeDto> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a note to the bill
            BillNotePOSTRequest noteCreateReq = new BillNotePOSTRequest()
            {
                BillID = billCreateRes.BillID,
                NoteText = "Bill Note " + RandomString(12),
                NoteType = new BillNoteTypeDto() { NoteTypeID = noteTypesGetManyRes[0].NoteTypeID }
            };

            BillNoteDto noteCreateRes = await Client.PostAsync(noteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteCreateRes.NoteID, Is.Not.Null);
            Assert.That(noteCreateRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Read all bill notes and ensure the created note is returned
            BillNotesGETManyRequest notesGetManyReq = new BillNotesGETManyRequest()
            {
                BillID = billCreateRes.BillID
            };

            List<BillNoteDto> notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.True);

            // Read the created bill note using the NoteID
            BillNoteGETRequest noteGetReq = new BillNoteGETRequest()
            {
                BillID = billCreateRes.BillID,
                NoteID = noteCreateRes.NoteID
            };

            BillNoteDto noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Update the bill note
            BillNotePATCHRequest notePatchReq = new BillNotePATCHRequest()
            {
                BillID = billCreateRes.BillID,
                NoteID = noteCreateRes.NoteID,
                NoteText = "Updated Bill Note " + RandomString(8)
            };

            BillNoteDto notePatchRes = await Client.PatchAsync(notePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(notePatchRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Read the updated bill note using the NoteID
            noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Remove the created bill note
            BillNoteDELETERequest noteDeleteReq = new BillNoteDELETERequest()
            {
                BillID = billCreateRes.BillID,
                NoteID = noteCreateRes.NoteID
            };

            await Client.DeleteAsync(noteDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted bill note is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                BillNoteDto getDeletedRes = await Client.GetAsync(noteGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all bill notes and ensure the deleted note is no longer returned
            notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.False);

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

