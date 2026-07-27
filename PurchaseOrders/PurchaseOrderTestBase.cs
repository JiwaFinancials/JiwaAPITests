using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PurchaseOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder;

namespace JiwaAPITests.PurchaseOrders
{
    public class PurchaseOrderTestBase : JiwaAPITest
    {
        public async Task<(Creditor creditor, InventoryItem inventoryItem, PurchaseOrderDto purchaseOrder)> CreatePurchaseOrderWithLineAsync()
        {
            // Create an inventory item for the purchase order line.
            InventoryPOSTRequest inventoryCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Purchase Order Test Item",
                DefaultPrice = 10.00M
            };

            InventoryItem inventoryCreateRes = await Client.PostAsync(inventoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inventoryCreateRes.InventoryID, Is.Not.Null);

            // Create a creditor for the purchase order.
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Purchase Order Test Creditor"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);

            // Create the purchase order.
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
                        Quantity = 5M
                    }
                }
            };

            PurchaseOrderDto purchaseOrderCreateRes = await Client.PostAsync(purchaseOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseOrderCreateRes.PurchaseOrderID, Is.Not.Null);
            Assert.That(purchaseOrderCreateRes.Reference, Is.EqualTo(purchaseOrderCreateReq.Reference));
            Assert.That(purchaseOrderCreateRes.Lines.Count, Is.EqualTo(1));
            Assert.That(purchaseOrderCreateRes.Lines[0].PurchaseOrderLineID, Is.Not.Null);

            return (creditorCreateRes, inventoryCreateRes, purchaseOrderCreateRes);
        }
    }
}

