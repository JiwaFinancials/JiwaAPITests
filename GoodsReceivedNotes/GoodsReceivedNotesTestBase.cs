using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoodsReceivedNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes.GoodsReceivedNote;

namespace JiwaAPITests.GoodsReceivedNotes
{
    public abstract class GoodsReceivedNotesTestBase : JiwaAPITest
    {
        protected async Task<GoodsReceivedNoteDto> CreateGoodsReceivedNoteAsync(Creditor creditor)
        {
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
            return grnCreateRes;
        }

        protected async Task<GoodsReceivedNoteDto> CreateGoodsReceivedNoteFromPurchaseOrderLineAsync(JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrder, decimal quantity)
        {
            GoodsReceivedNoteCREATEFromPOLinesRequest grnCreateFromPOLinesReq = new GoodsReceivedNoteCREATEFromPOLinesRequest()
            {
                ReceivedDate = DateTime.Today,
                ReceivedPOLineQuantities = new List<ReceivedPOLineQuantity>()
                {
                    new ReceivedPOLineQuantity()
                    {
                        OrderLineID = purchaseOrder.Lines[0].PurchaseOrderLineID,
                        Quantity = quantity
                    }
                }
            };

            GoodsReceivedNoteDto grnCreateFromPOLinesRes = await Client.PostAsync(grnCreateFromPOLinesReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(grnCreateFromPOLinesRes.GRNID, Is.Not.Null);
            return grnCreateFromPOLinesRes;
        }

        protected async Task ActivatePurchaseOrderAsync(string purchaseOrderID)
        {
            PurchaseOrderACTIVATERequest purchaseOrderActivateReq = new PurchaseOrderACTIVATERequest()
            {
                PurchaseOrderID = purchaseOrderID
            };

            await Client.PostAsync(purchaseOrderActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
        }

        protected async Task<(Creditor creditor, JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrder)> CreatePurchaseOrderWithLineAsync()
        {
            InventoryPOSTRequest inventoryCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Goods Received Note Test Item",
                DefaultPrice = 10.00M
            };

            InventoryItem inventoryCreateRes = await Client.PostAsync(inventoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inventoryCreateRes.InventoryID, Is.Not.Null);

            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Goods Received Note Test Creditor"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);

            PurchaseOrderPOSTRequest purchaseOrderCreateReq = new PurchaseOrderPOSTRequest()
            {
                CreditorAccountNo = creditorCreateRes.AccountNo,
                Lines = new List<PurchaseOrderLine>()
                {
                    new PurchaseOrderLine()
                    {
                        PartNo = inventoryCreateRes.PartNo,
                        Quantity = 5M
                    }
                }
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrderCreateRes = await Client.PostAsync(purchaseOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseOrderCreateRes.PurchaseOrderID, Is.Not.Null);
            Assert.That(purchaseOrderCreateRes.Lines.Count, Is.EqualTo(1));

            return (creditorCreateRes, purchaseOrderCreateRes);
        }
    }
}

