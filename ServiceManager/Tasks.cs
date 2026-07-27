using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.ServiceManager
{
    public class Tasks : ServiceManagerTestBase
    {
        #region "{Main}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_CRUD()
        {
            // Create a service manager job.
            Job jobCreateRes = await CreateJobAsync();

            // Read all tasks for the service manager job.
            ServiceManagerTasksGETManyRequest tasksGetManyReq = new ServiceManagerTasksGETManyRequest()
            {
                JobID = jobCreateRes.JobID
            };

            List<ServiceManagerTask> tasksGetManyRes = await Client.GetAsync(tasksGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tasksGetManyRes, Is.Not.Null);

            // Append a new task to the service manager job.
            ServiceManagerTask taskCreateRes = await CreateTaskAsync(jobCreateRes.JobID);

            // Read all tasks again and ensure the created task is returned.
            tasksGetManyRes = await Client.GetAsync(tasksGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tasksGetManyRes.Any(x => x.TaskID == taskCreateRes.TaskID), Is.True);

            // Read the created task.
            ServiceManagerTasksGETRequest taskGetReq = new ServiceManagerTasksGETRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID
            };

            ServiceManagerTask taskGetRes = await Client.GetAsync(taskGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(taskGetRes.TaskID, Is.EqualTo(taskCreateRes.TaskID));

            // Update the created task.
            ServiceManagerTasksPATCHRequest taskPatchReq = new ServiceManagerTasksPATCHRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                Description = "Updated task " + RandomString(6)
            };

            ServiceManagerTask taskPatchRes = await Client.PatchAsync(taskPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(taskPatchRes.TaskID, Is.EqualTo(taskCreateRes.TaskID));
            Assert.That(taskPatchRes.Description, Is.EqualTo(taskPatchReq.Description));

            // Verify the task was updated.
            taskGetRes = await Client.GetAsync(taskGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(taskGetRes.Description, Is.EqualTo(taskPatchReq.Description));

            // Delete the created task.
            ServiceManagerTasksDELETERequest taskDeleteReq = new ServiceManagerTasksDELETERequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID
            };

            await Client.DeleteAsync(taskDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the task was deleted.
            WebServiceException taskDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(taskGetReq);
            });
            Assert.That(taskDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all tasks again and ensure the deleted task is no longer returned.
            tasksGetManyRes = await Client.GetAsync(tasksGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tasksGetManyRes.Any(x => x.TaskID == taskCreateRes.TaskID), Is.False);

            // Clean up the created service manager job.
            await Client.DeleteAsync(new ServiceManagerJobDELETERequest() { JobID = jobCreateRes.JobID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion

        #region "{Actions}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_Retire_Unretire_NotFound()
        {
            string missingJobID = "missing-job-" + RandomString(8);
            string missingTaskID = "missing-task-" + RandomString(8);

            // Attempt to retire a missing service manager task.
            ServiceManagerTasksRetirePOSTRequest retireReq = new ServiceManagerTasksRetirePOSTRequest()
            {
                JobID = missingJobID,
                TaskID = missingTaskID
            };

            WebServiceException retireEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.PostAsync(retireReq);
            });
            Assert.That(retireEx.StatusCode, Is.EqualTo(404));

            // Attempt to unretire a missing service manager task.
            ServiceManagerTasksUnretirePOSTRequest unretireReq = new ServiceManagerTasksUnretirePOSTRequest()
            {
                JobID = missingJobID,
                TaskID = missingTaskID
            };

            WebServiceException unretireEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.PostAsync(unretireReq);
            });
            Assert.That(unretireEx.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task ServiceManager_Jobs_Tasks_ProcessCreditNote_ProcessSalesOrder_NotFound()
        {
            string missingJobID = "missing-job-" + RandomString(8);
            string missingTaskID = "missing-task-" + RandomString(8);

            // Attempt to process a credit note for a missing service manager task.
            ServiceManagerTasksMakeCreditNotePOSTRequest makeCreditNoteReq = new ServiceManagerTasksMakeCreditNotePOSTRequest()
            {
                JobID = missingJobID,
                TaskID = missingTaskID
            };

            WebServiceException makeCreditNoteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.PostAsync(makeCreditNoteReq);
            });
            Assert.That(makeCreditNoteEx.StatusCode, Is.EqualTo(404));

            // Attempt to process a sales order for a missing service manager task.
            ServiceManagerTasksMakeSalesOrderPOSTRequest makeSalesOrderReq = new ServiceManagerTasksMakeSalesOrderPOSTRequest()
            {
                JobID = missingJobID,
                TaskID = missingTaskID
            };

            WebServiceException makeSalesOrderEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.PostAsync(makeSalesOrderReq);
            });
            Assert.That(makeSalesOrderEx.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}
