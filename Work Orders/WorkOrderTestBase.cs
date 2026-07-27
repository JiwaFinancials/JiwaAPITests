using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Bills;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder;

namespace JiwaAPITests.WorkOrders
{
    public class WorkOrderTestBase : JiwaAPITest
    {
        public async Task<WorkOrderDto> CreateWorkOrderAsync()
        {
            // Create an item for the output
            InventoryPOSTRequest outputItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Output Item Test",
                DefaultPrice = 99.99M
            };

            InventoryItem outputItemCreateRes = await Client.PostAsync(outputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(outputItemCreateRes.InventoryID, Is.Not.Null);

            // Create an input item
            InventoryPOSTRequest inputItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Input Item Test",
                DefaultPrice = 12.75M
            };

            InventoryItem inputItemCreateRes = await Client.PostAsync(inputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inputItemCreateRes.InventoryID, Is.Not.Null);

            // Create a bill
            BillPOSTRequest billCreateReq = new BillPOSTRequest()
            {
                Stages = new List<BillStage>()
                {
                    new BillStage()
                    {
                        Name = "Stage 1",
                        Inputs = new List<BillInput>()
                        {
                            new BillInput()
                            {
                                PartNo = inputItemCreateRes.PartNo,
                                Quantity = 1,
                                IsRatio = true
                            }
                        }
                    }
                },
                Outputs = new List<BillOutput>()
                {
                    new BillOutput()
                    {
                        PartNo = outputItemCreateRes.PartNo,
                        Quantity = 1,
                        IsRatio = true
                    }
                }
            };

            Bill billCreateRes = await Client.PostAsync(billCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(billCreateRes.BillID, Is.Not.Null);

            // Create a work order
            WorkOrderPOSTRequest workOrderCreateReq = new WorkOrderPOSTRequest()
            {
                BillID = billCreateRes.BillID
            };

            WorkOrderDto workOrderCreateRes = await Client.PostAsync(workOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(workOrderCreateRes.WorkOrderID, Is.Not.Null);

            return workOrderCreateRes;
        }

        public async Task EnsureStockOnHandAsync(string partNo, decimal quantity)
        {
            // Create a creditor for purchase order and goods received note operations.
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Work Order Stock Seed Creditor"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);

            // Create a purchase order line for the requested inventory part.
            PurchaseOrderPOSTRequest purchaseOrderCreateReq = new PurchaseOrderPOSTRequest()
            {
                CreditorAccountNo = creditorCreateRes.AccountNo,
                Reference = "PO-" + RandomString(8),
                OrderDate = DateTime.Today,
                Lines = new List<PurchaseOrderLine>()
                {
                    new PurchaseOrderLine()
                    {
                        PartNo = partNo,
                        Quantity = quantity
                    }
                }
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrderCreateRes = await Client.PostAsync(purchaseOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseOrderCreateRes.PurchaseOrderID, Is.Not.Null);

            // Activate the purchase order so stock can be received.
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
                        Quantity = quantity
                    }
                }
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes.GoodsReceivedNote grnCreateRes = await Client.PostAsync(grnCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(grnCreateRes.GRNID, Is.Not.Null);

            // Activate the goods received note to commit stock on hand.
            GoodsReceivedNoteACTIVATERequest grnActivateReq = new GoodsReceivedNoteACTIVATERequest()
            {
                GRNID = grnCreateRes.GRNID
            };

            _ = await Client.PostAsync(grnActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
        }
    }
}

