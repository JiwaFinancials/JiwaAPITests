using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder;

namespace JiwaAPITests.WorkOrders
{
    public class Instructions : WorkOrderTestBase
    {
        #region "WorkOrders_Stages_Instructions"
        [Test]
        public async Task WorkOrders_Stages_Instructions_CRUD()
        {
            // Create a work order to use for stage instruction tests.
            WorkOrderDto workOrderCreateRes = await CreateWorkOrderAsync();
            string workOrderID = workOrderCreateRes.WorkOrderID;
            string stageID = workOrderCreateRes.Stages[0].StageID;

            // Read all instructions for the work order stage.
            WorkOrderInstructionsGETManyRequest instructionsGetManyReq = new WorkOrderInstructionsGETManyRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID
            };

            List<WorkOrderInstruction> instructionsGetManyRes = await Client.GetAsync(instructionsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Add an instruction to the work order stage.
            WorkOrderInstructionPOSTRequest instructionCreateReq = new WorkOrderInstructionPOSTRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InstructionText = "Instruction " + RandomString(8)
            };

            WorkOrderInstruction instructionCreateRes = await Client.PostAsync(instructionCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(instructionCreateRes.InstructionID, Is.Not.Null);
            Assert.That(instructionCreateRes.InstructionText, Is.EqualTo(instructionCreateReq.InstructionText));

            // Read all instructions again and ensure the new instruction is returned.
            instructionsGetManyRes = await Client.GetAsync(instructionsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(instructionsGetManyRes.Any(x => x.InstructionID == instructionCreateRes.InstructionID), Is.True);

            // Read the created instruction.
            WorkOrderInstructionGETRequest instructionGetReq = new WorkOrderInstructionGETRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InstructionID = instructionCreateRes.InstructionID
            };

            WorkOrderInstruction instructionGetRes = await Client.GetAsync(instructionGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(instructionGetRes.InstructionID, Is.EqualTo(instructionCreateRes.InstructionID));
            Assert.That(instructionGetRes.InstructionText, Is.EqualTo(instructionCreateReq.InstructionText));

            // Update the instruction.
            WorkOrderInstructionPATCHRequest instructionPatchReq = new WorkOrderInstructionPATCHRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InstructionID = instructionCreateRes.InstructionID,
                InstructionText = "Updated instruction " + RandomString(8)
            };

            WorkOrderInstruction instructionPatchRes = await Client.PatchAsync(instructionPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(instructionPatchRes.InstructionID, Is.EqualTo(instructionPatchReq.InstructionID));
            Assert.That(instructionPatchRes.InstructionID, Is.EqualTo(instructionCreateRes.InstructionID));
            Assert.That(instructionPatchRes.InstructionText, Is.EqualTo(instructionPatchReq.InstructionText));

            // Verify the instruction was updated.
            instructionGetRes = await Client.GetAsync(instructionGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(instructionGetRes.InstructionText, Is.EqualTo(instructionPatchReq.InstructionText));

            // Delete the instruction.
            WorkOrderInstructionDELETERequest instructionDeleteReq = new WorkOrderInstructionDELETERequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InstructionID = instructionCreateRes.InstructionID
            };

            await Client.DeleteAsync(instructionDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the instruction was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(instructionGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all instructions again and ensure the deleted instruction is no longer returned.
            instructionsGetManyRes = await Client.GetAsync(instructionsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(instructionsGetManyRes.Any(x => x.InstructionID == instructionCreateRes.InstructionID), Is.False);
        }
        #endregion
    }
}


