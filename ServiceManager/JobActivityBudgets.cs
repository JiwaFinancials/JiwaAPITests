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
    public class JobActivityBudgets : ServiceManagerTestBase
    {
        #region "{Main}"
        [Test]
        public async Task ServiceManager_Jobs_JobActivityBudgets_CRUD()
        {
            // Create a service manager job.
            Job jobCreateRes = await CreateJobAsync();

            // Read all activity budgets for the service manager job.
            ServiceManagerJobActivityBudgetsGETManyRequest budgetsGetManyReq = new ServiceManagerJobActivityBudgetsGETManyRequest()
            {
                JobID = jobCreateRes.JobID
            };

            List<JobActivityBudget> budgetsGetManyRes = await Client.GetAsync(budgetsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(budgetsGetManyRes, Is.Not.Null);

            // Select an activity to use for the budget line.
            ServiceManagerActivity activity = await GetAnyActivityAsync();

            // Append a new activity budget to the service manager job.
            ServiceManagerJobActivityBudgetsPOSTRequest budgetCreateReq = new ServiceManagerJobActivityBudgetsPOSTRequest()
            {
                JobID = jobCreateRes.JobID,
                ActivityID = activity.ActivityID,
                ActivityName = activity.Name,
                BudgetedBillingTime = 1.25M,
                BudgetedElapsedTime = 1.50M,
                BudgetedBillingValue = 150M
            };

            JobActivityBudget budgetCreateRes = await Client.PostAsync(budgetCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(budgetCreateRes.JobActivityBudgetID, Is.Not.Null);

            // Read all activity budgets again and ensure the created budget is returned.
            budgetsGetManyRes = await Client.GetAsync(budgetsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(budgetsGetManyRes.Any(x => x.JobActivityBudgetID == budgetCreateRes.JobActivityBudgetID), Is.True);

            // Read the created activity budget.
            ServiceManagerJobActivityBudgetsGETRequest budgetGetReq = new ServiceManagerJobActivityBudgetsGETRequest()
            {
                JobID = jobCreateRes.JobID,
                JobActivityBudgetID = budgetCreateRes.JobActivityBudgetID
            };

            JobActivityBudget budgetGetRes = await Client.GetAsync(budgetGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(budgetGetRes.JobActivityBudgetID, Is.EqualTo(budgetCreateRes.JobActivityBudgetID));

            // Update the created activity budget.
            ServiceManagerJobActivityBudgetsPATCHRequest budgetPatchReq = new ServiceManagerJobActivityBudgetsPATCHRequest()
            {
                JobID = jobCreateRes.JobID,
                JobActivityBudgetID = budgetCreateRes.JobActivityBudgetID,
                BudgetedBillingTime = 2.00M,
                BudgetedElapsedTime = 2.50M,
                BudgetedBillingValue = 250M
            };

            JobActivityBudget budgetPatchRes = await Client.PatchAsync(budgetPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(budgetPatchRes.JobActivityBudgetID, Is.EqualTo(budgetPatchReq.JobActivityBudgetID));
            Assert.That(budgetPatchRes.JobActivityBudgetID, Is.EqualTo(budgetCreateRes.JobActivityBudgetID));
            Assert.That(budgetPatchRes.BudgetedBillingTime, Is.EqualTo(budgetPatchReq.BudgetedBillingTime));
            Assert.That(budgetPatchRes.BudgetedBillingValue, Is.EqualTo(budgetPatchReq.BudgetedBillingValue));

            // Verify the activity budget was updated.
            budgetGetRes = await Client.GetAsync(budgetGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(budgetGetRes.BudgetedBillingTime, Is.EqualTo(budgetPatchReq.BudgetedBillingTime));
            Assert.That(budgetGetRes.BudgetedBillingValue, Is.EqualTo(budgetPatchReq.BudgetedBillingValue));

            // Delete the created activity budget.
            ServiceManagerJobActivityBudgetsDELETERequest budgetDeleteReq = new ServiceManagerJobActivityBudgetsDELETERequest()
            {
                JobID = jobCreateRes.JobID,
                JobActivityBudgetID = budgetCreateRes.JobActivityBudgetID
            };

            await Client.DeleteAsync(budgetDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the activity budget was deleted.
            WebServiceException budgetDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(budgetGetReq);
            });
            Assert.That(budgetDeleteEx.StatusCode, Is.EqualTo(404));

            // Clean up the created service manager job.
            await Client.DeleteAsync(new ServiceManagerJobDELETERequest() { JobID = jobCreateRes.JobID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}


