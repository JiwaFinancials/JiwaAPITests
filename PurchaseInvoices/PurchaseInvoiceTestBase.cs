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
using PurchaseInvoiceDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseInvoices.PurchaseInvoice;
using PurchaseOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder;

namespace JiwaAPITests.PurchaseInvoices
{
    public abstract class PurchaseInvoiceTestBase : JiwaAPITest
    {
        protected async Task<(Creditor creditor, InventoryItem inventoryItem, PurchaseOrderDto purchaseOrder, GoodsReceivedNoteDto goodsReceivedNote)> CreateGoodsReceivedNoteWithDependenciesAsync()
        {
            // Create an inventory item for the purchase order line.
            InventoryPOSTRequest inventoryCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Purchase Invoice Test Item",
                DefaultPrice = 10.00M
            };

            InventoryItem inventoryCreateRes = await Client.PostAsync(inventoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inventoryCreateRes.InventoryID, Is.Not.Null);

            // Create a creditor for downstream purchase order and purchase invoice operations.
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Purchase Invoice Test Creditor"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);

            // Create a purchase order with a single line.
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
                        Quantity = 3M
                    }
                }
            };

            PurchaseOrderDto purchaseOrderCreateRes = await Client.PostAsync(purchaseOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseOrderCreateRes.PurchaseOrderID, Is.Not.Null);
            Assert.That(purchaseOrderCreateRes.Lines.Count, Is.EqualTo(1));
            Assert.That(purchaseOrderCreateRes.Lines[0].PurchaseOrderLineID, Is.Not.Null);

            // Activate the purchase order so a goods received note can be created from its line.
            PurchaseOrderACTIVATERequest purchaseOrderActivateReq = new PurchaseOrderACTIVATERequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID
            };

            _ = await Client.PostAsync(purchaseOrderActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a goods received note from the purchase order line.
            GoodsReceivedNoteCREATEFromPOLinesRequest grnCreateReq = new GoodsReceivedNoteCREATEFromPOLinesRequest()
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

            GoodsReceivedNoteDto grnCreateRes = await Client.PostAsync(grnCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(grnCreateRes.GRNID, Is.Not.Null);
            Assert.That(grnCreateRes.SlipNo, Is.Not.Null.And.Not.Empty);

            // Activate the goods received note so it can be added to a purchase invoice.
            GoodsReceivedNoteACTIVATERequest grnActivateReq = new GoodsReceivedNoteACTIVATERequest()
            {
                GRNID = grnCreateRes.GRNID
            };

            _ = await Client.PostAsync(grnActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            return (creditorCreateRes, inventoryCreateRes, purchaseOrderCreateRes, grnCreateRes);
        }

        protected async Task<PurchaseInvoiceDto> CreatePurchaseInvoiceFromGoodsReceivedNoteAsync(GoodsReceivedNoteDto goodsReceivedNote)
        {
            // Create a purchase invoice from the goods received note number.
            PurchaseInvoiceCREATEFromGRNRequest purchaseInvoiceCreateFromGrnReq = new PurchaseInvoiceCREATEFromGRNRequest()
            {
                GRNNos = new[] { goodsReceivedNote.SlipNo }
            };

            PurchaseInvoiceDto purchaseInvoiceCreateFromGrnRes = await Client.PostAsync(purchaseInvoiceCreateFromGrnReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseInvoiceCreateFromGrnRes.PurchaseInvoiceID, Is.Not.Null);

            return purchaseInvoiceCreateFromGrnRes;
        }
    }
}

