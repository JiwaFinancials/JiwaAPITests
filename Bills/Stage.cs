using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Bills;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillDto = JiwaFinancials.Jiwa.JiwaServiceModel.Bills.Bill;
using BillInputDto = JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillInput;
using BillInstructionDto = JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillInstruction;
using BillOutputDto = JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillOutput;
using BillStageDto = JiwaFinancials.Jiwa.JiwaServiceModel.Bills.BillStage;

namespace JiwaAPITests.Bills
{
    public class Stage : JiwaAPITest
    {
        private async Task<InventoryItem> CreateInventoryItem(string description, decimal defaultPrice)
        {
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = description,
                DefaultPrice = defaultPrice
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.InventoryID, Is.Not.Null);
            Assert.That(itemCreateRes.PartNo, Is.EqualTo(itemCreateReq.PartNo));

            return itemCreateRes;
        }

        private async Task<BillDto> CreateBillWithStage(List<BillInputDto>? inputs = null, List<BillInstructionDto>? instructions = null)
        {
            InventoryItem outputItemCreateRes = await CreateInventoryItem("Output Item Test", 99.99M);

            BillPOSTRequest billCreateReq = new BillPOSTRequest()
            {
                Stages = new List<BillStageDto>()
                {
                    new BillStageDto()
                    {
                        Name = "Stage " + RandomString(6),
                        Inputs = inputs ?? new List<BillInputDto>(),
                        Instructions = instructions ?? new List<BillInstructionDto>()
                    }
                },
                Outputs = new List<BillOutputDto>()
                {
                    new BillOutputDto()
                    {
                        PartNo = outputItemCreateRes.PartNo,
                        Quantity = 1,
                        IsRatio = true
                    }
                }
            };

            BillDto billCreateRes = await Client.PostAsync(billCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(billCreateRes.BillID, Is.Not.Null);
            Assert.That(billCreateRes.Stages.Count, Is.EqualTo(1));

            return billCreateRes;
        }

        #region "Stages"
        [Test]
        public async Task BillStage_CRUD()
        {
            InventoryItem inputItemCreateRes = await CreateInventoryItem("Stage Input Item Test", 12.75M);

            BillDto billCreateRes = await CreateBillWithStage(new List<BillInputDto>()
            {
                new BillInputDto()
                {
                    PartNo = inputItemCreateRes.PartNo,
                    Quantity = 1,
                    IsRatio = true
                }
            });

            BillStagesGETManyRequest stagesGetManyReq = new BillStagesGETManyRequest()
            {
                BillID = billCreateRes.BillID
            };

            List<BillStageDto> stagesGetManyRes = await Client.GetAsync(stagesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(stagesGetManyRes.Count, Is.EqualTo(1));
            Assert.That(stagesGetManyRes.Any(x => x.StageID == billCreateRes.Stages[0].StageID), Is.True);

            BillStagePOSTRequest stageCreateReq = new BillStagePOSTRequest()
            {
                BillID = billCreateRes.BillID,
                Name = "Bill Stage " + RandomString(8)
            };

            BillStageDto stageCreateRes = await Client.PostAsync(stageCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(stageCreateRes.StageID, Is.Not.Null);
            Assert.That(stageCreateRes.Name, Is.EqualTo(stageCreateReq.Name));

            stagesGetManyRes = await Client.GetAsync(stagesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(stagesGetManyRes.Count, Is.EqualTo(2));
            Assert.That(stagesGetManyRes.Any(x => x.StageID == stageCreateRes.StageID), Is.True);

            BillStageGETRequest stageGetReq = new BillStageGETRequest()
            {
                BillID = billCreateRes.BillID,
                StageID = stageCreateRes.StageID
            };

            BillStageDto stageGetRes = await Client.GetAsync(stageGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(stageGetRes.StageID, Is.EqualTo(stageCreateRes.StageID));
            Assert.That(stageGetRes.Name, Is.EqualTo(stageCreateReq.Name));

            BillStagePATCHRequest stagePatchReq = new BillStagePATCHRequest()
            {
                BillID = billCreateRes.BillID,
                StageID = stageCreateRes.StageID,
                Name = "Updated Bill Stage " + RandomString(6)
            };

            BillStageDto stagePatchRes = await Client.PatchAsync(stagePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(stagePatchRes.StageID, Is.EqualTo(stagePatchReq.StageID));
            Assert.That(stagePatchRes.StageID, Is.EqualTo(stageCreateRes.StageID));
            Assert.That(stagePatchRes.Name, Is.EqualTo(stagePatchReq.Name));

            stageGetRes = await Client.GetAsync(stageGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(stageGetRes.Name, Is.EqualTo(stagePatchReq.Name));

            BillStageDELETERequest stageDeleteReq = new BillStageDELETERequest()
            {
                BillID = billCreateRes.BillID,
                StageID = stageCreateRes.StageID
            };

            await Client.DeleteAsync(stageDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                BillStageDto getDeletedRes = await Client.GetAsync(stageGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            stagesGetManyRes = await Client.GetAsync(stagesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(stagesGetManyRes.Count, Is.EqualTo(1));
            Assert.That(stagesGetManyRes.Any(x => x.StageID == stageCreateRes.StageID), Is.False);

            await DeleteBill(billCreateRes.BillID);
        }
        #endregion

        #region "Stage Inputs"
        [Test]
        public async Task BillStageInputs_GETMany()
        {
            InventoryItem inputItem1CreateRes = await CreateInventoryItem("Stage Input Item 1 Test", 12.75M);
            InventoryItem inputItem2CreateRes = await CreateInventoryItem("Stage Input Item 2 Test", 13.25M);

            BillDto billCreateRes = await CreateBillWithStage(new List<BillInputDto>()
            {
                new BillInputDto()
                {
                    PartNo = inputItem1CreateRes.PartNo,
                    Quantity = 1,
                    IsRatio = true
                },
                new BillInputDto()
                {
                    PartNo = inputItem2CreateRes.PartNo,
                    Quantity = 2,
                    IsRatio = true
                }
            });

            BillInputsGETManyRequest inputsGetManyReq = new BillInputsGETManyRequest()
            {
                BillID = billCreateRes.BillID,
                StageID = billCreateRes.Stages[0].StageID
            };

            List<BillInputDto> inputsGetManyRes = await Client.GetAsync(inputsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(inputsGetManyRes.Count, Is.EqualTo(2));
            Assert.That(inputsGetManyRes.Any(x => x.InputID == billCreateRes.Stages[0].Inputs[0].InputID), Is.True);
            Assert.That(inputsGetManyRes.Any(x => x.InputID == billCreateRes.Stages[0].Inputs[1].InputID), Is.True);

            await DeleteBill(billCreateRes.BillID);
        }
        #endregion

        #region "Stage Instructions"
        [Test]
        public async Task BillStageInstructions_CRUD()
        {
            InventoryItem inputItemCreateRes = await CreateInventoryItem("Instruction Stage Input Item Test", 12.75M);

            BillDto billCreateRes = await CreateBillWithStage(new List<BillInputDto>()
            {
                new BillInputDto()
                {
                    PartNo = inputItemCreateRes.PartNo,
                    Quantity = 1,
                    IsRatio = true
                }
            });

            BillInstructionPOSTRequest instructionCreateReq = new BillInstructionPOSTRequest()
            {
                BillID = billCreateRes.BillID,
                StageID = billCreateRes.Stages[0].StageID,
                InstructionText = "Bill Instruction " + RandomString(10)
            };

            BillInstructionDto instructionCreateRes = await Client.PostAsync(instructionCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(instructionCreateRes.InstructionID, Is.Not.Null);
            Assert.That(instructionCreateRes.InstructionText, Is.EqualTo(instructionCreateReq.InstructionText));

            BillInstructionsGETManyRequest instructionsGetManyReq = new BillInstructionsGETManyRequest()
            {
                BillID = billCreateRes.BillID,
                StageID = billCreateRes.Stages[0].StageID
            };

            List<BillInstructionDto> instructionsGetManyRes = await Client.GetAsync(instructionsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(instructionsGetManyRes.Count, Is.EqualTo(1));
            Assert.That(instructionsGetManyRes.Any(x => x.InstructionID == instructionCreateRes.InstructionID), Is.True);

            BillInstructionGETRequest instructionGetReq = new BillInstructionGETRequest()
            {
                BillID = billCreateRes.BillID,
                StageID = billCreateRes.Stages[0].StageID,
                InstructionID = instructionCreateRes.InstructionID
            };

            BillInstructionDto instructionGetRes = await Client.GetAsync(instructionGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(instructionGetRes.InstructionID, Is.EqualTo(instructionCreateRes.InstructionID));
            Assert.That(instructionGetRes.InstructionText, Is.EqualTo(instructionCreateReq.InstructionText));

            BillInstructionPATCHRequest instructionPatchReq = new BillInstructionPATCHRequest()
            {
                BillID = billCreateRes.BillID,
                StageID = billCreateRes.Stages[0].StageID,
                InstructionID = instructionCreateRes.InstructionID,
                InstructionText = "Updated Bill Instruction " + RandomString(8)
            };

            BillInstructionDto instructionPatchRes = await Client.PatchAsync(instructionPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(instructionPatchRes.InstructionID, Is.EqualTo(instructionPatchReq.InstructionID));
            Assert.That(instructionPatchRes.InstructionID, Is.EqualTo(instructionCreateRes.InstructionID));
            Assert.That(instructionPatchRes.InstructionText, Is.EqualTo(instructionPatchReq.InstructionText));

            instructionGetRes = await Client.GetAsync(instructionGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(instructionGetRes.InstructionText, Is.EqualTo(instructionPatchReq.InstructionText));

            BillInstructionDELETERequest instructionDeleteReq = new BillInstructionDELETERequest()
            {
                BillID = billCreateRes.BillID,
                StageID = billCreateRes.Stages[0].StageID,
                InstructionID = instructionCreateRes.InstructionID
            };

            await Client.DeleteAsync(instructionDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                BillInstructionDto getDeletedRes = await Client.GetAsync(instructionGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            instructionsGetManyRes = await Client.GetAsync(instructionsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(instructionsGetManyRes.Count, Is.EqualTo(0));
            Assert.That(instructionsGetManyRes.Any(x => x.InstructionID == instructionCreateRes.InstructionID), Is.False);

            await DeleteBill(billCreateRes.BillID);
        }
        #endregion

        private async Task DeleteBill(string billId)
        {
            BillDELETERequest billDeleteReq = new BillDELETERequest()
            {
                BillID = billId
            };

            await Client.DeleteAsync(billDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
    }
}


