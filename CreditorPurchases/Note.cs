using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.CRBatchTX;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using ServiceStack;
using CreditorPurchaseNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.Note;
using CreditorPurchaseNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.CreditorPurchases
{
    public class Note : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task CreditorPurchaseNote_CRUD()
        {
            // Create a creditor to use for a creditor purchase
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Creditor Test"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);
            Assert.That(creditorCreateRes.AccountNo, Is.EqualTo(creditorCreateReq.AccountNo));

            // Create a creditor purchase to append a note to
            CreditorPurchasePOSTRequest purchaseCreateReq = new CreditorPurchasePOSTRequest()
            {
                Description = "Creditor Purchase Note Test",
                BatchDate = DateTime.Today,
                TransLines = new List<CRBatchTranLine>()
                {
                    new CRBatchTranLine()
                    {
                        RemitNo = RandomString(8),
                        CreditorAccountNo = creditorCreateReq.AccountNo,
                        HomeTransAmount = 123.45M,
                        SupplierTransAmount = 123.45M,
                        ReceiptDate = DateTime.Today,
                        DueDate = DateTime.Today.AddDays(30)
                    }
                }
            };

            CreditorBatchTrans purchaseCreateRes = await Client.PostAsync(purchaseCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseCreateRes.BatchID, Is.Not.Null);

            // Read note types and use one for the note creation request
            CreditorPurchaseNoteTypesGETManyRequest noteTypesGetManyReq = new CreditorPurchaseNoteTypesGETManyRequest();
            List<CreditorPurchaseNoteTypeDto> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Count, Is.GreaterThan(0));

            // Append a note to the creditor purchase
            CreditorPurchaseNotePOSTRequest noteCreateReq = new CreditorPurchaseNotePOSTRequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID,
                NoteText = "Creditor Purchase Note " + RandomString(12),
                NoteType = new CreditorPurchaseNoteTypeDto() { NoteTypeID = noteTypesGetManyRes[0].NoteTypeID }
            };

            CreditorPurchaseNoteDto noteCreateRes = await Client.PostAsync(noteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteCreateRes.NoteID, Is.Not.Null);
            Assert.That(noteCreateRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Read all creditor purchase notes and ensure the created note is returned
            CreditorPurchaseNotesGETManyRequest notesGetManyReq = new CreditorPurchaseNotesGETManyRequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID
            };

            List<CreditorPurchaseNoteDto> notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.True);

            // Read the created creditor purchase note using the NoteID
            CreditorPurchaseNoteGETRequest noteGetReq = new CreditorPurchaseNoteGETRequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID,
                NoteID = noteCreateRes.NoteID
            };

            CreditorPurchaseNoteDto noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Update the creditor purchase note
            CreditorPurchaseNotePATCHRequest notePatchReq = new CreditorPurchaseNotePATCHRequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID,
                NoteID = noteCreateRes.NoteID,
                NoteText = "Updated Creditor Purchase Note " + RandomString(8)
            };

            CreditorPurchaseNoteDto notePatchRes = await Client.PatchAsync(notePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(notePatchReq.NoteID));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(notePatchRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Read the updated creditor purchase note using the NoteID
            noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Remove the created creditor purchase note
            CreditorPurchaseNoteDELETERequest noteDeleteReq = new CreditorPurchaseNoteDELETERequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID,
                NoteID = noteCreateRes.NoteID
            };

            await Client.DeleteAsync(noteDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted creditor purchase note is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                CreditorPurchaseNoteDto getDeletedRes = await Client.GetAsync(noteGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all creditor purchase notes and ensure the deleted note is no longer returned
            notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.False);

            // Remove the created creditor purchase
            CreditorPurchaseDELETERequest purchaseDeleteReq = new CreditorPurchaseDELETERequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID
            };

            await Client.DeleteAsync(purchaseDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}


