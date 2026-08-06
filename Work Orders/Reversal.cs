using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders;
using NUnit.Framework;
using System.Threading.Tasks;
using WorkOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder;
using WorkOrderStatusDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrderStatus;

namespace JiwaAPITests.WorkOrders
{
    public class Reversal : WorkOrderTestBase
    {
        #region "WorkOrders_Reversal"
        [Test]
        public async Task WorkOrders_Reversal_CRUD()
        {
            // Create a work order to reverse.
            WorkOrderDto workOrderCreateRes = await CreateWorkOrderAsync();

            // Start the work order.
            WorkOrderPATCHRequest workOrderPatchReq = new WorkOrderPATCHRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                Status = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrderStatuses.Started
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder workOrderPatchRes = await Client.PatchAsync(workOrderPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(workOrderPatchRes.WorkOrderID, Is.EqualTo(workOrderPatchReq.WorkOrderID));
            Assert.That(workOrderPatchRes.WorkOrderID, Is.EqualTo(workOrderCreateRes.WorkOrderID));
            Assert.That(workOrderPatchRes.Status, Is.EqualTo(JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrderStatuses.Started));

            // Close the work order.
            workOrderPatchReq = new WorkOrderPATCHRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                Status = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrderStatuses.Closed
            };

            workOrderPatchRes = await Client.PatchAsync(workOrderPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(workOrderPatchRes.WorkOrderID, Is.EqualTo(workOrderPatchReq.WorkOrderID));
            Assert.That(workOrderPatchRes.WorkOrderID, Is.EqualTo(workOrderCreateRes.WorkOrderID));
            Assert.That(workOrderPatchRes.Status, Is.EqualTo(JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrderStatuses.Closed));

            // Create a reversal work order.
            WorkOrderReversalPOSTRequest reversalCreateReq = new WorkOrderReversalPOSTRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID
            };

            WorkOrderDto reversalCreateRes = await Client.PostAsync(reversalCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(reversalCreateRes.WorkOrderID, Is.Not.Null);
            Assert.That(reversalCreateRes.ReversalWorkOrderID, Is.EqualTo(workOrderCreateRes.WorkOrderID));
        }
        #endregion
    }
}


