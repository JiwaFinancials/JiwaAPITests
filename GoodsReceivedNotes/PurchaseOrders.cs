using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoodsReceivedNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes.GoodsReceivedNote;

namespace JiwaAPITests.GoodsReceivedNotes
{
    public class PurchaseOrders : GoodsReceivedNotesTestBase
    {
        #region "GoodsReceivedNotes_PurchaseOrders"
        [Test]
        public async Task GoodsReceivedNotes_PurchaseOrders_CRUD()
        {
            // Create the required creditor and purchase order dependencies.
            (Creditor creditor, JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrder) = await CreatePurchaseOrderWithLineAsync();

            // Activate the purchase order so downstream receive operations are valid.
            await ActivatePurchaseOrderAsync(purchaseOrder.PurchaseOrderID);

            // Create a goods received note to append purchase orders to.
            GoodsReceivedNoteDto grnCreateRes = await CreateGoodsReceivedNoteAsync(creditor);

            // Append a purchase order to the goods received note.
            GoodsReceivedNotePurchaseOrderPOSTRequest grnPurchaseOrderCreateReq = new GoodsReceivedNotePurchaseOrderPOSTRequest()
            {
                GRNID = grnCreateRes.GRNID,
                OrderID = purchaseOrder.PurchaseOrderID
            };

            GoodsReceivedNotePurchaseOrderReceived grnPurchaseOrderCreateRes = await Client.PostAsync(grnPurchaseOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(grnPurchaseOrderCreateRes.OrderID, Is.EqualTo(grnPurchaseOrderCreateReq.OrderID));

            // Read all purchase orders for the goods received note.
            GoodsReceivedNotePurchaseOrdersGETManyRequest grnPurchaseOrdersGetManyReq = new GoodsReceivedNotePurchaseOrdersGETManyRequest()
            {
                GRNID = grnCreateRes.GRNID
            };

            List<GoodsReceivedNotePurchaseOrderReceived> grnPurchaseOrdersGetManyRes = await Client.GetAsync(grnPurchaseOrdersGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(grnPurchaseOrdersGetManyRes.Any(x => x.OrderID == purchaseOrder.PurchaseOrderID), Is.True);

            // Read the appended purchase order for the goods received note.
            GoodsReceivedNotePurchaseOrderGETRequest grnPurchaseOrderGetReq = new GoodsReceivedNotePurchaseOrderGETRequest()
            {
                GRNID = grnCreateRes.GRNID,
                OrderID = purchaseOrder.PurchaseOrderID
            };

            GoodsReceivedNotePurchaseOrderReceived grnPurchaseOrderGetRes = await Client.GetAsync(grnPurchaseOrderGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(grnPurchaseOrderGetRes.OrderID, Is.EqualTo(purchaseOrder.PurchaseOrderID));

            // Delete the appended purchase order from the goods received note.
            GoodsReceivedNotePurchaseOrderDELETERequest grnPurchaseOrderDeleteReq = new GoodsReceivedNotePurchaseOrderDELETERequest()
            {
                GRNID = grnCreateRes.GRNID,
                OrderID = purchaseOrder.PurchaseOrderID
            };

            await Client.DeleteAsync(grnPurchaseOrderDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the purchase order was removed from the goods received note.
            WebServiceException grnPurchaseOrderDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(grnPurchaseOrderGetReq);
            });
            Assert.That(grnPurchaseOrderDeleteEx.StatusCode, Is.EqualTo(404));

            grnPurchaseOrdersGetManyRes = await Client.GetAsync(grnPurchaseOrdersGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(grnPurchaseOrdersGetManyRes.Any(x => x.OrderID == purchaseOrder.PurchaseOrderID), Is.False);
        }
        #endregion
    }
}

