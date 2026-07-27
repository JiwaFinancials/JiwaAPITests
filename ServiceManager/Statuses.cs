using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager.Configuration;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.ServiceManager
{
    public class Statuses : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_Statuses_CRUD()
        {
            // Read existing statuses so a default can be restored before deleting the test status.
            ServiceManagerStatusesGETManyRequest statusesGetManyReq = new ServiceManagerStatusesGETManyRequest();
            List<ServiceManagerStatus> existingStatuses = await Client.GetAsync(statusesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            ServiceManagerStatus originalDefaultStatus = existingStatuses.FirstOrDefault(x => x.IsDefault == true);
            Assert.That(originalDefaultStatus, Is.Not.Null);

            // Create a service manager status.
            ServiceManagerStatusesPOSTRequest statusCreateReq = new ServiceManagerStatusesPOSTRequest()
            {
                Name = "Status " + RandomString(8),
                Description = "Service manager status " + RandomString(8),
                IsEnabled = true,
                IsDefault = false
            };

            ServiceManagerStatus statusCreateRes = await Client.PostAsync(statusCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(statusCreateRes.StatusID, Is.Not.Null);
            Assert.That(statusCreateRes.Name, Is.EqualTo(statusCreateReq.Name));

            // Read all service manager statuses.
            List<ServiceManagerStatus> statusesGetManyRes = await Client.GetAsync(statusesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(statusesGetManyRes.Any(x => x.StatusID == statusCreateRes.StatusID), Is.True);

            // Read the created service manager status.
            ServiceManagerStatusesGETRequest statusGetReq = new ServiceManagerStatusesGETRequest()
            {
                StatusID = statusCreateRes.StatusID
            };

            ServiceManagerStatus statusGetRes = await Client.GetAsync(statusGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(statusGetRes.StatusID, Is.EqualTo(statusCreateRes.StatusID));

            // Update the created service manager status.
            ServiceManagerStatusesPATCHRequest statusPatchReq = new ServiceManagerStatusesPATCHRequest()
            {
                StatusID = statusCreateRes.StatusID,
                Name = "Updated Status " + RandomString(6),
                Description = "Updated status description",
                IsEnabled = true,
                IsDefault = false
            };

            ServiceManagerStatus statusPatchRes = await Client.PatchAsync(statusPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(statusPatchRes.StatusID, Is.EqualTo(statusCreateRes.StatusID));
            Assert.That(statusPatchRes.Name, Is.EqualTo(statusPatchReq.Name));

            // Verify the service manager status was updated.
            statusGetRes = await Client.GetAsync(statusGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(statusGetRes.Name, Is.EqualTo(statusPatchReq.Name));

            if (statusGetRes.IsDefault == true && originalDefaultStatus.StatusID != statusCreateRes.StatusID)
            {
                // Restore the original default status before deleting the created one.
                ServiceManagerStatusesPATCHRequest restoreDefaultStatusReq = new ServiceManagerStatusesPATCHRequest()
                {
                    StatusID = originalDefaultStatus.StatusID,
                    Name = originalDefaultStatus.Name,
                    Description = originalDefaultStatus.Description,
                    IsEnabled = originalDefaultStatus.IsEnabled,
                    IsDefault = true
                };

                _ = await Client.PatchAsync(restoreDefaultStatusReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            }

            // Delete the created service manager status.
            ServiceManagerStatusesDELETERequest statusDeleteReq = new ServiceManagerStatusesDELETERequest()
            {
                StatusID = statusCreateRes.StatusID
            };

            await Client.DeleteAsync(statusDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the service manager status was deleted.
            WebServiceException statusDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(statusGetReq);
            });
            Assert.That(statusDeleteEx.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "{Cache}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_Statuses_CacheDelete()
        {
            // Attempt to clear service manager statuses cache.
            ServiceManagerStatusesCACHEDELETERequest cacheDeleteReq = new ServiceManagerStatusesCACHEDELETERequest();

            WebServiceException cacheEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                await Client.DeleteAsync(cacheDeleteReq);
            });
            Assert.That(cacheEx.ErrorMessage, Does.Contain("Invalid ClientKey"));
        }
        #endregion
    }
}

