using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.ServiceManager
{
    public class CustomerReturns : ServiceManagerTestBase
    {
        #region "{Main}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_CustomerReturns_CRUD()
        {
            // Create an inventory item for the customer return line.
            InventoryPOSTRequest inventoryCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Service manager return item " + RandomString(5),
                DefaultPrice = 10.00M
            };

            InventoryItem inventoryCreateRes = await Client.PostAsync(inventoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inventoryCreateRes.InventoryID, Is.Not.Null);

            // Create a service manager job.
            Job jobCreateRes = await CreateJobAsync();

            // Create a service manager task.
            ServiceManagerTask taskCreateRes = await CreateTaskAsync(jobCreateRes.JobID);

            // Read all customer returns for the service manager task.
            ServiceManagerTaskCustomerReturnGETManyRequest returnsGetManyReq = new ServiceManagerTaskCustomerReturnGETManyRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID
            };

            List<CustomerReturn> returnsGetManyRes = await Client.GetAsync(returnsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(returnsGetManyRes, Is.Not.Null);

            // Append a customer return to the service manager task.
            ServiceManagerTaskCustomerReturnPOSTRequest returnCreateReq = new ServiceManagerTaskCustomerReturnPOSTRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                InventoryID = inventoryCreateRes.InventoryID,
                PartNo = inventoryCreateReq.PartNo,
                Description = "Task return " + RandomString(6),
                Notes = "Initial return notes " + RandomString(4),
                Quantity = 1M,
                Cost = 10M
            };

            CustomerReturn returnCreateRes = await Client.PostAsync(returnCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(returnCreateRes.CustomerReturnID, Is.Not.Null);
            Assert.That(returnCreateRes.PartNo, Is.EqualTo(returnCreateReq.PartNo));

            // Read all customer returns again and ensure the created return is returned.
            returnsGetManyRes = await Client.GetAsync(returnsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(returnsGetManyRes.Any(x => x.CustomerReturnID == returnCreateRes.CustomerReturnID), Is.True);

            // Read the created customer return.
            ServiceManagerTaskCustomerReturnGETRequest returnGetReq = new ServiceManagerTaskCustomerReturnGETRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                CustomerReturnID = returnCreateRes.CustomerReturnID
            };

            CustomerReturn returnGetRes = await Client.GetAsync(returnGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(returnGetRes.CustomerReturnID, Is.EqualTo(returnCreateRes.CustomerReturnID));

            // Update the created customer return.
            ServiceManagerTaskCustomerReturnPATCHRequest returnPatchReq = new ServiceManagerTaskCustomerReturnPATCHRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                CustomerReturnID = returnCreateRes.CustomerReturnID,
                Notes = "Updated return notes " + RandomString(6),
                Quantity = 2M,
                Cost = 12M
            };

            CustomerReturn returnPatchRes = await Client.PatchAsync(returnPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(returnPatchRes.CustomerReturnID, Is.EqualTo(returnCreateRes.CustomerReturnID));
            Assert.That(returnPatchRes.Notes, Is.EqualTo(returnPatchReq.Notes));

            // Verify the customer return was updated.
            returnGetRes = await Client.GetAsync(returnGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(returnGetRes.Notes, Is.EqualTo(returnPatchReq.Notes));

            // Delete the created customer return.
            ServiceManagerTaskCustomerReturnDELETERequest returnDeleteReq = new ServiceManagerTaskCustomerReturnDELETERequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                CustomerReturnID = returnCreateRes.CustomerReturnID
            };

            await Client.DeleteAsync(returnDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the customer return was deleted.
            WebServiceException returnDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(returnGetReq);
            });
            Assert.That(returnDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all customer returns again and ensure the deleted return is no longer returned.
            returnsGetManyRes = await Client.GetAsync(returnsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(returnsGetManyRes.Any(x => x.CustomerReturnID == returnCreateRes.CustomerReturnID), Is.False);

            // Clean up the created service manager task.
            await Client.DeleteAsync(new ServiceManagerTasksDELETERequest() { JobID = jobCreateRes.JobID, TaskID = taskCreateRes.TaskID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Clean up the created service manager job.
            await Client.DeleteAsync(new ServiceManagerJobDELETERequest() { JobID = jobCreateRes.JobID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion

        #region "{LineDetails}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_CustomerReturns_LineDetails_CRUD()
        {
            // Create an inventory item for the customer return.
            InventoryPOSTRequest inventoryCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Service manager return detail item " + RandomString(5),
                DefaultPrice = 10.00M
            };

            InventoryItem inventoryCreateRes = await Client.PostAsync(inventoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inventoryCreateRes.InventoryID, Is.Not.Null);

            // Seed stock so customer return line details can transact against inventory on hand.
            await EnsureStockOnHandAsync(inventoryCreateRes.PartNo, 5M);

            // Create a service manager job.
            Job jobCreateRes = await CreateJobAsync();

            // Create a service manager task.
            ServiceManagerTask taskCreateRes = await CreateTaskAsync(jobCreateRes.JobID);

            // Create a customer return.
            ServiceManagerTaskCustomerReturnPOSTRequest returnCreateReq = new ServiceManagerTaskCustomerReturnPOSTRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                InventoryID = inventoryCreateRes.InventoryID,
                PartNo = inventoryCreateReq.PartNo,
                Description = "Task return " + RandomString(6),
                Notes = "Line details return " + RandomString(4),
                Quantity = 1M,
                Cost = 10M
            };

            CustomerReturn returnCreateRes = await Client.PostAsync(returnCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(returnCreateRes.CustomerReturnID, Is.Not.Null);

            // Read all line details for the customer return.
            ServiceManagerTaskCustomerReturnLineDetailsGETManyRequest lineDetailsGetManyReq = new ServiceManagerTaskCustomerReturnLineDetailsGETManyRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                CustomerReturnID = returnCreateRes.CustomerReturnID
            };

            List<CustomerReturnLineDetail> lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes, Is.Not.Null);

            // Append a line detail to the customer return.
            ServiceManagerTaskCustomerReturnLineDetailPOSTRequest lineDetailCreateReq = new ServiceManagerTaskCustomerReturnLineDetailPOSTRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                CustomerReturnID = returnCreateRes.CustomerReturnID,
                Quantity = 1M,
                Cost = 10M
            };

            CustomerReturnLineDetail lineDetailCreateRes = await Client.PostAsync(lineDetailCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(lineDetailCreateRes.CustomerReturnLineDetailID, Is.Not.Null);

            // Read the created line detail.
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            CustomerReturnLineDetail persistedLineDetail = lineDetailsGetManyRes.First(x => x.CustomerReturnLineDetailID == lineDetailCreateRes.CustomerReturnLineDetailID);

            ServiceManagerTaskCustomerReturnLineDetailGETRequest lineDetailGetReq = new ServiceManagerTaskCustomerReturnLineDetailGETRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                CustomerReturnID = returnCreateRes.CustomerReturnID,
                CustomerReturnLineDetailID = persistedLineDetail.CustomerReturnLineDetailID
            };

            CustomerReturnLineDetail lineDetailGetRes = await Client.GetAsync(lineDetailGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailGetRes.CustomerReturnLineDetailID, Is.EqualTo(persistedLineDetail.CustomerReturnLineDetailID));

            // Update the created line detail.
            ServiceManagerTaskCustomerReturnLineDetailPATCHRequest lineDetailPatchReq = new ServiceManagerTaskCustomerReturnLineDetailPATCHRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                CustomerReturnID = returnCreateRes.CustomerReturnID,
                CustomerReturnLineDetailID = persistedLineDetail.CustomerReturnLineDetailID,
                Quantity = 1M,
                Cost = 12M
            };

            CustomerReturnLineDetail lineDetailPatchRes = await Client.PatchAsync(lineDetailPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailPatchRes.CustomerReturnLineDetailID, Is.EqualTo(persistedLineDetail.CustomerReturnLineDetailID));
            Assert.That(lineDetailPatchRes.Cost, Is.EqualTo(lineDetailPatchReq.Cost));

            // Replace line details for the customer return line.
            CustomerReturnLineDetail lineDetailPutItem = new CustomerReturnLineDetail()
            {
                Quantity = 1M,
                Cost = 13M
            };

            ServiceManagerTaskCustomerReturnLineDetailPUTRequest lineDetailsPutReq = new ServiceManagerTaskCustomerReturnLineDetailPUTRequest()
            {
                lineDetailPutItem
            };

            List<CustomerReturnLineDetail> lineDetailsPutRes = await Client.PutAsync<List<CustomerReturnLineDetail>>($"/ServiceManager/Jobs/{jobCreateRes.JobID}/Tasks/{taskCreateRes.TaskID}/CustomerReturns/{returnCreateRes.CustomerReturnID}/LineDetails", lineDetailsPutReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsPutRes.Count, Is.EqualTo(1));
            Assert.That(lineDetailsPutRes[0].Cost, Is.EqualTo(lineDetailsPutReq[0].Cost));

            // Read all line details again and ensure only the replacement detail remains.
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes.Count, Is.EqualTo(1));

            string lineDetailIDToDelete = lineDetailsGetManyRes[0].CustomerReturnLineDetailID;

            // Delete the customer return line detail.
            ServiceManagerTaskCustomerReturnLineDetailDELETERequest lineDetailDeleteReq = new ServiceManagerTaskCustomerReturnLineDetailDELETERequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                CustomerReturnID = returnCreateRes.CustomerReturnID,
                CustomerReturnLineDetailID = lineDetailIDToDelete
            };

            await Client.DeleteAsync(lineDetailDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the line detail was deleted.
            WebServiceException lineDetailDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(lineDetailGetReq);
            });
            Assert.That(lineDetailDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all line details again and ensure the deleted detail is no longer returned.
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes.Any(x => x.CustomerReturnLineDetailID == lineDetailIDToDelete), Is.False);

            // Clean up the created service manager task.
            await Client.DeleteAsync(new ServiceManagerTasksDELETERequest() { JobID = jobCreateRes.JobID, TaskID = taskCreateRes.TaskID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Clean up the created service manager job.
            await Client.DeleteAsync(new ServiceManagerJobDELETERequest() { JobID = jobCreateRes.JobID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

