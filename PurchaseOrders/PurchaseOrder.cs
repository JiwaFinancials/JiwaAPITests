using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments;
using JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder;

namespace JiwaAPITests.PurchaseOrders
{
    public class PurchaseOrder : PurchaseOrderTestBase
    {
        #region "{Main}"
        [Test]
        public async Task PurchaseOrders_CRUD()
        {
            // Create the required creditor, inventory item and purchase order.
            (Creditor creditor, InventoryItem inventoryItem, PurchaseOrderDto purchaseOrderCreateRes) = await CreatePurchaseOrderWithLineAsync();

            Assert.That(purchaseOrderCreateRes.PurchaseOrderID, Is.Not.Null);
            Assert.That(purchaseOrderCreateRes.CreditorAccountNo, Is.EqualTo(creditor.AccountNo));
            Assert.That(purchaseOrderCreateRes.Lines.Count, Is.EqualTo(1));
            Assert.That(purchaseOrderCreateRes.Lines[0].PartNo, Is.EqualTo(inventoryItem.PartNo));
            Assert.That(purchaseOrderCreateRes.Lines[0].PurchaseOrderLineID, Is.Not.Null);

            // Read the created purchase order.
            PurchaseOrderGETRequest purchaseOrderGetReq = new PurchaseOrderGETRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID
            };

            PurchaseOrderDto purchaseOrderGetRes = await Client.GetAsync(purchaseOrderGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchaseOrderGetRes.PurchaseOrderID, Is.EqualTo(purchaseOrderCreateRes.PurchaseOrderID));
            Assert.That(purchaseOrderGetRes.Reference, Is.EqualTo(purchaseOrderCreateRes.Reference));

            // Update the purchase order.
            PurchaseOrderPATCHRequest purchaseOrderPatchReq = new PurchaseOrderPATCHRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID,
                Reference = "Updated-PO-" + RandomString(8)
            };

            PurchaseOrderDto purchaseOrderPatchRes = await Client.PatchAsync(purchaseOrderPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchaseOrderPatchRes.PurchaseOrderID, Is.EqualTo(purchaseOrderCreateRes.PurchaseOrderID));
            Assert.That(purchaseOrderPatchRes.Reference, Is.EqualTo(purchaseOrderPatchReq.Reference));

            // Read the updated purchase order.
            purchaseOrderGetRes = await Client.GetAsync(purchaseOrderGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchaseOrderGetRes.Reference, Is.EqualTo(purchaseOrderPatchReq.Reference));

