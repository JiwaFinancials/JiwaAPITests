using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager.Configuration;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.ServiceManager
{
    public class Activities : ServiceManagerTestBase
    {
        #region "{Main}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_LabourLines_Activities_CRUD()
        {
            // Read existing activities so a default can be restored before deleting the test activity.
            ServiceManagerActivitiesGETManyRequest activitiesGetManyReq = new ServiceManagerActivitiesGETManyRequest();
            List<ServiceManagerActivity> existingActivities = await Client.GetAsync(activitiesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            ServiceManagerActivity originalDefaultActivity = existingActivities.FirstOrDefault(x => x.IsDefault == true);
            Assert.That(originalDefaultActivity, Is.Not.Null);

            // Read all service manager activities.
            List<ServiceManagerActivity> activitiesGetManyRes = await Client.GetAsync(activitiesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(activitiesGetManyRes, Is.Not.Null);

            // Create a service manager activity.
            ServiceManagerActivitiesPOSTRequest activityCreateReq = new ServiceManagerActivitiesPOSTRequest()
            {
                Name = "Activity " + RandomString(8),
                Description = "Service manager activity " + RandomString(8),
                IsEnabled = true,
                IsDefault = false
            };

            ServiceManagerActivity activityCreateRes = await Client.PostAsync(activityCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(activityCreateRes.ActivityID, Is.Not.Null);
            Assert.That(activityCreateRes.Name, Is.EqualTo(activityCreateReq.Name));

            // Read all service manager activities again and ensure the created activity is returned.
            activitiesGetManyRes = await Client.GetAsync(activitiesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(activitiesGetManyRes.Any(x => x.ActivityID == activityCreateRes.ActivityID), Is.True);

            // Read the created service manager activity.
            ServiceManagerActivitiesGETRequest activityGetReq = new ServiceManagerActivitiesGETRequest()
            {
                ActivityID = activityCreateRes.ActivityID
            };

            ServiceManagerActivity activityGetRes = await Client.GetAsync(activityGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(activityGetRes.ActivityID, Is.EqualTo(activityCreateRes.ActivityID));

            // Update the created service manager activity.
            ServiceManagerActivitiesPATCHRequest activityPatchReq = new ServiceManagerActivitiesPATCHRequest()
            {
                ActivityID = activityCreateRes.ActivityID,
                Name = "Updated Activity " + RandomString(6),
                Description = "Updated service manager activity",
                IsEnabled = true,
                IsDefault = false
            };

            ServiceManagerActivity activityPatchRes = await Client.PatchAsync(activityPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(activityPatchRes.ActivityID, Is.EqualTo(activityPatchReq.ActivityID));
            Assert.That(activityPatchRes.ActivityID, Is.EqualTo(activityCreateRes.ActivityID));
            Assert.That(activityPatchRes.Name, Is.EqualTo(activityPatchReq.Name));

            // Verify the service manager activity was updated.
            activityGetRes = await Client.GetAsync(activityGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(activityGetRes.Name, Is.EqualTo(activityPatchReq.Name));

            if (activityGetRes.IsDefault == true && originalDefaultActivity.ActivityID != activityCreateRes.ActivityID)
            {
                // Restore the original default activity before deleting the created one.
                ServiceManagerActivitiesPATCHRequest restoreDefaultActivityReq = new ServiceManagerActivitiesPATCHRequest()
                {
                    ActivityID = originalDefaultActivity.ActivityID,
                    Name = originalDefaultActivity.Name,
                    Description = originalDefaultActivity.Description,
                    IsEnabled = originalDefaultActivity.IsEnabled,
                    IsDefault = true
                };

                var restoreDefaultActivityReqRes = await Client.PatchAsync(restoreDefaultActivityReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(restoreDefaultActivityReqRes.ActivityID, Is.EqualTo(restoreDefaultActivityReq.ActivityID));
            }

            // Delete the created service manager activity.
            ServiceManagerActivitiesDELETERequest activityDeleteReq = new ServiceManagerActivitiesDELETERequest()
            {
                ActivityID = activityCreateRes.ActivityID
            };

            await Client.DeleteAsync(activityDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the service manager activity was deleted.
            WebServiceException activityDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(activityGetReq);
            });
            Assert.That(activityDeleteEx.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "{Cache}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_LabourLines_Activities_CacheDelete()
        {
            // Attempt to clear service manager activities cache.
            ServiceManagerActivitiesCACHEDELETERequest cacheDeleteReq = new ServiceManagerActivitiesCACHEDELETERequest();

            WebServiceException cacheEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                await Client.DeleteAsync(cacheDeleteReq);
            });
            Assert.That(cacheEx.ErrorMessage, Does.Contain("Invalid ClientKey"));
        }
        #endregion
    }
}


