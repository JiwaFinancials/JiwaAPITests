using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager;
using NUnit.Framework;
using ServiceStack;
using System.Threading.Tasks;

namespace JiwaAPITests.ServiceManager
{
    public class Jobs : ServiceManagerTestBase
    {
        #region "{Main}"
        [Test]
        public async Task ServiceManager_Jobs_CRUD()
        {
            // Create a service manager job.
            Job jobCreateRes = await CreateJobAsync();

            // Read the created service manager job.
            ServiceManagerJobGETRequest jobGetReq = new ServiceManagerJobGETRequest()
            {
                JobID = jobCreateRes.JobID
            };

            Job jobGetRes = await Client.GetAsync(jobGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(jobGetRes.JobID, Is.EqualTo(jobCreateRes.JobID));

            // Update the service manager job.
            ServiceManagerJobPATCHRequest jobPatchReq = new ServiceManagerJobPATCHRequest()
            {
                JobID = jobCreateRes.JobID,
                ContactName = "Updated Contact " + RandomString(6)
            };

            Job jobPatchRes = await Client.PatchAsync(jobPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(jobPatchRes.JobID, Is.EqualTo(jobCreateRes.JobID));
            Assert.That(jobPatchRes.ContactName, Is.EqualTo(jobPatchReq.ContactName));

            // Verify the service manager job was updated.
            jobGetRes = await Client.GetAsync(jobGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(jobGetRes.ContactName, Is.EqualTo(jobPatchReq.ContactName));

            // Delete the service manager job.
            ServiceManagerJobDELETERequest jobDeleteReq = new ServiceManagerJobDELETERequest()
            {
                JobID = jobCreateRes.JobID
            };

            await Client.DeleteAsync(jobDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the service manager job was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(jobGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "{Cache}"
        [Test]
        public async Task ServiceManager_Jobs_CacheDelete()
        {
            // Create a service manager job to supply a JobID.
            Job jobCreateRes = await CreateJobAsync();

            // Attempt to clear the service manager job cache.
            ServiceManagerJobCACHEDELETERequest cacheDeleteReq = new ServiceManagerJobCACHEDELETERequest()
            {
                JobID = jobCreateRes.JobID
            };

            WebServiceException cacheEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                await Client.DeleteAsync(cacheDeleteReq);
            });
            Assert.That(cacheEx.ErrorMessage, Does.Contain("Invalid ClientKey"));

            // Clean up the created service manager job.
            await Client.DeleteAsync(new ServiceManagerJobDELETERequest() { JobID = jobCreateRes.JobID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}
