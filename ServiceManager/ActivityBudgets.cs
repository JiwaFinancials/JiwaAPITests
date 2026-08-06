using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager.Configuration;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.ServiceManager
{
    public class ActivityBudgets : ServiceManagerTestBase
    {
        #region "{Main}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_ActivityBudgets_CRUD()
        {
            // Create a service manager job.
            Job jobCreateRes = await CreateJobAsync();

            // Create a service manager task.
            ServiceManagerTask taskCreateRes = await CreateTaskAsync(jobCreateRes.JobID);

            // Read all activity budgets for the service manager task.
            ServiceManagerActivityBudgetsGETManyRequest budgetsGetManyReq = new ServiceManagerActivityBudgetsGETManyRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID
            };

            List<ActivityBudget> budgetsGetManyRes = await Client.GetAsync(budgetsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(budgetsGetManyRes, Is.Not.Null);

            // Select an activity to use for the activity budget.
            ServiceManagerActivity activity = await GetAnyActivityAsync();

            // Append an activity budget to the service manager task.
            ServiceManagerActivityBudgetsPOSTRequest budgetCreateReq = new ServiceManagerActivityBudgetsPOSTRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                Activity = new ServiceManagerActivity()
                {
                    ActivityID = activity.ActivityID,
                    Name = activity.Name
                },
                BudgetedBillingTime = 1.25M,
                BudgetedElapsedTime = 1.50M,
                BudgetedBillingValue = 150M
            };

            ActivityBudget budgetCreateRes = await Client.PostAsync(budgetCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(budgetCreateRes.ActivityBudgetID, Is.Not.Null);

            // Read all activity budgets again and ensure the created budget is returned.
            budgetsGetManyRes = await Client.GetAsync(budgetsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(budgetsGetManyRes.Any(x => x.ActivityBudgetID == budgetCreateRes.ActivityBudgetID), Is.True);

            // Read the created activity budget.
            ServiceManagerActivityBudgetsGETRequest budgetGetReq = new ServiceManagerActivityBudgetsGETRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                ActivityBudgetID = budgetCreateRes.ActivityBudgetID
            };

            ActivityBudget budgetGetRes = await Client.GetAsync(budgetGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(budgetGetRes.ActivityBudgetID, Is.EqualTo(budgetCreateRes.ActivityBudgetID));

            // Update the created activity budget.
            ServiceManagerActivityBudgetsPATCHRequest budgetPatchReq = new ServiceManagerActivityBudgetsPATCHRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                ActivityBudgetID = budgetCreateRes.ActivityBudgetID,
                BudgetedBillingTime = 2.00M,
                BudgetedElapsedTime = 2.50M,
                BudgetedBillingValue = 240M
            };

            ActivityBudget budgetPatchRes = await Client.PatchAsync(budgetPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(budgetPatchRes.ActivityBudgetID, Is.EqualTo(budgetPatchReq.ActivityBudgetID));
            Assert.That(budgetPatchRes.ActivityBudgetID, Is.EqualTo(budgetCreateRes.ActivityBudgetID));
            Assert.That(budgetPatchRes.BudgetedBillingTime, Is.EqualTo(budgetPatchReq.BudgetedBillingTime));
            Assert.That(budgetPatchRes.BudgetedBillingValue, Is.EqualTo(budgetPatchReq.BudgetedBillingValue));

            // Verify the activity budget was updated.
            budgetGetRes = await Client.GetAsync(budgetGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(budgetGetRes.BudgetedBillingTime, Is.EqualTo(budgetPatchReq.BudgetedBillingTime));
            Assert.That(budgetGetRes.BudgetedBillingValue, Is.EqualTo(budgetPatchReq.BudgetedBillingValue));

            // Delete the created activity budget.
            ServiceManagerActivityBudgetsDELETERequest budgetDeleteReq = new ServiceManagerActivityBudgetsDELETERequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                ActivityBudgetID = budgetCreateRes.ActivityBudgetID
            };

            await Client.DeleteAsync(budgetDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the activity budget was deleted.
            WebServiceException budgetDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(budgetGetReq);
            });
            Assert.That(budgetDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all activity budgets again and ensure the deleted budget is no longer returned.
            budgetsGetManyRes = await Client.GetAsync(budgetsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(budgetsGetManyRes.Any(x => x.ActivityBudgetID == budgetCreateRes.ActivityBudgetID), Is.False);

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


