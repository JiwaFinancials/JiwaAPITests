using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager.Configuration;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.ServiceManager
{
    public class LabourLines : ServiceManagerTestBase
    {
        #region "{Main}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_LabourLines_CRUD()
        {
            // Create an inventory item for the labour line.
            InventoryPOSTRequest inventoryCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Service manager labour line item " + RandomString(5),
                PhysicalItem = false,
                DefaultPrice = 120.00M
            };

            InventoryItem inventoryCreateRes = await Client.PostAsync(inventoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inventoryCreateRes.InventoryID, Is.Not.Null);

            // Create a service manager job.
            Job jobCreateRes = await CreateJobAsync();

            // Create a service manager task.
            ServiceManagerTask taskCreateRes = await CreateTaskAsync(jobCreateRes.JobID);

            // Read all labour lines for the service manager task.
            ServiceManagerTaskLabourLineGETManyRequest labourLinesGetManyReq = new ServiceManagerTaskLabourLineGETManyRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID
            };

            List<LabourLine> labourLinesGetManyRes = await Client.GetAsync(labourLinesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(labourLinesGetManyRes, Is.Not.Null);

            // Select an activity to use for the labour line.
            ServiceManagerActivity activity = await GetAnyActivityAsync();

            // Append a labour line to the service manager task.
            ServiceManagerTaskLabourLinePOSTRequest labourLineCreateReq = new ServiceManagerTaskLabourLinePOSTRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                InventoryID = inventoryCreateRes.InventoryID,
                PartNo = inventoryCreateReq.PartNo,
                InventoryDescription = inventoryCreateReq.Description,
                Description = "Task labour line " + RandomString(6),
                StartTime = DateTime.Today,
                EndTime = DateTime.Today.AddHours(2),
                BillingTime = 1.50M,
                Rate = 120M,
                Activity = new ServiceManagerActivity()
                {
                    ActivityID = activity.ActivityID,
                    Name = activity.Name
                }
            };

            LabourLine labourLineCreateRes = await Client.PostAsync(labourLineCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(labourLineCreateRes.LabourLineID, Is.Not.Null);
            Assert.That(labourLineCreateRes.Description, Is.EqualTo(labourLineCreateReq.Description));

            // Read all labour lines again and ensure the created labour line is returned.
            labourLinesGetManyRes = await Client.GetAsync(labourLinesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(labourLinesGetManyRes.Any(x => x.LabourLineID == labourLineCreateRes.LabourLineID), Is.True);

            // Read the created labour line.
            ServiceManagerTaskLabourLineGETRequest labourLineGetReq = new ServiceManagerTaskLabourLineGETRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                LabourLineID = labourLineCreateRes.LabourLineID
            };

            LabourLine labourLineGetRes = await Client.GetAsync(labourLineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(labourLineGetRes.LabourLineID, Is.EqualTo(labourLineCreateRes.LabourLineID));

            // Update the created labour line.
            ServiceManagerTaskLabourLinePATCHRequest labourLinePatchReq = new ServiceManagerTaskLabourLinePATCHRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                LabourLineID = labourLineCreateRes.LabourLineID,
                Description = "Updated labour line " + RandomString(6),
                BillingTime = 2.25M,
                Rate = 135M
            };

            LabourLine labourLinePatchRes = await Client.PatchAsync(labourLinePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(labourLinePatchRes.LabourLineID, Is.EqualTo(labourLinePatchReq.LabourLineID));
            Assert.That(labourLinePatchRes.LabourLineID, Is.EqualTo(labourLineCreateRes.LabourLineID));
            Assert.That(labourLinePatchRes.Description, Is.EqualTo(labourLinePatchReq.Description));

            // Verify the labour line was updated.
            labourLineGetRes = await Client.GetAsync(labourLineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(labourLineGetRes.Description, Is.EqualTo(labourLinePatchReq.Description));

            // Delete the created labour line.
            ServiceManagerTaskLabourLineDELETERequest labourLineDeleteReq = new ServiceManagerTaskLabourLineDELETERequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                LabourLineID = labourLineCreateRes.LabourLineID
            };

            await Client.DeleteAsync(labourLineDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the labour line was deleted.
            WebServiceException labourLineDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(labourLineGetReq);
            });
            Assert.That(labourLineDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all labour lines again and ensure the deleted labour line is no longer returned.
            labourLinesGetManyRes = await Client.GetAsync(labourLinesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(labourLinesGetManyRes.Any(x => x.LabourLineID == labourLineCreateRes.LabourLineID), Is.False);

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
        public async Task ServiceManager_Jobs_Tasks_LabourLines_LineDetails_CRUD()
        {
            // Create an inventory item for the labour line.
            InventoryPOSTRequest inventoryCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Service manager labour line detail item " + RandomString(5),
                PhysicalItem = false,
                DefaultPrice = 120.00M
            };

            InventoryItem inventoryCreateRes = await Client.PostAsync(inventoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inventoryCreateRes.InventoryID, Is.Not.Null);

            // Create a service manager job.
            Job jobCreateRes = await CreateJobAsync();

            // Create a service manager task.
            ServiceManagerTask taskCreateRes = await CreateTaskAsync(jobCreateRes.JobID);

            // Create a debtor for the prepaid labour pack.
            JiwaFinancials.Jiwa.JiwaServiceModel.Debtors.Debtor debtorCreateRes = await Client.PostAsync(new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Service manager labour line detail debtor " + RandomString(5)
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, Is.Not.Null);

            // Create a prepaid labour pack for labour line details.
            JiwaFinancials.Jiwa.JiwaServiceModel.PrepaidLabour.PrepaidLabourPack prepaidLabourPackCreateRes = await Client.PostAsync(new PrepaidLabourPackPOSTRequest()
            {
                PackNo = "PL-" + RandomString(6),
                Name = "Service manager labour line detail pack " + RandomString(5),
                Description = "Service manager labour line detail pack",
                DebtorID = debtorCreateRes.DebtorID,
                AccountNo = debtorCreateRes.AccountNo,
                TotalHours = 20M,
                ReorderLevel = 5M,
                Rate = 120M,
                Ratio = 1M,
                SpecialUse = false
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(prepaidLabourPackCreateRes.PackID, Is.Not.Null);

            // Select an activity to use for the labour line.
            ServiceManagerActivity activity = await GetAnyActivityAsync();

            // Create a labour line.
            ServiceManagerTaskLabourLinePOSTRequest labourLineCreateReq = new ServiceManagerTaskLabourLinePOSTRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                InventoryID = inventoryCreateRes.InventoryID,
                PartNo = inventoryCreateReq.PartNo,
                InventoryDescription = inventoryCreateReq.Description,
                Description = "Task labour line " + RandomString(6),
                StartTime = DateTime.Today,
                EndTime = DateTime.Today.AddHours(2),
                BillingTime = 1.50M,
                Rate = 120M,
                IsPrepaid = false,
                Activity = new ServiceManagerActivity()
                {
                    ActivityID = activity.ActivityID,
                    Name = activity.Name
                }
            };

            LabourLine labourLineCreateRes = await Client.PostAsync(labourLineCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(labourLineCreateRes.LabourLineID, Is.Not.Null);

            // Read all line details for the labour line.
            ServiceManagerTaskLabourLineLineDetailsGETManyRequest lineDetailsGetManyReq = new ServiceManagerTaskLabourLineLineDetailsGETManyRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                LabourLineID = labourLineCreateRes.LabourLineID
            };

            List<LabourLineDetail> lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes, Is.Not.Null);

            // Append a line detail to the labour line.
            ServiceManagerTaskLabourLineLineDetailPOSTRequest lineDetailCreateReq = new ServiceManagerTaskLabourLineLineDetailPOSTRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                LabourLineID = labourLineCreateRes.LabourLineID,
                Quantity = 1M,
                PrepaidLabourPack = new ServiceManagerPrepaidLabourPack()
                {
                    PrepaidLabourPackID = prepaidLabourPackCreateRes.PackID,
                    Name = prepaidLabourPackCreateRes.Name,
                    Rate = prepaidLabourPackCreateRes.Rate ?? 0M
                }
            };

            LabourLineDetail lineDetailCreateRes = await Client.PostAsync(lineDetailCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(lineDetailCreateRes.LabourLineDetailID, Is.Not.Null);

            // Read the created line detail.
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            LabourLineDetail persistedLineDetail = lineDetailsGetManyRes.First(x => x.LabourLineDetailID == lineDetailCreateRes.LabourLineDetailID);

            ServiceManagerTaskLabourLineLineDetailGETRequest lineDetailGetReq = new ServiceManagerTaskLabourLineLineDetailGETRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                LabourLineID = labourLineCreateRes.LabourLineID,
                LabourLineDetailID = persistedLineDetail.LabourLineDetailID
            };

            LabourLineDetail lineDetailGetRes = await Client.GetAsync(lineDetailGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailGetRes.LabourLineDetailID, Is.EqualTo(persistedLineDetail.LabourLineDetailID));

            // Update the created line detail.
            ServiceManagerTaskLabourLineLineDetailPATCHRequest lineDetailPatchReq = new ServiceManagerTaskLabourLineLineDetailPATCHRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                LabourLineID = labourLineCreateRes.LabourLineID,
                LabourLineDetailID = persistedLineDetail.LabourLineDetailID,
                Quantity = 2M,
                PrepaidLabourPack = new ServiceManagerPrepaidLabourPack()
                {
                    PrepaidLabourPackID = prepaidLabourPackCreateRes.PackID,
                    Name = prepaidLabourPackCreateRes.Name,
                    Rate = prepaidLabourPackCreateRes.Rate ?? 0M
                }
            };

            LabourLineDetail lineDetailPatchRes = await Client.PatchAsync(lineDetailPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailPatchRes.LabourLineDetailID, Is.EqualTo(lineDetailPatchReq.LabourLineDetailID));
            Assert.That(lineDetailPatchRes.LabourLineDetailID, Is.EqualTo(persistedLineDetail.LabourLineDetailID));
            Assert.That(lineDetailPatchRes.Quantity, Is.EqualTo(lineDetailPatchReq.Quantity));

            // Replace line details for the labour line.
            LabourLineDetail lineDetailPutItem = new LabourLineDetail()
            {
                Quantity = 3M,
                PrepaidLabourPack = new ServiceManagerPrepaidLabourPack()
                {
                    PrepaidLabourPackID = prepaidLabourPackCreateRes.PackID,
                    Name = prepaidLabourPackCreateRes.Name,
                    Rate = prepaidLabourPackCreateRes.Rate ?? 0M
                }
            };

            ServiceManagerTaskLabourLineLineDetailPUTRequest lineDetailsPutReq = new ServiceManagerTaskLabourLineLineDetailPUTRequest()
            {
                lineDetailPutItem
            };

            List<LabourLineDetail> lineDetailsPutRes = await Client.PutAsync<List<LabourLineDetail>>($"/ServiceManager/Jobs/{jobCreateRes.JobID}/Tasks/{taskCreateRes.TaskID}/LabourLines/{labourLineCreateRes.LabourLineID}/LineDetails", lineDetailsPutReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsPutRes.Count, Is.EqualTo(1));
            Assert.That(lineDetailsPutRes[0].Quantity, Is.EqualTo(lineDetailsPutReq[0].Quantity));

            // Read all line details again and ensure only the replacement detail remains.
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes.Count, Is.EqualTo(1));

            string lineDetailIDToDelete = lineDetailsGetManyRes[0].LabourLineDetailID;

            // Delete the labour line detail.
            ServiceManagerTaskLabourLineLineDetailDELETERequest lineDetailDeleteReq = new ServiceManagerTaskLabourLineLineDetailDELETERequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                LabourLineID = labourLineCreateRes.LabourLineID,
                LabourLineDetailID = lineDetailIDToDelete
            };

            await Client.DeleteAsync(lineDetailDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the line detail was deleted.
            WebServiceException lineDetailDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(new ServiceManagerTaskLabourLineLineDetailGETRequest()
                {
                    JobID = jobCreateRes.JobID,
                    TaskID = taskCreateRes.TaskID,
                    LabourLineID = labourLineCreateRes.LabourLineID,
                    LabourLineDetailID = lineDetailIDToDelete
                });
            });
            Assert.That(lineDetailDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all line details again and ensure the deleted detail is no longer returned.
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes.Any(x => x.LabourLineDetailID == lineDetailIDToDelete), Is.False);

            // Clean up the created service manager task.
            await Client.DeleteAsync(new ServiceManagerTasksDELETERequest() { JobID = jobCreateRes.JobID, TaskID = taskCreateRes.TaskID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Clean up the created service manager job.
            await Client.DeleteAsync(new ServiceManagerJobDELETERequest() { JobID = jobCreateRes.JobID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Clean up the created prepaid labour pack.
            await Client.DeleteAsync(new PrepaidLabourPackDELETERequest()
            {
                PackID = prepaidLabourPackCreateRes.PackID
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}


