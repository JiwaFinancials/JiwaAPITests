using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager.Configuration;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.ServiceManager
{
    public class Priorities : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_Priorities_CRUD()
        {
            // Read existing priorities so a default can be restored before deleting the test priority.
            ServiceManagerPrioritiesGETManyRequest prioritiesGetManyReq = new ServiceManagerPrioritiesGETManyRequest();
            List<ServiceManagerPriority> existingPriorities = await Client.GetAsync(prioritiesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            ServiceManagerPriority originalDefaultPriority = existingPriorities.FirstOrDefault(x => x.IsDefault == true);
            Assert.That(originalDefaultPriority, Is.Not.Null);

            // Create a service manager priority.
            ServiceManagerPrioritiesPOSTRequest priorityCreateReq = new ServiceManagerPrioritiesPOSTRequest()
            {
                Name = "Priority " + RandomString(8),
                Description = "Service manager priority " + RandomString(8),
                IsEnabled = true,
                IsDefault = false,
                ResponseTime = 1M,
                DeadLine = 2M
            };

            ServiceManagerPriority priorityCreateRes = await Client.PostAsync(priorityCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(priorityCreateRes.PriorityID, Is.Not.Null);
            Assert.That(priorityCreateRes.Name, Is.EqualTo(priorityCreateReq.Name));

            // Read all service manager priorities.
            List<ServiceManagerPriority> prioritiesGetManyRes = await Client.GetAsync(prioritiesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(prioritiesGetManyRes.Any(x => x.PriorityID == priorityCreateRes.PriorityID), Is.True);

            // Read the created service manager priority.
            ServiceManagerPrioritiesGETRequest priorityGetReq = new ServiceManagerPrioritiesGETRequest()
            {
                PriorityID = priorityCreateRes.PriorityID
            };

            ServiceManagerPriority priorityGetRes = await Client.GetAsync(priorityGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(priorityGetRes.PriorityID, Is.EqualTo(priorityCreateRes.PriorityID));

            // Update the created service manager priority.
            ServiceManagerPrioritiesPATCHRequest priorityPatchReq = new ServiceManagerPrioritiesPATCHRequest()
            {
                PriorityID = priorityCreateRes.PriorityID,
                Name = "Updated Priority " + RandomString(6),
                Description = "Updated priority description",
                IsEnabled = true,
                IsDefault = false,
                ResponseTime = 3M,
                DeadLine = 4M
            };

            ServiceManagerPriority priorityPatchRes = await Client.PatchAsync(priorityPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(priorityPatchRes.PriorityID, Is.EqualTo(priorityCreateRes.PriorityID));
            Assert.That(priorityPatchRes.Name, Is.EqualTo(priorityPatchReq.Name));

            // Verify the service manager priority was updated.
            priorityGetRes = await Client.GetAsync(priorityGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(priorityGetRes.Name, Is.EqualTo(priorityPatchReq.Name));

            if (priorityGetRes.IsDefault == true && originalDefaultPriority.PriorityID != priorityCreateRes.PriorityID)
            {
                // Restore the original default priority before deleting the created one.
                ServiceManagerPrioritiesPATCHRequest restoreDefaultPriorityReq = new ServiceManagerPrioritiesPATCHRequest()
                {
                    PriorityID = originalDefaultPriority.PriorityID,
                    Name = originalDefaultPriority.Name,
                    Description = originalDefaultPriority.Description,
                    IsEnabled = originalDefaultPriority.IsEnabled,
                    IsDefault = true,
                    ResponseTime = originalDefaultPriority.ResponseTime,
                    DeadLine = originalDefaultPriority.DeadLine
                };

                _ = await Client.PatchAsync(restoreDefaultPriorityReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            }

            // Delete the created service manager priority.
            ServiceManagerPrioritiesDELETERequest priorityDeleteReq = new ServiceManagerPrioritiesDELETERequest()
            {
                PriorityID = priorityCreateRes.PriorityID
            };

            await Client.DeleteAsync(priorityDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the service manager priority was deleted.
            WebServiceException priorityDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(priorityGetReq);
            });
            Assert.That(priorityDeleteEx.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "{Cache}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_Priorities_CacheDelete()
        {
            // Attempt to clear service manager priorities cache.
            ServiceManagerPrioritiesCACHEDELETERequest cacheDeleteReq = new ServiceManagerPrioritiesCACHEDELETERequest();

            WebServiceException cacheEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                await Client.DeleteAsync(cacheDeleteReq);
            });
            Assert.That(cacheEx.ErrorMessage, Does.Contain("Invalid ClientKey"));
        }
        #endregion
    }
}

