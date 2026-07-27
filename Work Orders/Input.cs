using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder;

namespace JiwaAPITests.WorkOrders
{
    public class Input : WorkOrderTestBase
    {
        #region "WorkOrders_Stages_Inputs"
        [Test]
        public async Task WorkOrders_Stages_Inputs_CRUD()
        {
            // Create an inventory item for the stage input.
            InventoryPOSTRequest inputItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Work Order Stage Input Test Item",
                DefaultPrice = 10.00M
            };

            InventoryItem inputItemCreateRes = await Client.PostAsync(inputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inputItemCreateRes.InventoryID, Is.Not.Null);

            // Create a work order to use for stage input tests.
            WorkOrderDto workOrderCreateRes = await CreateWorkOrderAsync();

            string workOrderID = workOrderCreateRes.WorkOrderID;
            string stageID = workOrderCreateRes.Stages[0].StageID;

            // Read all inputs for the work order stage.
            WorkOrderInputsGETManyRequest inputsGetManyReq = new WorkOrderInputsGETManyRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID
            };

            List<WorkOrderInput> inputsGetManyRes = await Client.GetAsync(inputsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Append an input to the work order stage.
            WorkOrderInputPOSTRequest inputCreateReq = new WorkOrderInputPOSTRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                PartNo = inputItemCreateRes.PartNo,
                Quantity = 3M,
                IsRatio = true
            };

            WorkOrderInput inputCreateRes = await Client.PostAsync(inputCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inputCreateRes.InputID, Is.Not.Null);
            Assert.That(inputCreateRes.PartNo, Is.EqualTo(inputCreateReq.PartNo));

            // Read all inputs again and ensure the new input is returned.
            inputsGetManyRes = await Client.GetAsync(inputsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(inputsGetManyRes.Any(x => x.InputID == inputCreateRes.InputID), Is.True);

            // Read the created input.
            WorkOrderInputGETRequest inputGetReq = new WorkOrderInputGETRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InputID = inputCreateRes.InputID
            };

            WorkOrderInput inputGetRes = await Client.GetAsync(inputGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(inputGetRes.InputID, Is.EqualTo(inputCreateRes.InputID));
            Assert.That(inputGetRes.PartNo, Is.EqualTo(inputCreateReq.PartNo));

            // Update the input.
            WorkOrderInputPATCHRequest inputPatchReq = new WorkOrderInputPATCHRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InputID = inputCreateRes.InputID,
                Quantity = 7M
            };

            WorkOrderInput inputPatchRes = await Client.PatchAsync(inputPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(inputPatchRes.InputID, Is.EqualTo(inputCreateRes.InputID));
            Assert.That(inputPatchRes.Quantity, Is.EqualTo(inputPatchReq.Quantity));

            // Verify the input was updated.
            inputGetRes = await Client.GetAsync(inputGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(inputGetRes.Quantity, Is.EqualTo(inputPatchReq.Quantity));

            // Delete the input.
            WorkOrderInputDELETERequest inputDeleteReq = new WorkOrderInputDELETERequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InputID = inputCreateRes.InputID
            };

            await Client.DeleteAsync(inputDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the input was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(inputGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all inputs again and ensure the deleted input is no longer returned.
            inputsGetManyRes = await Client.GetAsync(inputsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(inputsGetManyRes.Any(x => x.InputID == inputCreateRes.InputID), Is.False);
        }
        #endregion
    }
}

