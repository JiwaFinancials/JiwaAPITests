using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoodsReceivedNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes.GoodsReceivedNote;

namespace JiwaAPITests.GoodsReceivedNotes
{
    public class GoodsReceivedNote : GoodsReceivedNotesTestBase
    {
        #region "GoodsReceivedNotes_Core"
        [Test]
        public async Task GoodsReceivedNotes_CRUD()
        {
            // Create the required creditor and purchase order dependencies.
            (Creditor creditor, JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrder) = await CreatePurchaseOrderWithLineAsync();

            // Create a goods received note.
            GoodsReceivedNotePOSTRequest grnCreateReq = new GoodsReceivedNotePOSTRequest()
            {
                CreditorID = creditor.CreditorID,
                CreditorAccountNo = creditor.AccountNo,
                Reference = "GRN-" + RandomString(8),
                SlipDate = DateTime.Today
            };

            GoodsReceivedNoteDto grnCreateRes = await Client.PostAsync(grnCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(grnCreateRes.GRNID, Is.Not.Null);
            Assert.That(grnCreateRes.CreditorAccountNo, Is.EqualTo(grnCreateReq.CreditorAccountNo));

            // Read the created goods received note.
            GoodsReceivedNoteGETRequest grnGetReq = new GoodsReceivedNoteGETRequest()
            {
                GRNID = grnCreateRes.GRNID
            };

            GoodsReceivedNoteDto grnGetRes = await Client.GetAsync(grnGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(grnGetRes.GRNID, Is.EqualTo(grnCreateRes.GRNID));

            // Update the goods received note.
            GoodsReceivedNotePATCHRequest grnPatchReq = new GoodsReceivedNotePATCHRequest()
            {
                GRNID = grnCreateRes.GRNID,
                Reference = "Updated-" + RandomString(8),
                SlipDate = DateTime.Today.AddDays(-1)
            };

            GoodsReceivedNoteDto grnPatchRes = await Client.PatchAsync(grnPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(grnPatchRes.GRNID, Is.EqualTo(grnCreateRes.GRNID));
            Assert.That(grnPatchRes.Reference, Is.EqualTo(grnPatchReq.Reference));

            // Delete the goods received note.
            GoodsReceivedNoteDELETERequest grnDeleteReq = new GoodsReceivedNoteDELETERequest()
            {
                GRNID = grnCreateRes.GRNID
            };

            await Client.DeleteAsync(grnDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the goods received note was deleted.
            WebServiceException grnDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(grnGetReq);
            });
            Assert.That(grnDeleteEx.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "GoodsReceivedNotes_Activate"
        [Test]
        public async Task GoodsReceivedNotes_Activate()
        {
            // Create the required creditor and purchase order dependencies.
            (Creditor creditor, JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrder) = await CreatePurchaseOrderWithLineAsync();

            // Create a goods received note to activate.
            GoodsReceivedNoteDto grnCreateRes = await CreateGoodsReceivedNoteAsync(creditor);

            // Activate the goods received note.
            GoodsReceivedNoteACTIVATERequest grnActivateReq = new GoodsReceivedNoteACTIVATERequest()
            {
                GRNID = grnCreateRes.GRNID
            };

            GoodsReceivedNoteDto grnActivateRes = await Client.PostAsync(grnActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            if (grnActivateRes != null)
            {
                Assert.That(grnActivateRes.Status, Is.EqualTo(GoodsReceivedNoteDto.Statuses.Activated));
            }
        }
        #endregion

        #region "GoodsReceivedNotes_FromPurchaseOrderLines"
        [Test]
        public async Task GoodsReceivedNotes_FromPurchaseOrderLines_CreatesGoodsReceivedNote()
        {
            // Create the required purchase order dependency.
            (_, JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrder) = await CreatePurchaseOrderWithLineAsync();

            // Activate the purchase order before receiving stock from its line.
            await ActivatePurchaseOrderAsync(purchaseOrder.PurchaseOrderID);

            // Create a goods received note from the purchase order line.
            GoodsReceivedNoteCREATEFromPOLinesRequest grnCreateFromPOLinesReq = new GoodsReceivedNoteCREATEFromPOLinesRequest()
            {
                ReceivedDate = DateTime.Today,
                ReceivedPOLineQuantities = new List<ReceivedPOLineQuantity>()
                {
                    new ReceivedPOLineQuantity()
                    {
                        OrderLineID = purchaseOrder.Lines[0].PurchaseOrderLineID,
                        Quantity = 2M
                    }
                }
            };

            GoodsReceivedNoteDto grnCreateFromPOLinesRes = await Client.PostAsync(grnCreateFromPOLinesReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(grnCreateFromPOLinesRes.GRNID, Is.Not.Null);
            Assert.That(grnCreateFromPOLinesRes.Lines.Any(x => x.OrderLineID == purchaseOrder.Lines[0].PurchaseOrderLineID), Is.True);
        }
        #endregion

        #region "GoodsReceivedNotes_FromPurchaseOrders"
        [Test]
        public async Task GoodsReceivedNotes_FromPurchaseOrders_CreatesGoodsReceivedNote()
        {
            // Create the required purchase order dependency.
            (_, JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrder) = await CreatePurchaseOrderWithLineAsync();

            // Activate the purchase order before receiving stock from the purchase order.
            await ActivatePurchaseOrderAsync(purchaseOrder.PurchaseOrderID);

            // Create a goods received note from purchase order number(s).
            GoodsReceivedNoteCREATEFromPORequest grnCreateFromPOReq = new GoodsReceivedNoteCREATEFromPORequest()
            {
                OrderNos = new[] { purchaseOrder.OrderNo }
            };

            GoodsReceivedNoteDto grnCreateFromPORes = await Client.PostAsync(grnCreateFromPOReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(grnCreateFromPORes.GRNID, Is.Not.Null);
            Assert.That(grnCreateFromPORes.PurchaseOrders.Any(x => x.OrderNo == purchaseOrder.OrderNo), Is.True);
        }
        #endregion
    }
}

