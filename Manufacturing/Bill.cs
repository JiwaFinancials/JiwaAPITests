using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using JiwaFinancials.Jiwa.JiwaServiceModel.Bills;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables.Or;

namespace JiwaAPITests.Manufacturing
{
    public class Bill : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Bill_CRUD()
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

            // Get the bill
            BillGETRequest billGetReq = new BillGETRequest() { BillID = billCreateRes.BillID };
            JiwaFinancials.Jiwa.JiwaServiceModel.Bills.Bill billGetRes = await Client.GetAsync(billGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(billGetRes.BillID, Is.EqualTo(billCreateRes.BillID));

            // Get an input item from the bill
            BillInputGETRequest billInputItemGetReq = new BillInputGETRequest()
            {
                BillID = billCreateRes.BillID,
                StageID = billCreateRes.Stages[0].StageID,
                InputID = billCreateRes.Stages[0].Inputs[0].InputID
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillInput billInputItemGetRes = await Client.GetAsync(billInputItemGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(billInputItemGetRes.PartNo, Is.EqualTo(inputItem1CreateReq.PartNo));
            Assert.That(billInputItemGetRes.InputID, Is.EqualTo(billCreateRes.Stages[0].Inputs[0].InputID));

            // Remove an input from the bill
            BillInputDELETERequest billInputItemDeleteReq = new BillInputDELETERequest()
            {
                BillID = billCreateRes.BillID,
                StageID = billCreateRes.Stages[0].StageID,
                InputID = billCreateRes.Stages[0].Inputs[0].InputID
            };
            await Client.DeleteAsync(billInputItemDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the bill input is no longer present
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillInput getDeletedRes = await Client.GetAsync(billInputItemGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Add another Input to the bill
            JiwaFinancials.Jiwa.JiwaServiceModel.BillInputPOSTRequest billInputItemCreateReq = new BillInputPOSTRequest()
            {
                BillID = billCreateRes.BillID,
                StageID = billCreateRes.Stages[0].StageID,
                PartNo = inputItem1CreateRes.PartNo,
                Quantity = 1,
                IsRatio = true
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillInput billInputItemCreateRes = await Client.PostAsync(billInputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(billInputItemCreateRes.InputID, !Is.Null);

            // Update an input on the bill
            JiwaFinancials.Jiwa.JiwaServiceModel.BillInputPATCHRequest billInputItemPachReq = new BillInputPATCHRequest()
            {
                BillID = billCreateRes.BillID,
                StageID = billCreateRes.Stages[0].StageID,
                InputID = billInputItemCreateRes.InputID,
                Quantity = 5
            };
            JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillInput billInputItemPatchRes = await Client.PatchAsync(billInputItemPachReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(billInputItemPatchRes.Quantity, Is.EqualTo(billInputItemPachReq.Quantity));

            // Delete the bill
            BillDELETERequest billDeleteReq = new BillDELETERequest()
            {
                BillID = billCreateRes.BillID
            };
            await Client.DeleteAsync(billDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the deleted bill is not present
            billGetReq = new BillGETRequest() { BillID = billCreateRes.BillID };            
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.Bills.Bill getDeletedRes = await Client.GetAsync(billGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}
