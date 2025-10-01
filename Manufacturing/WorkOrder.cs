using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;

namespace JiwaAPITests.Manufacturing
{
    public class WorkOrder : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task WorkOrder_CRUD()
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
            Assert.That(outputItemCreateRes.PartNo, Is.EqualTo(outputItemCreateReq.PartNo));
            Assert.That(outputItemCreateRes.InventoryID, !Is.Null);

            // Create the input items            
            InventoryPOSTRequest inputItem1CreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Input Item 1 Test",
                DefaultPrice = 12.75M
            };
            
            InventoryItem inputItem1CreateRes = await Client.PostAsync(inputItem1CreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inputItem1CreateRes.PartNo, Is.EqualTo(inputItem1CreateReq.PartNo));
            Assert.That(inputItem1CreateRes.InventoryID, !Is.Null);

            InventoryPOSTRequest inputItem2CreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Input Item 2 Test",
                DefaultPrice = 13.14M
            };

            InventoryItem inputItem2CreateRes = await Client.PostAsync(inputItem2CreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inputItem2CreateRes.PartNo, Is.EqualTo(inputItem2CreateReq.PartNo));
            Assert.That(inputItem2CreateRes.InventoryID, !Is.Null);

            InventoryPOSTRequest inputItem3CreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Input Item 3 Test",
                DefaultPrice = 17.95M
            };

            InventoryItem inputItem3CreateRes = await Client.PostAsync(inputItem3CreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inputItem3CreateRes.PartNo, Is.EqualTo(inputItem3CreateReq.PartNo));
            Assert.That(inputItem3CreateRes.InventoryID, !Is.Null);

            // Create a bill
            BillPOSTRequest billCreateReq = new BillPOSTRequest()
            {
                Stages = new List<JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillStage>()
                {
                    {
                        new JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillStage()
                        {
                            Name = "Stage 1",
                            Inputs = new List<JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillInput>()
                            {
                                new JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillInput()
                                {
                                    PartNo = inputItem1CreateRes.PartNo, Quantity = 1, IsRatio = true
                                },
                                new JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillInput()
                                {
                                    PartNo = inputItem2CreateRes.PartNo, Quantity = 2, IsRatio = true
                                },
                                new JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillInput()
                                {
                                    PartNo = inputItem3CreateRes.PartNo, Quantity = 1, IsRatio = true
                                }
                            }
                        }
                    }
                },
                Outputs = new List<JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillOutput>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillOutput() { PartNo = outputItemCreateRes.PartNo, Quantity = 1, IsRatio = true }
                }
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.Bills.Bill billCreateRes = await Client.PostAsync(billCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(billCreateRes.BillID, !Is.Null);

            // Create a work order with the bill created previously
            WorkOrderPOSTRequest workOrderCreateReq = new WorkOrderPOSTRequest()
            {
                BillID = billCreateRes.BillID
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder workOrderCreateRes = await Client.PostAsync(workOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(workOrderCreateRes.WorkOrderID, !Is.Null);
            Assert.That(workOrderCreateRes.BillID, Is.EqualTo(billCreateRes.BillID));
            Assert.That(workOrderCreateRes.Stages[0].Inputs.Count, Is.EqualTo(billCreateReq.Stages[0].Inputs.Count));
            Assert.That(workOrderCreateRes.Outputs.Count, Is.EqualTo(billCreateReq.Outputs.Count));

            // Add an input to the work order
            JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrderInputPOSTRequest workOrderInputItemCreateReq = new WorkOrderInputPOSTRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                StageID = workOrderCreateRes.Stages[0].StageID,
                PartNo = inputItem1CreateRes.PartNo,
                Quantity = 99,
                IsRatio = true
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrderInput workOrderInputItemCreateRes = await Client.PostAsync(workOrderInputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(workOrderInputItemCreateRes.InputID, !Is.Null);

            // Get an item from the bill
            WorkOrderInputGETRequest workOrderInputItemGetReq = new WorkOrderInputGETRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                StageID = workOrderCreateRes.Stages[0].StageID,
                InputID = workOrderInputItemCreateRes.InputID
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrderInput workOrderInputItemGetRes = await Client.GetAsync(workOrderInputItemGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(workOrderInputItemGetRes.PartNo, Is.EqualTo(inputItem1CreateReq.PartNo));
            Assert.That(workOrderInputItemGetRes.InputID, Is.EqualTo(workOrderInputItemCreateRes.InputID));

            // Update an input on the work order
            WorkOrderInputPATCHRequest workOrderInputItemPatchReq = new WorkOrderInputPATCHRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                StageID = workOrderCreateRes.Stages[0].StageID,
                InputID = workOrderInputItemCreateRes.InputID,
                Quantity = 11
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrderInput workOrderInputItemPatchRes = await Client.PatchAsync(workOrderInputItemPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(workOrderInputItemPatchRes.Quantity, Is.EqualTo(workOrderInputItemPatchReq.Quantity));

            // Verify the item has been patched
            workOrderInputItemGetReq = new WorkOrderInputGETRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                StageID = workOrderCreateRes.Stages[0].StageID,
                InputID = workOrderInputItemCreateRes.InputID
            };
            workOrderInputItemGetRes = await Client.GetAsync(workOrderInputItemGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));            
            Assert.That(workOrderInputItemGetRes.InputID, Is.EqualTo(workOrderInputItemPatchReq.InputID));
            Assert.That(workOrderInputItemGetRes.Quantity, Is.EqualTo(workOrderInputItemPatchReq.Quantity));

            // Delete an input from the work order
            WorkOrderInputDELETERequest workOrderInputItemDeleteReq = new WorkOrderInputDELETERequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                StageID = workOrderCreateRes.Stages[0].StageID,
                InputID = workOrderInputItemCreateRes.InputID
            };
            await Client.DeleteAsync(workOrderInputItemDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the input item is no longer on the work order
            workOrderInputItemGetReq = new WorkOrderInputGETRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                StageID = workOrderCreateRes.Stages[0].StageID,
                InputID = workOrderInputItemCreateRes.InputID
            };            
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrderInput getDeletedRes = await Client.GetAsync(workOrderInputItemGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}
