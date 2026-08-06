using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder;
using WorkOrderOutputDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrderOutput;

namespace JiwaAPITests.WorkOrders
{
    public class Output : WorkOrderTestBase
    {
        #region "WorkOrders_Outputs"
        [Test]
        public async Task WorkOrders_Outputs_CRUD()
        {
            // Create a work order to use for output tests.
            WorkOrderDto workOrderCreateRes = await CreateWorkOrderAsync();

            // Create an inventory item for the appended output.
            InventoryPOSTRequest outputItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Work Order Output Test Item",
                DefaultPrice = 99.99M
            };

            InventoryItem outputItemCreateRes = await Client.PostAsync(outputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(outputItemCreateRes.InventoryID, Is.Not.Null);

            // Read all work order outputs.
            WorkOrderOutputsGETManyRequest outputsGetManyReq = new WorkOrderOutputsGETManyRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID
            };

            List<WorkOrderOutputDto> outputsGetManyRes = await Client.GetAsync(outputsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(outputsGetManyRes.Count, Is.GreaterThan(0));
            Assert.That(outputsGetManyRes.Any(x => x.OutputID == workOrderCreateRes.Outputs[0].OutputID), Is.True);

            // Append a work order output.
            WorkOrderOutputPOSTRequest outputCreateReq = new WorkOrderOutputPOSTRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                PartNo = outputItemCreateRes.PartNo,
                Quantity = 2M,
                IsRatio = true,
                Note = "Work order output note " + RandomString(6)
            };

            WorkOrderOutputDto outputCreateRes = await Client.PostAsync(outputCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(outputCreateRes.OutputID, Is.Not.Null);
            Assert.That(outputCreateRes.PartNo, Is.EqualTo(outputCreateReq.PartNo));
            Assert.That(outputCreateRes.Quantity, Is.EqualTo(outputCreateReq.Quantity));
            Assert.That(outputCreateRes.IsRatio, Is.EqualTo(outputCreateReq.IsRatio));
            Assert.That(outputCreateRes.Note, Is.EqualTo(outputCreateReq.Note));

            // Read all work order outputs again and ensure the appended output is returned.
            outputsGetManyRes = await Client.GetAsync(outputsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(outputsGetManyRes.Any(x => x.OutputID == outputCreateRes.OutputID), Is.True);

            // Read the appended work order output.
            WorkOrderOutputGETRequest outputGetReq = new WorkOrderOutputGETRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                OutputID = outputCreateRes.OutputID
            };

            WorkOrderOutputDto outputGetRes = await Client.GetAsync(outputGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(outputGetRes.OutputID, Is.EqualTo(outputCreateRes.OutputID));
            Assert.That(outputGetRes.PartNo, Is.EqualTo(outputCreateReq.PartNo));
            Assert.That(outputGetRes.Quantity, Is.EqualTo(outputCreateReq.Quantity));
            Assert.That(outputGetRes.Note, Is.EqualTo(outputCreateReq.Note));

            // Update the appended work order output.
            WorkOrderOutputPATCHRequest outputPatchReq = new WorkOrderOutputPATCHRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                OutputID = outputCreateRes.OutputID,
                Quantity = 5M,
                IsRatio = false,
                Note = "Updated work order output note " + RandomString(6)
            };

            WorkOrderOutputDto outputPatchRes = await Client.PatchAsync(outputPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(outputPatchRes.OutputID, Is.EqualTo(outputPatchReq.OutputID));
            Assert.That(outputPatchRes.OutputID, Is.EqualTo(outputCreateRes.OutputID));
            Assert.That(outputPatchRes.Quantity, Is.EqualTo(outputPatchReq.Quantity));
            Assert.That(outputPatchRes.IsRatio, Is.EqualTo(outputPatchReq.IsRatio));
            Assert.That(outputPatchRes.Note, Is.EqualTo(outputPatchReq.Note));

            // Read the updated work order output.
            outputGetRes = await Client.GetAsync(outputGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(outputGetRes.Quantity, Is.EqualTo(outputPatchReq.Quantity));
            Assert.That(outputGetRes.IsRatio, Is.EqualTo(outputPatchReq.IsRatio));
            Assert.That(outputGetRes.Note, Is.EqualTo(outputPatchReq.Note));

            // Delete the appended work order output.
            WorkOrderOutputDELETERequest outputDeleteReq = new WorkOrderOutputDELETERequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                OutputID = outputCreateRes.OutputID
            };

            await Client.DeleteAsync(outputDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the work order output was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(outputGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all work order outputs again and ensure the deleted output is no longer returned.
            outputsGetManyRes = await Client.GetAsync(outputsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(outputsGetManyRes.Any(x => x.OutputID == outputCreateRes.OutputID), Is.False);
        }
        #endregion
    }
}


