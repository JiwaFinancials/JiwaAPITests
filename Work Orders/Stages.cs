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
    public class Stages : WorkOrderTestBase
    {
        #region "WorkOrders_Stages"
        [Test]
        public async Task WorkOrders_Stages_CRUD()
        {
            // Create a work order to use for stage tests.
            WorkOrderDto workOrderCreateRes = await CreateWorkOrderAsync();

            // Read all stages for the work order.
            WorkOrderStagesGETManyRequest stagesGetManyReq = new WorkOrderStagesGETManyRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID
            };

            List<WorkOrderStage> stagesGetManyRes = await Client.GetAsync(stagesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(stagesGetManyRes.Count, Is.GreaterThan(0));

            // Append a new stage to the work order.
            WorkOrderStagePOSTRequest stageCreateReq = new WorkOrderStagePOSTRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                Name = "Test Stage " + RandomString(6)
            };

            WorkOrderStage stageCreateRes = await Client.PostAsync(stageCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(stageCreateRes.StageID, Is.Not.Null);
            Assert.That(stageCreateRes.Name, Is.EqualTo(stageCreateReq.Name));

            // Read all stages again and ensure the new stage is returned.
            stagesGetManyRes = await Client.GetAsync(stagesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(stagesGetManyRes.Any(x => x.StageID == stageCreateRes.StageID), Is.True);

            // Read the created stage.
            WorkOrderStageGETRequest stageGetReq = new WorkOrderStageGETRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                StageID = stageCreateRes.StageID
            };

            WorkOrderStage stageGetRes = await Client.GetAsync(stageGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(stageGetRes.StageID, Is.EqualTo(stageCreateRes.StageID));
            Assert.That(stageGetRes.Name, Is.EqualTo(stageCreateReq.Name));

            // Update the created stage.
            WorkOrderStagePATCHRequest stagePatchReq = new WorkOrderStagePATCHRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                StageID = stageCreateRes.StageID,
                Name = "Updated Stage " + RandomString(6)
            };

            WorkOrderStage stagePatchRes = await Client.PatchAsync(stagePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(stagePatchRes.StageID, Is.EqualTo(stagePatchReq.StageID));
            Assert.That(stagePatchRes.StageID, Is.EqualTo(stageCreateRes.StageID));
            Assert.That(stagePatchRes.Name, Is.EqualTo(stagePatchReq.Name));

            // Verify the stage was updated.
            stageGetRes = await Client.GetAsync(stageGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(stageGetRes.Name, Is.EqualTo(stagePatchReq.Name));

            // Delete the stage.
            WorkOrderStageDELETERequest stageDeleteReq = new WorkOrderStageDELETERequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                StageID = stageCreateRes.StageID
            };

            await Client.DeleteAsync(stageDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the stage was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(stageGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all stages again and ensure the deleted stage is no longer returned.
            stagesGetManyRes = await Client.GetAsync(stagesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(stagesGetManyRes.Any(x => x.StageID == stageCreateRes.StageID), Is.False);
        }
        #endregion
    }
}


