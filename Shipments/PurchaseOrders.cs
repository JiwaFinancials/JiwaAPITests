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
using ShipmentDto = JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments.Shipment;
using ShipmentPurchaseOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments.ShipmentPurchaseOrderReceived;

namespace JiwaAPITests.Shipments
{
    public class PurchaseOrders : JiwaAPITest
    {
        #region "{PurchaseOrders}"
        [Test]
        public async Task ShipmentPurchaseOrders_CRUD()
        {
            // Create an inventory item for purchase order lines.
            InventoryPOSTRequest inventoryCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Shipment Purchase Order Test Item",
                DefaultPrice = 10.00M
            };

            InventoryItem inventoryCreateRes = await Client.PostAsync(inventoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inventoryCreateRes.InventoryID, Is.Not.Null);

            // Create a creditor for purchase order creation.
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Shipment Purchase Order Test Creditor"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);

            // Create a purchase order to append to a shipment.
            PurchaseOrderPOSTRequest purchaseOrderCreateReq = new PurchaseOrderPOSTRequest()
            {
                CreditorAccountNo = creditorCreateRes.AccountNo,
                Reference = "PO-" + RandomString(8),
                OrderDate = DateTime.Today,
                Lines = new List<PurchaseOrderLine>()
                {
                    new PurchaseOrderLine()
                    {
                        PartNo = inventoryCreateRes.PartNo,
                        Quantity = 2M
                    }
                }
            };

            PurchaseOrderDto purchaseOrderCreateRes = await Client.PostAsync(purchaseOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseOrderCreateRes.PurchaseOrderID, Is.Not.Null);

            // Activate the purchase order so it can be received on a shipment.
            PurchaseOrderACTIVATERequest purchaseOrderActivateReq = new PurchaseOrderACTIVATERequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID
            };

            _ = await Client.PostAsync(purchaseOrderActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a shipment to append purchase orders to.
            LandedCostShipmentPOSTRequest shipmentCreateReq = new LandedCostShipmentPOSTRequest()
            {
                DeliveryNotes = "Shipment " + RandomString(8)
            };

            ShipmentDto shipmentCreateRes = await Client.PostAsync(shipmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentCreateRes.ShipmentID, Is.Not.Null);

            // Append a purchase order to the shipment.
            LandedCostShipmentPurchaseOrderPOSTRequest shipmentPurchaseOrderCreateReq = new LandedCostShipmentPurchaseOrderPOSTRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID,
                OrderID = purchaseOrderCreateRes.PurchaseOrderID
            };

            ShipmentPurchaseOrderDto shipmentPurchaseOrderCreateRes = await Client.PostAsync(shipmentPurchaseOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentPurchaseOrderCreateRes.OrderID, Is.EqualTo(purchaseOrderCreateRes.PurchaseOrderID));

            // Read all purchase orders for the shipment.
            LandedCostShipmentPurchaseOrdersGETManyRequest shipmentPurchaseOrdersGetManyReq = new LandedCostShipmentPurchaseOrdersGETManyRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID
            };

            List<ShipmentPurchaseOrderDto> shipmentPurchaseOrdersGetManyRes = await Client.GetAsync(shipmentPurchaseOrdersGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(shipmentPurchaseOrdersGetManyRes.Any(x => x.OrderID == purchaseOrderCreateRes.PurchaseOrderID), Is.True);

            // Read the appended shipment purchase order.
            LandedCostShipmentPurchaseOrderGETRequest shipmentPurchaseOrderGetReq = new LandedCostShipmentPurchaseOrderGETRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID,
                OrderID = purchaseOrderCreateRes.PurchaseOrderID
            };

            ShipmentPurchaseOrderDto shipmentPurchaseOrderGetRes = await Client.GetAsync(shipmentPurchaseOrderGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(shipmentPurchaseOrderGetRes.OrderID, Is.EqualTo(purchaseOrderCreateRes.PurchaseOrderID));

            // Delete the appended purchase order from the shipment.
            LandedCostShipmentPurchaseOrderDELETERequest shipmentPurchaseOrderDeleteReq = new LandedCostShipmentPurchaseOrderDELETERequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID,
                OrderID = purchaseOrderCreateRes.PurchaseOrderID
            };

            await Client.DeleteAsync(shipmentPurchaseOrderDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the shipment purchase order was deleted.
            WebServiceException shipmentPurchaseOrderDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(shipmentPurchaseOrderGetReq);
            });
            Assert.That(shipmentPurchaseOrderDeleteEx.StatusCode, Is.EqualTo(404));

            shipmentPurchaseOrdersGetManyRes = await Client.GetAsync(shipmentPurchaseOrdersGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(shipmentPurchaseOrdersGetManyRes.Any(x => x.OrderID == purchaseOrderCreateRes.PurchaseOrderID), Is.False);
        }
        #endregion
    }
}
