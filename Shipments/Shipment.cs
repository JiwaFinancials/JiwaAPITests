using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments;
using JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShipmentDto = JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments.Shipment;

namespace JiwaAPITests.Shipments
{
    public class Shipment : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Shipment_CRUD()
        {
            // Create a shipment.
            LandedCostShipmentPOSTRequest shipmentCreateReq = new LandedCostShipmentPOSTRequest()
            {
                DeliveryNotes = "Shipment " + RandomString(8)
            };

            ShipmentDto shipmentCreateRes = await Client.PostAsync(shipmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentCreateRes.ShipmentID, Is.Not.Null);

            // Read the created shipment.
            LandedCostShipmentGETRequest shipmentGetReq = new LandedCostShipmentGETRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID
            };

            ShipmentDto shipmentGetRes = await Client.GetAsync(shipmentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(shipmentGetRes.ShipmentID, Is.EqualTo(shipmentCreateRes.ShipmentID));

            // Update the shipment.
            LandedCostShipmentPATCHRequest shipmentPatchReq = new LandedCostShipmentPATCHRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID,
                DeliveryNotes = "Updated Shipment " + RandomString(8),
                WayBillNo = "WB" + RandomString(6)
            };

            ShipmentDto shipmentPatchRes = await Client.PatchAsync(shipmentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(shipmentPatchRes.ShipmentID, Is.EqualTo(shipmentCreateRes.ShipmentID));
            Assert.That(shipmentPatchRes.DeliveryNotes, Is.EqualTo(shipmentPatchReq.DeliveryNotes));

            // Verify the shipment was updated.
            shipmentGetRes = await Client.GetAsync(shipmentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(shipmentGetRes.DeliveryNotes, Is.EqualTo(shipmentPatchReq.DeliveryNotes));

            // Delete the shipment.
            LandedCostShipmentDELETERequest shipmentDeleteReq = new LandedCostShipmentDELETERequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID
            };

            await Client.DeleteAsync(shipmentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the shipment was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                ShipmentDto deletedShipmentGetRes = await Client.GetAsync(shipmentGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "{Activate}"
        [Test]
        public async Task Shipment_Activate()
        {
            // Create a shipment to activate.
            LandedCostShipmentPOSTRequest shipmentCreateReq = new LandedCostShipmentPOSTRequest();
            ShipmentDto shipmentCreateRes = await Client.PostAsync(shipmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentCreateRes.ShipmentID, Is.Not.Null);

            // Activate the created shipment.
            LandedCostShipmentACTIVATERequest shipmentActivateReq = new LandedCostShipmentACTIVATERequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID
            };

            await Client.PostAsync(shipmentActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
        }
        #endregion

        #region "{FromPurchaseOrders}"
        [Test]
        public async Task Shipment_CreateFromPurchaseOrders()
        {
            // Create an inventory item for purchase order lines.
            InventoryPOSTRequest inventoryCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Shipment From PO Test Item",
                DefaultPrice = 10.00M
            };

            InventoryItem inventoryCreateRes = await Client.PostAsync(inventoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inventoryCreateRes.InventoryID, Is.Not.Null);

            // Create a creditor for purchase orders.
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Shipment From PO Test Creditor"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);

            // Create the first purchase order.
            PurchaseOrderPOSTRequest purchaseOrderOneCreateReq = new PurchaseOrderPOSTRequest()
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

            JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrderOneCreateRes = await Client.PostAsync(purchaseOrderOneCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseOrderOneCreateRes.PurchaseOrderID, Is.Not.Null);
            Assert.That(purchaseOrderOneCreateRes.OrderNo, Is.Not.Null.And.Not.Empty);

            // Activate the first purchase order.
            PurchaseOrderACTIVATERequest purchaseOrderOneActivateReq = new PurchaseOrderACTIVATERequest()
            {
                PurchaseOrderID = purchaseOrderOneCreateRes.PurchaseOrderID
            };

            _ = await Client.PostAsync(purchaseOrderOneActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create the second purchase order.
            PurchaseOrderPOSTRequest purchaseOrderTwoCreateReq = new PurchaseOrderPOSTRequest()
            {
                CreditorAccountNo = creditorCreateRes.AccountNo,
                Reference = "PO-" + RandomString(8),
                OrderDate = DateTime.Today,
                Lines = new List<PurchaseOrderLine>()
                {
                    new PurchaseOrderLine()
                    {
                        PartNo = inventoryCreateRes.PartNo,
                        Quantity = 3M
                    }
                }
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrderTwoCreateRes = await Client.PostAsync(purchaseOrderTwoCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseOrderTwoCreateRes.PurchaseOrderID, Is.Not.Null);
            Assert.That(purchaseOrderTwoCreateRes.OrderNo, Is.Not.Null.And.Not.Empty);

            // Activate the second purchase order.
            PurchaseOrderACTIVATERequest purchaseOrderTwoActivateReq = new PurchaseOrderACTIVATERequest()
            {
                PurchaseOrderID = purchaseOrderTwoCreateRes.PurchaseOrderID
            };

            _ = await Client.PostAsync(purchaseOrderTwoActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a shipment from purchase orders.
            LandedCostShipmentCREATEFromPORequest shipmentCreateFromPOReq = new LandedCostShipmentCREATEFromPORequest()
            {
                OrderNos = new[] { purchaseOrderOneCreateRes.OrderNo, purchaseOrderTwoCreateRes.OrderNo }
            };

            ShipmentDto shipmentCreateFromPORes = await Client.PostAsync(shipmentCreateFromPOReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentCreateFromPORes.ShipmentID, Is.Not.Null);

            // Read the created shipment.
            LandedCostShipmentGETRequest shipmentGetReq = new LandedCostShipmentGETRequest()
            {
                ShipmentID = shipmentCreateFromPORes.ShipmentID
            };

            ShipmentDto shipmentGetRes = await Client.GetAsync(shipmentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(shipmentGetRes.ShipmentID, Is.EqualTo(shipmentCreateFromPORes.ShipmentID));

            // Delete the shipment.
            LandedCostShipmentDELETERequest shipmentDeleteReq = new LandedCostShipmentDELETERequest()
            {
                ShipmentID = shipmentCreateFromPORes.ShipmentID
            };

            await Client.DeleteAsync(shipmentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the shipment was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                ShipmentDto deletedShipmentGetRes = await Client.GetAsync(shipmentGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}