            // Delete the purchase order.
            PurchaseOrderDELETERequest purchaseOrderDeleteReq = new PurchaseOrderDELETERequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID
            };

            await Client.DeleteAsync(purchaseOrderDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the purchase order was deleted.
            WebServiceException purchaseOrderDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(purchaseOrderGetReq);
            });
            Assert.That(purchaseOrderDeleteEx.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "PurchaseOrders_Activate"
        [Test]
        public async Task PurchaseOrders_Activate()
        {
            // Create the required creditor, inventory item and purchase order.
            (Creditor creditor, InventoryItem inventoryItem, PurchaseOrderDto purchaseOrderCreateRes) = await CreatePurchaseOrderWithLineAsync();

            // Activate the purchase order.
            PurchaseOrderACTIVATERequest purchaseOrderActivateReq = new PurchaseOrderACTIVATERequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID
            };

            _ = await Client.PostAsync(purchaseOrderActivateReq);

            // Read the activated purchase order.
            PurchaseOrderGETRequest purchaseOrderGetReq = new PurchaseOrderGETRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID
            };

            PurchaseOrderDto purchaseOrderGetRes = await Client.GetAsync(purchaseOrderGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchaseOrderGetRes.OrderStatus, Is.EqualTo(PurchaseOrderStatus.Sent));
        }
        #endregion

        #region "PurchaseOrders_FromPurchaseOrderLines"
        [Test]
        public async Task PurchaseOrders_FromPurchaseOrderLines_CreatesGoodsReceivedNote()
        {
            // Create the required creditor, inventory item and purchase order.
            (Creditor creditor, InventoryItem inventoryItem, PurchaseOrderDto purchaseOrderCreateRes) = await CreatePurchaseOrderWithLineAsync();

            // Activate the purchase order before receiving stock against it.
            await ActivatePurchaseOrderAsync(purchaseOrderCreateRes.PurchaseOrderID);

            // Receive stock from the purchase order line when the order is not on a shipment.
            PurchaseOrderReceiveStockCREATEFromPOLinesRequest receiveStockReq = new PurchaseOrderReceiveStockCREATEFromPOLinesRequest()
            {
                ReceivedDate = DateTime.Today,
                ReceivedPOLineQuantities = new List<ReceivedPOLineQuantity>()
                {
                    new ReceivedPOLineQuantity()
                    {
                        OrderLineID = purchaseOrderCreateRes.Lines[0].PurchaseOrderLineID,
                        Quantity = 2M
                    }
                }
            };

            List<ReceivalDocument> receiveStockRes = await Client.PostAsync(receiveStockReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(receiveStockRes.Count, Is.GreaterThan(0));
            Assert.That(receiveStockRes.Any(x => x.IsGoodsReceivedNote == true && !string.IsNullOrWhiteSpace(x.DocumentID)), Is.True);
            Assert.That(receiveStockRes.Any(x => x.IsLandedCostBookIn == true), Is.False);
        }

        [Test]
        public async Task PurchaseOrders_FromPurchaseOrderLines_CreatesBookInForShipmentPurchaseOrder()
        {
            // Create the required creditor, inventory item and purchase order.
            (Creditor creditor, InventoryItem inventoryItem, PurchaseOrderDto purchaseOrderCreateRes) = await CreatePurchaseOrderWithLineAsync();

            // Activate the purchase order before receiving stock against it.
            await ActivatePurchaseOrderAsync(purchaseOrderCreateRes.PurchaseOrderID);

            // Add the purchase order to a shipment so receiving creates a landed cost book in.
            Shipment shipmentCreateRes = await CreateShipmentWithPurchaseOrderAsync(purchaseOrderCreateRes);

            // Receive stock from the purchase order line when the order is on a shipment.
            PurchaseOrderReceiveStockCREATEFromPOLinesRequest receiveStockReq = new PurchaseOrderReceiveStockCREATEFromPOLinesRequest()
            {
                ReceivedDate = DateTime.Today,
                ReceivedPOLineQuantities = new List<ReceivedPOLineQuantity>()
                {
                    new ReceivedPOLineQuantity()
                    {
                        OrderLineID = purchaseOrderCreateRes.Lines[0].PurchaseOrderLineID,
                        Quantity = 2M
                    }
                }
            };

            List<ReceivalDocument> receiveStockRes = await Client.PostAsync(receiveStockReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(receiveStockRes.Count, Is.GreaterThan(0));
            Assert.That(receiveStockRes.Any(x => x.IsLandedCostBookIn == true && !string.IsNullOrWhiteSpace(x.DocumentID)), Is.True);
        }
        #endregion

        private async Task<Shipment> CreateShipmentWithPurchaseOrderAsync(PurchaseOrderDto purchaseOrder)
        {
            // Create a shipment for the purchase order.
            LandedCostShipmentPOSTRequest shipmentCreateReq = new LandedCostShipmentPOSTRequest();
            Shipment shipmentCreateRes = await Client.PostAsync(shipmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentCreateRes.ShipmentID, Is.Not.Null);

            // Add the purchase order to the shipment.
            LandedCostShipmentPurchaseOrderPOSTRequest shipmentPurchaseOrderCreateReq = new LandedCostShipmentPurchaseOrderPOSTRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID,
                OrderID = purchaseOrder.PurchaseOrderID
            };

            await Client.PostAsync(shipmentPurchaseOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            return shipmentCreateRes;
        }

        private async Task ActivatePurchaseOrderAsync(string purchaseOrderID)
        {
            // Activate the purchase order so downstream receive operations are valid.
            PurchaseOrderACTIVATERequest purchaseOrderActivateReq = new PurchaseOrderACTIVATERequest()
            {
                PurchaseOrderID = purchaseOrderID
            };

            _ = await Client.PostAsync(purchaseOrderActivateReq);
        }
    }
}

