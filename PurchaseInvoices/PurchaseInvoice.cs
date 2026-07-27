using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseInvoiceDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseInvoices.PurchaseInvoice;
using PurchaseInvoiceGoodsReceivedNoteInvoicedDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseInvoices.PurchaseInvoiceGoodsReceivedNoteInvoiced;

namespace JiwaAPITests.PurchaseInvoices
{
    public class PurchaseInvoice : PurchaseInvoiceTestBase
    {
        #region "PurchaseInvoices_Core"
        [Test]
        public async Task PurchaseInvoices_CRUD()
        {
            // Create dependencies used by the purchase invoice.
            (var creditor, _, _, var goodsReceivedNote) = await CreateGoodsReceivedNoteWithDependenciesAsync();

            // Create a purchase invoice.
            PurchaseInvoicePOSTRequest purchaseInvoiceCreateReq = new PurchaseInvoicePOSTRequest()
            {
                CreditorID = creditor.CreditorID,
                CreditorAccountNo = creditor.AccountNo,
                InvoiceNo = "PINV-" + RandomString(8),
                InvoiceDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(14),
                GoodsReceivedNotes = new List<PurchaseInvoiceGoodsReceivedNoteInvoicedDto>()
                {
                    new PurchaseInvoiceGoodsReceivedNoteInvoicedDto()
                    {
                        GRNID = goodsReceivedNote.GRNID
                    }
                }
            };

            PurchaseInvoiceDto purchaseInvoiceCreateRes = await Client.PostAsync(purchaseInvoiceCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseInvoiceCreateRes.PurchaseInvoiceID, Is.Not.Null);
            Assert.That(purchaseInvoiceCreateRes.CreditorID, Is.EqualTo(purchaseInvoiceCreateReq.CreditorID));

            // Read the created purchase invoice.
            PurchaseInvoiceGETRequest purchaseInvoiceGetReq = new PurchaseInvoiceGETRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID
            };

            PurchaseInvoiceDto purchaseInvoiceGetRes = await Client.GetAsync(purchaseInvoiceGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchaseInvoiceGetRes.PurchaseInvoiceID, Is.EqualTo(purchaseInvoiceCreateRes.PurchaseInvoiceID));
            Assert.That(purchaseInvoiceGetRes.InvoiceNo, Is.EqualTo(purchaseInvoiceCreateReq.InvoiceNo));

            // Update the purchase invoice.
            PurchaseInvoicePATCHRequest purchaseInvoicePatchReq = new PurchaseInvoicePATCHRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID,
                DueDate = DateTime.Today.AddDays(21)
            };

            PurchaseInvoiceDto purchaseInvoicePatchRes = await Client.PatchAsync(purchaseInvoicePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchaseInvoicePatchRes.PurchaseInvoiceID, Is.EqualTo(purchaseInvoiceCreateRes.PurchaseInvoiceID));
            Assert.That(purchaseInvoicePatchRes.DueDate, Is.EqualTo(purchaseInvoicePatchReq.DueDate));

            // Read the updated purchase invoice.
            purchaseInvoiceGetRes = await Client.GetAsync(purchaseInvoiceGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchaseInvoiceGetRes.DueDate, Is.EqualTo(purchaseInvoicePatchReq.DueDate));

            // Delete the purchase invoice.
            PurchaseInvoiceDELETERequest purchaseInvoiceDeleteReq = new PurchaseInvoiceDELETERequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID
            };

            await Client.DeleteAsync(purchaseInvoiceDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the purchase invoice was deleted.
            WebServiceException purchaseInvoiceDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(purchaseInvoiceGetReq);
            });
            Assert.That(purchaseInvoiceDeleteEx.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "PurchaseInvoices_Activate"
        [Test]
        public async Task PurchaseInvoices_Activate()
        {
            // Create dependencies used to create the purchase invoice.
            (_, _, _, var goodsReceivedNote) = await CreateGoodsReceivedNoteWithDependenciesAsync();

            // Create a purchase invoice from the goods received note.
            PurchaseInvoiceDto purchaseInvoiceCreateRes = await CreatePurchaseInvoiceFromGoodsReceivedNoteAsync(goodsReceivedNote);

            // Activate the purchase invoice.
            PurchaseInvoiceACTIVATERequest purchaseInvoiceActivateReq = new PurchaseInvoiceACTIVATERequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID
            };

            PurchaseInvoiceDto purchaseInvoiceActivateRes = await Client.PostAsync(purchaseInvoiceActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            if (purchaseInvoiceActivateRes?.Status != null)
            {
                Assert.That(purchaseInvoiceActivateRes.Status.ToString(), Is.EqualTo("Activated"));
            }
        }
        #endregion

        #region "PurchaseInvoices_FromGoodsReceivedNotes"
        [Test]
        public async Task PurchaseInvoices_FromGoodsReceivedNotes_CreatesPurchaseInvoice()
        {
            // Create dependencies used by the from-goods-received-notes endpoint.
            (_, _, _, var goodsReceivedNote) = await CreateGoodsReceivedNoteWithDependenciesAsync();

            // Create a purchase invoice from the supplied goods received note number.
            PurchaseInvoiceCREATEFromGRNRequest purchaseInvoiceCreateFromGrnReq = new PurchaseInvoiceCREATEFromGRNRequest()
            {
                GRNNos = new[] { goodsReceivedNote.SlipNo }
            };

            PurchaseInvoiceDto purchaseInvoiceCreateFromGrnRes = await Client.PostAsync(purchaseInvoiceCreateFromGrnReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseInvoiceCreateFromGrnRes.PurchaseInvoiceID, Is.Not.Null);
            Assert.That(purchaseInvoiceCreateFromGrnRes.GoodsReceivedNotes.Any(x => x.GRNID == goodsReceivedNote.GRNID), Is.True);
        }
        #endregion
    }
}

