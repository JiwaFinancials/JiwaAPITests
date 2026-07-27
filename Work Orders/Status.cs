using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkOrderStatusDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrderStatus;

namespace JiwaAPITests.WorkOrders
{
    public class Status : JiwaAPITest
    {
        #region "WorkOrders_Statuses"
        [Test]
        public async Task WorkOrders_Statuses_GET()
        {
            // Read all work order statuses.
            WorkOrderStatusesGETManyRequest statusesGetManyReq = new WorkOrderStatusesGETManyRequest();
            List<WorkOrderStatusDto> statusesGetManyRes = await Client.GetAsync(statusesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(statusesGetManyRes, Is.Not.Null);
            Assert.That(statusesGetManyRes.Count, Is.GreaterThan(0));
        }
        
        [Test]
        public async Task WorkOrders_Status_GET()
        {
            // Read all work order statuses to obtain a valid StatusID.
            WorkOrderStatusesGETManyRequest statusesGetManyReq = new WorkOrderStatusesGETManyRequest();
            List<WorkOrderStatusDto> statusesGetManyRes = await Client.GetAsync(statusesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(statusesGetManyRes, Is.Not.Null);
            Assert.That(statusesGetManyRes.Count, Is.GreaterThan(0));
            WorkOrderStatusDto firstStatus = statusesGetManyRes.First();

            // Read a work order status by StatusID.
            WorkOrderStatusGETRequest statusGetReq = new WorkOrderStatusGETRequest()
            {
                StatusID = firstStatus.StatusID
            };

            WorkOrderStatusDto statusGetRes = await Client.GetAsync(statusGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(statusGetRes.StatusID, Is.EqualTo(firstStatus.StatusID));
            Assert.That(statusGetRes.Name, Is.EqualTo(firstStatus.Name));
        }
        #endregion
    }
}
