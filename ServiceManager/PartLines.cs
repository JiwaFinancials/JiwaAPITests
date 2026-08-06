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
    public class PartLines : ServiceManagerTestBase
    {
        #region "{Main}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_PartLines_CRUD()
        {
            // Create an inventory item for the part line.
            InventoryPOSTRequest inventoryCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Service manager part line item " + RandomString(5),
                DefaultPrice = 15.00M
            };

            InventoryItem inventoryCreateRes = await Client.PostAsync(inventoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inventoryCreateRes.InventoryID, Is.Not.Null);

            // Create a service manager job.
            Job jobCreateRes = await CreateJobAsync();

            // Create a service manager task.
            ServiceManagerTask taskCreateRes = await CreateTaskAsync(jobCreateRes.JobID);

            // Read all part lines for the service manager task.
            ServiceManagerTaskPartLineGETManyRequest partLinesGetManyReq = new ServiceManagerTaskPartLineGETManyRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID
            };

            List<PartLine> partLinesGetManyRes = await Client.GetAsync(partLinesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(partLinesGetManyRes, Is.Not.Null);

            // Append a part line to the service manager task.
            ServiceManagerTaskPartLinePOSTRequest partLineCreateReq = new ServiceManagerTaskPartLinePOSTRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                InventoryID = inventoryCreateRes.InventoryID,
                PartNo = inventoryCreateReq.PartNo,
                Description = "Task part line " + RandomString(6),
                Quantity = 1M,
                ItemPrice = 15M
            };

            PartLine partLineCreateRes = await Client.PostAsync(partLineCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(partLineCreateRes.PartLineID, Is.Not.Null);
            Assert.That(partLineCreateRes.PartNo, Is.EqualTo(partLineCreateReq.PartNo));

            // Read all part lines again and ensure the created part line is returned.
            partLinesGetManyRes = await Client.GetAsync(partLinesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(partLinesGetManyRes.Any(x => x.PartLineID == partLineCreateRes.PartLineID), Is.True);

            // Read the created part line.
            ServiceManagerTaskPartLineGETRequest partLineGetReq = new ServiceManagerTaskPartLineGETRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                PartLineID = partLineCreateRes.PartLineID
            };

            PartLine partLineGetRes = await Client.GetAsync(partLineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(partLineGetRes.PartLineID, Is.EqualTo(partLineCreateRes.PartLineID));

            // Update the created part line.
            ServiceManagerTaskPartLinePATCHRequest partLinePatchReq = new ServiceManagerTaskPartLinePATCHRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                PartLineID = partLineCreateRes.PartLineID,
                UserDefinedString1 = "UpdatedPartLine-" + RandomString(6),
                Quantity = 2M,
                ItemPrice = 18M
            };

            PartLine partLinePatchRes = await Client.PatchAsync(partLinePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(partLinePatchRes.PartLineID, Is.EqualTo(partLinePatchReq.PartLineID));
            Assert.That(partLinePatchRes.PartLineID, Is.EqualTo(partLineCreateRes.PartLineID));
            Assert.That(partLinePatchRes.UserDefinedString1, Is.EqualTo(partLinePatchReq.UserDefinedString1));

            // Verify the part line was updated.
            partLineGetRes = await Client.GetAsync(partLineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(partLineGetRes.UserDefinedString1, Is.EqualTo(partLinePatchReq.UserDefinedString1));

            // Delete the created part line.
            ServiceManagerTaskPartLineDELETERequest partLineDeleteReq = new ServiceManagerTaskPartLineDELETERequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                PartLineID = partLineCreateRes.PartLineID
            };

            await Client.DeleteAsync(partLineDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the part line was deleted.
            WebServiceException partLineDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(partLineGetReq);
            });
            Assert.That(partLineDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all part lines again and ensure the deleted part line is no longer returned.
            partLinesGetManyRes = await Client.GetAsync(partLinesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(partLinesGetManyRes.Any(x => x.PartLineID == partLineCreateRes.PartLineID), Is.False);

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
        public async Task ServiceManager_Jobs_Tasks_PartLines_LineDetails_CRUD()
        {
            // Create an inventory item for the part line.
            InventoryPOSTRequest inventoryCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Service manager part line detail item " + RandomString(5),
                DefaultPrice = 15.00M
            };

            InventoryItem inventoryCreateRes = await Client.PostAsync(inventoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inventoryCreateRes.InventoryID, Is.Not.Null);

            // Seed stock so part line details can transact against inventory on hand.
            await EnsureStockOnHandAsync(inventoryCreateRes.PartNo, 5M);

            // Create a service manager job.
            Job jobCreateRes = await CreateJobAsync();

            // Create a service manager task.
            ServiceManagerTask taskCreateRes = await CreateTaskAsync(jobCreateRes.JobID);

            // Create a part line.
            ServiceManagerTaskPartLinePOSTRequest partLineCreateReq = new ServiceManagerTaskPartLinePOSTRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                InventoryID = inventoryCreateRes.InventoryID,
                PartNo = inventoryCreateReq.PartNo,
                Description = "Task part line " + RandomString(6),
                Quantity = 1M,
                ItemPrice = 15M
            };

            PartLine partLineCreateRes = await Client.PostAsync(partLineCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(partLineCreateRes.PartLineID, Is.Not.Null);

            // Read all line details for the part line.
            ServiceManagerTaskPartLineLineDetailsGETManyRequest lineDetailsGetManyReq = new ServiceManagerTaskPartLineLineDetailsGETManyRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                PartLineID = partLineCreateRes.PartLineID
            };

            List<PartLineDetail> lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes, Is.Not.Null);

            // Append a line detail to the part line.
            ServiceManagerTaskPartLineLineDetailPOSTRequest lineDetailCreateReq = new ServiceManagerTaskPartLineLineDetailPOSTRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                PartLineID = partLineCreateRes.PartLineID,
                Quantity = 1M,
                Cost = 15M
            };

            PartLineDetail lineDetailCreateRes = await Client.PostAsync(lineDetailCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(lineDetailCreateRes.PartLineDetailID, Is.Not.Null);

            // Read the created line detail.
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            PartLineDetail persistedLineDetail = lineDetailsGetManyRes.First(x => x.PartLineDetailID == lineDetailCreateRes.PartLineDetailID);

            ServiceManagerTaskPartLineLineDetailGETRequest lineDetailGetReq = new ServiceManagerTaskPartLineLineDetailGETRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                PartLineID = partLineCreateRes.PartLineID,
                PartLineDetailID = persistedLineDetail.PartLineDetailID
            };

            PartLineDetail lineDetailGetRes = await Client.GetAsync(lineDetailGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailGetRes.PartLineDetailID, Is.EqualTo(persistedLineDetail.PartLineDetailID));

            // Update the created line detail.
            ServiceManagerTaskPartLineLineDetailPATCHRequest lineDetailPatchReq = new ServiceManagerTaskPartLineLineDetailPATCHRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                PartLineID = partLineCreateRes.PartLineID,
                PartLineDetailID = persistedLineDetail.PartLineDetailID,
                Quantity = 1M,
                Cost = 18M
            };

            PartLineDetail lineDetailPatchRes = await Client.PatchAsync(lineDetailPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailPatchRes.PartLineDetailID, Is.EqualTo(lineDetailPatchReq.PartLineDetailID));
            Assert.That(lineDetailPatchRes.PartLineDetailID, Is.EqualTo(persistedLineDetail.PartLineDetailID));
            Assert.That(lineDetailPatchRes.Cost, Is.EqualTo(lineDetailPatchReq.Cost));

            // Replace line details for the part line.
            PartLineDetail lineDetailPutItem = new PartLineDetail()
            {
                Quantity = 1M,
                Cost = 19M,
                DateIn = lineDetailPatchRes.DateIn,
                ExpiryDate = lineDetailPatchRes.ExpiryDate
            };

            ServiceManagerTaskPartLineLineDetailPUTRequest lineDetailsPutReq = new ServiceManagerTaskPartLineLineDetailPUTRequest()
            {
                lineDetailPutItem
            };

            List<PartLineDetail> lineDetailsPutRes = await Client.PutAsync<List<PartLineDetail>>($"/ServiceManager/Jobs/{jobCreateRes.JobID}/Tasks/{taskCreateRes.TaskID}/PartLines/{partLineCreateRes.PartLineID}/LineDetails", lineDetailsPutReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsPutRes.Count, Is.EqualTo(1));
            Assert.That(lineDetailsPutRes[0].Cost, Is.EqualTo(lineDetailsPutReq[0].Cost));

            // Read all line details again and ensure only the replacement detail remains.
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes.Count, Is.EqualTo(1));

            string lineDetailIDToDelete = lineDetailsGetManyRes[0].PartLineDetailID;

            // Delete the part line detail.
            ServiceManagerTaskPartLineLineDetailDELETERequest lineDetailDeleteReq = new ServiceManagerTaskPartLineLineDetailDELETERequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                PartLineID = partLineCreateRes.PartLineID,
                PartLineDetailID = lineDetailIDToDelete
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
            Assert.That(lineDetailsGetManyRes.Any(x => x.PartLineDetailID == lineDetailIDToDelete), Is.False);

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


