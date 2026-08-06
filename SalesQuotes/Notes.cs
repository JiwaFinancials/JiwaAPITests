using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesQuoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuote;
using SalesQuoteNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.Note;
using SalesQuoteNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;

namespace JiwaAPITests.SalesQuotes
{
    public class Notes : JiwaAPITest
    {
        private async Task<SalesQuoteDto> CreateSalesQuoteWithLineAsync()
        {
            // Create an inventory item
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Sales Quote Notes Test Item",
                DefaultPrice = 12.00M
            };
            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a debtor
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Sales Quote Notes Test Debtor"
            };
            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a sales quote with one line
            SalesQuotePOSTRequest quoteCreateReq = new SalesQuotePOSTRequest()
            {
                DebtorAccountNo = debtorCreateReq.AccountNo,
                InvoiceInitDate = DateTime.Today,
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuoteLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesQuotes.SalesQuoteLine()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        QuantityOrdered = 1
                    }
                }
            };
            SalesQuoteDto quoteCreateRes = await Client.PostAsync(quoteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(quoteCreateRes.QuoteID, Is.Not.Null);

            return quoteCreateRes;
        }

        #region "{Notes}"
        [Test]
        public async Task SalesQuote_Notes_CRUD()
        {
            // Create a note type for the sales quote note
            SalesQuoteNoteTypePOSTRequest noteTypeCreateReq = new SalesQuoteNoteTypePOSTRequest()
            {
                Description = "Sales Quote Note Type " + RandomString(8),
                DefaultType = false
            };
            SalesQuoteNoteTypeDto noteTypeCreateRes = await Client.PostAsync(noteTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteTypeCreateRes.NoteTypeID, Is.Not.Null);

            // Create a sales quote to append a note to
            SalesQuoteDto salesQuoteCreateRes = await CreateSalesQuoteWithLineAsync();

            // Append a note to the sales quote
            SalesQuoteNotePOSTRequest noteCreateReq = new SalesQuoteNotePOSTRequest()
            {
                QuoteID = salesQuoteCreateRes.QuoteID,
                NoteText = "Sales Quote Note " + RandomString(12),
                NoteType = new SalesQuoteNoteTypeDto() { NoteTypeID = noteTypeCreateRes.NoteTypeID }
            };
            SalesQuoteNoteDto noteCreateRes = await Client.PostAsync(noteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteCreateRes.NoteID, Is.Not.Null);
            Assert.That(noteCreateRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Read all sales quote notes and ensure the created note is returned
            SalesQuoteNotesGETManyRequest notesGetManyReq = new SalesQuoteNotesGETManyRequest()
            {
                QuoteID = salesQuoteCreateRes.QuoteID
            };
            List<SalesQuoteNoteDto> notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.True);

            // Read the created sales quote note
            SalesQuoteNoteGETRequest noteGetReq = new SalesQuoteNoteGETRequest()
            {
                QuoteID = salesQuoteCreateRes.QuoteID,
                NoteID = noteCreateRes.NoteID
            };
            SalesQuoteNoteDto noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Update the sales quote note
            SalesQuoteNotePATCHRequest notePatchReq = new SalesQuoteNotePATCHRequest()
            {
                QuoteID = salesQuoteCreateRes.QuoteID,
                NoteID = noteCreateRes.NoteID,
                NoteText = "Updated Sales Quote Note " + RandomString(8)
            };
            SalesQuoteNoteDto notePatchRes = await Client.PatchAsync(notePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(notePatchReq.NoteID));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(notePatchRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Read the updated sales quote note
            noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Delete the sales quote note
            SalesQuoteNoteDELETERequest noteDeleteReq = new SalesQuoteNoteDELETERequest()
            {
                QuoteID = salesQuoteCreateRes.QuoteID,
                NoteID = noteCreateRes.NoteID
            };
            await Client.DeleteAsync(noteDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the sales quote note was deleted
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
            SalesQuoteNoteTypeDELETERequest noteTypeDeleteReq = new SalesQuoteNoteTypeDELETERequest()
            {
                NoteTypeID = noteTypeCreateRes.NoteTypeID
            };
            await Client.DeleteAsync(noteTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}


