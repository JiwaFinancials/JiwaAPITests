using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;

namespace JiwaAPITests.WorkOrders
{
    public class WorkOrder : WorkOrderTestBase
    {
        #region "{Main}"
        [Test]
        public async Task WorkOrder_CRUD()
        {
            // Create a work order.
            JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder workOrderCreateRes = await CreateWorkOrderAsync();

            // Read the created work order.
            WorkOrderGETRequest workOrderGetReq = new WorkOrderGETRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder workOrderGetRes = await Client.GetAsync(workOrderGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(workOrderGetRes.WorkOrderID, Is.EqualTo(workOrderCreateRes.WorkOrderID));
            Assert.That(workOrderGetRes.BillID, Is.EqualTo(workOrderCreateRes.BillID));
            Assert.That(workOrderGetRes.Stages.Count, Is.EqualTo(workOrderCreateRes.Stages.Count));
            Assert.That(workOrderGetRes.Outputs.Count, Is.EqualTo(workOrderCreateRes.Outputs.Count));

            // Update the work order.
            WorkOrderPATCHRequest workOrderPatchReq = new WorkOrderPATCHRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                Reference = "Updated work order reference " + RandomString(6),
                DateRequired = DateTime.Today.AddDays(14),
                ProductionQuantity = 7M
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder workOrderPatchRes = await Client.PatchAsync(workOrderPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(workOrderPatchRes.WorkOrderID, Is.EqualTo(workOrderPatchReq.WorkOrderID));
            Assert.That(workOrderPatchRes.WorkOrderID, Is.EqualTo(workOrderCreateRes.WorkOrderID));
            Assert.That(workOrderPatchRes.Reference, Is.EqualTo(workOrderPatchReq.Reference));
            Assert.That(workOrderPatchRes.DateRequired, Is.EqualTo(workOrderPatchReq.DateRequired));
            Assert.That(workOrderPatchRes.ProductionQuantity, Is.EqualTo(workOrderPatchReq.ProductionQuantity));

            // Verify the work order was updated.
            workOrderGetRes = await Client.GetAsync(workOrderGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(workOrderGetRes.Reference, Is.EqualTo(workOrderPatchReq.Reference));
            Assert.That(workOrderGetRes.DateRequired, Is.EqualTo(workOrderPatchReq.DateRequired));
            Assert.That(workOrderGetRes.ProductionQuantity, Is.EqualTo(workOrderPatchReq.ProductionQuantity));

            // Delete the work order.
            WorkOrderDELETERequest workOrderDeleteReq = new WorkOrderDELETERequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID
            };

            await Client.DeleteAsync(workOrderDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the work order was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(workOrderGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}

