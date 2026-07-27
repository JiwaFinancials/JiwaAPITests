using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder;
using AllocationDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.Allocation;

namespace JiwaAPITests.WorkOrders
{
    public class Allocations : WorkOrderTestBase
    {
        #region "WorkOrders_Allocations"
        [Test]
        public async Task WorkOrders_Allocations_ReadDelete()
        {
            // Create a work order to use for allocation tests
            WorkOrderDto workOrderCreateRes = await CreateWorkOrderAsync();

            // Read all work order allocations
            WorkOrderAllocationsGETManyRequest allocationsGetManyReq = new WorkOrderAllocationsGETManyRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID
            };

            List<AllocationDto> allocationsGetManyRes = await Client.GetAsync(allocationsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // If there are existing allocations, verify we can read and delete them
            if (allocationsGetManyRes.Count > 0)
            {
                // Read an allocation
                AllocationDto firstAllocation = allocationsGetManyRes[0];

                WorkOrderAllocationGETRequest allocationGetReq = new WorkOrderAllocationGETRequest()
                {
                    WorkOrderID = workOrderCreateRes.WorkOrderID,
                    AllocationID = firstAllocation.AllocationID
                };

                AllocationDto allocationGetRes = await Client.GetAsync(allocationGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(allocationGetRes.AllocationID, Is.EqualTo(firstAllocation.AllocationID));
            }
        }
        #endregion
    }
}
