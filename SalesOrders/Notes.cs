using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder;
using SalesOrderNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.Note;
using SalesOrderNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;

namespace JiwaAPITests.SalesOrders
{
    public class Notes : JiwaAPITest
    {
        private async Task<SalesOrderDto> CreateSalesOrderWithLineAsync()
        {
            // Create an inventory item
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Notes Test Item",
                DefaultPrice = 12.00M
            };
            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a debtor
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Notes Test Debtor"
            };
            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a sales order with one line
            SalesOrderPOSTRequest soCreateReq = new SalesOrderPOSTRequest()
            {
                DebtorAccountNo = debtorCreateReq.AccountNo,
                InvoiceInitDate = DateTime.Today,
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        QuantityOrdered = 1
                    }
                }
            };
            SalesOrderDto soCreateRes = await Client.PostAsync(soCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(soCreateRes.InvoiceID, Is.Not.Null);

            return soCreateRes;
        }

        #region "{Notes}"
        [Test]
        public async Task SalesOrder_Notes_CRUD()
        {
            // Create a note type for the sales order note
            SalesOrderNoteTypePOSTRequest noteTypeCreateReq = new SalesOrderNoteTypePOSTRequest()
            {
                Description = "Sales Order Note Type " + RandomString(8),
                DefaultType = false
            };
            SalesOrderNoteTypeDto noteTypeCreateRes = await Client.PostAsync(noteTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteTypeCreateRes.NoteTypeID, Is.Not.Null);

            // Create a sales order to append a note to
            SalesOrderDto salesOrderCreateRes = await CreateSalesOrderWithLineAsync();

            // Append a note to the sales order
            SalesOrderNotePOSTRequest noteCreateReq = new SalesOrderNotePOSTRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                NoteText = "Sales Order Note " + RandomString(12),
                NoteType = new SalesOrderNoteTypeDto() { NoteTypeID = noteTypeCreateRes.NoteTypeID }
            };
            SalesOrderNoteDto noteCreateRes = await Client.PostAsync(noteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteCreateRes.NoteID, Is.Not.Null);
            Assert.That(noteCreateRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Read all sales order notes and ensure the created note is returned
            SalesOrderNotesGETManyRequest notesGetManyReq = new SalesOrderNotesGETManyRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID
            };
            List<SalesOrderNoteDto> notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.True);

            // Read the created sales order note
            SalesOrderNoteGETRequest noteGetReq = new SalesOrderNoteGETRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                NoteID = noteCreateRes.NoteID
            };
            SalesOrderNoteDto noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Update the sales order note
            SalesOrderNotePATCHRequest notePatchReq = new SalesOrderNotePATCHRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                NoteID = noteCreateRes.NoteID,
                NoteText = "Updated Sales Order Note " + RandomString(8)
            };
            SalesOrderNoteDto notePatchRes = await Client.PatchAsync(notePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(notePatchReq.NoteID));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(notePatchRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Read the updated sales order note
            noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Delete the sales order note
            SalesOrderNoteDELETERequest noteDeleteReq = new SalesOrderNoteDELETERequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID,
                NoteID = noteCreateRes.NoteID
            };
            await Client.DeleteAsync(noteDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the sales order note was deleted
            WebServiceException noteDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(noteGetReq);
            });
            Assert.That(noteDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all notes and ensure the deleted note is no longer returned
            notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.False);

            // Clean up the note type used for the test
            SalesOrderNoteTypeDELETERequest noteTypeDeleteReq = new SalesOrderNoteTypeDELETERequest()
            {
                NoteTypeID = noteTypeCreateRes.NoteTypeID
            };
            await Client.DeleteAsync(noteTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}


