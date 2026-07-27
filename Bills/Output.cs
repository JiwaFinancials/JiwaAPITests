using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.Bills;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.Bills
{
    public class Output : JiwaAPITest
    {
        #region "Outputs"
        [Test]
        public async Task BillOutput_CRUD()
        {
            // Create bill items
            InventoryPOSTRequest initialOutputItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Initial Output Item Test",
                DefaultPrice = 99.99M
            };

            InventoryItem initialOutputItemCreateRes = await Client.PostAsync(initialOutputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(initialOutputItemCreateRes.InventoryID, Is.Not.Null);

            InventoryPOSTRequest appendedOutputItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Appended Output Item Test",
                DefaultPrice = 149.99M
            };

            InventoryItem appendedOutputItemCreateRes = await Client.PostAsync(appendedOutputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(appendedOutputItemCreateRes.InventoryID, Is.Not.Null);

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
                        PartNo = initialOutputItemCreateRes.PartNo,
                        Quantity = 1,
                        IsRatio = true
                    }
                }
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Bills.Bill billCreateRes = await Client.PostAsync(billCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(billCreateRes.BillID, Is.Not.Null);
            Assert.That(billCreateRes.Outputs.Count, Is.EqualTo(1));

            // Read all bill outputs and ensure the original output is returned
            BillOutputsGETManyRequest outputsGetManyReq = new BillOutputsGETManyRequest()
            {
                BillID = billCreateRes.BillID
            };

            List<BillOutput> outputsGetManyRes = await Client.GetAsync(outputsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(outputsGetManyRes.Count, Is.EqualTo(1));
            Assert.That(outputsGetManyRes.Any(x => x.OutputID == billCreateRes.Outputs[0].OutputID), Is.True);

            // Append a bill output
            BillOutputPOSTRequest outputCreateReq = new BillOutputPOSTRequest()
            {
                BillID = billCreateRes.BillID,
                PartNo = appendedOutputItemCreateRes.PartNo,
                Quantity = 2,
                IsRatio = true,
                Note = "Bill output note " + RandomString(6)
            };

            BillOutput outputCreateRes = await Client.PostAsync(outputCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(outputCreateRes.OutputID, Is.Not.Null);
            Assert.That(outputCreateRes.PartNo, Is.EqualTo(outputCreateReq.PartNo));
            Assert.That(outputCreateRes.Quantity, Is.EqualTo(outputCreateReq.Quantity));
            Assert.That(outputCreateRes.Note, Is.EqualTo(outputCreateReq.Note));

            // Read all bill outputs again and ensure the appended output is returned
            outputsGetManyRes = await Client.GetAsync(outputsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(outputsGetManyRes.Count, Is.EqualTo(2));
            Assert.That(outputsGetManyRes.Any(x => x.OutputID == outputCreateRes.OutputID), Is.True);

            // Read the appended bill output using the OutputID
            BillOutputGETRequest outputGetReq = new BillOutputGETRequest()
            {
                BillID = billCreateRes.BillID,
                OutputID = outputCreateRes.OutputID
            };

            BillOutput outputGetRes = await Client.GetAsync(outputGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(outputGetRes.OutputID, Is.EqualTo(outputCreateRes.OutputID));
            Assert.That(outputGetRes.PartNo, Is.EqualTo(outputCreateReq.PartNo));
            Assert.That(outputGetRes.Quantity, Is.EqualTo(outputCreateReq.Quantity));
            Assert.That(outputGetRes.Note, Is.EqualTo(outputCreateReq.Note));

            // Update the appended bill output
            BillOutputPATCHRequest outputPatchReq = new BillOutputPATCHRequest()
            {
                BillID = billCreateRes.BillID,
                OutputID = outputCreateRes.OutputID,
                Quantity = 5,
                IsRatio = false,
                Note = "Updated bill output note " + RandomString(6)
            };

            BillOutput outputPatchRes = await Client.PatchAsync(outputPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(outputPatchRes.OutputID, Is.EqualTo(outputCreateRes.OutputID));
            Assert.That(outputPatchRes.Quantity, Is.EqualTo(outputPatchReq.Quantity));
            Assert.That(outputPatchRes.IsRatio, Is.EqualTo(outputPatchReq.IsRatio));
            Assert.That(outputPatchRes.Note, Is.EqualTo(outputPatchReq.Note));

            // Read the updated bill output using the OutputID
            outputGetRes = await Client.GetAsync(outputGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(outputGetRes.Quantity, Is.EqualTo(outputPatchReq.Quantity));
            Assert.That(outputGetRes.IsRatio, Is.EqualTo(outputPatchReq.IsRatio));
            Assert.That(outputGetRes.Note, Is.EqualTo(outputPatchReq.Note));

            // Remove the appended bill output
            BillOutputDELETERequest outputDeleteReq = new BillOutputDELETERequest()
            {
                BillID = billCreateRes.BillID,
                OutputID = outputCreateRes.OutputID
            };

            await Client.DeleteAsync(outputDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted bill output is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                BillOutput getDeletedRes = await Client.GetAsync(outputGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all bill outputs and ensure the deleted output is no longer returned
            outputsGetManyRes = await Client.GetAsync(outputsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(outputsGetManyRes.Count, Is.EqualTo(1));
            Assert.That(outputsGetManyRes.Any(x => x.OutputID == outputCreateRes.OutputID), Is.False);

            // Remove the created bill
            BillDELETERequest billDeleteReq = new BillDELETERequest()
            {
                BillID = billCreateRes.BillID
            };

            await Client.DeleteAsync(billDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

