using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Threading.Tasks;

namespace JiwaAPITests.Regions
{
    public class Region : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Region_CRUD()
        {
            // Create a region
            RegionPOSTRequest regionCreateReq = new RegionPOSTRequest()
            {
                Name = $"Region {RandomString(8)}",
                Description = "Test Region"
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Regions.Region regionCreateRes = await Client.PostAsync(regionCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(regionCreateRes.RegionID, Is.Not.Null);
            Assert.That(regionCreateRes.Name, Is.EqualTo(regionCreateReq.Name));

            // Read the created region
            RegionGETRequest regionGetReq = new RegionGETRequest()
            {
                RegionID = regionCreateRes.RegionID
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Regions.Region regionGetRes = await Client.GetAsync(regionGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(regionGetRes.RegionID, Is.EqualTo(regionCreateRes.RegionID));
            Assert.That(regionGetRes.Name, Is.EqualTo(regionCreateReq.Name));

            // Update the created region
            RegionPATCHRequest regionPatchReq = new RegionPATCHRequest()
            {
                RegionID = regionCreateRes.RegionID,
                Name = $"Updated Region {RandomString(6)}",
                Description = "Updated Test Region"
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Regions.Region regionPatchRes = await Client.PatchAsync(regionPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(regionPatchRes.RegionID, Is.EqualTo(regionPatchReq.RegionID));
            Assert.That(regionPatchRes.RegionID, Is.EqualTo(regionCreateRes.RegionID));
            Assert.That(regionPatchRes.Name, Is.EqualTo(regionPatchReq.Name));

            // Verify the region was updated
            regionGetRes = await Client.GetAsync(regionGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(regionGetRes.Name, Is.EqualTo(regionPatchReq.Name));
            Assert.That(regionGetRes.Description, Is.EqualTo(regionPatchReq.Description));

            // Delete the region
            RegionDELETERequest regionDeleteReq = new RegionDELETERequest()
            {
                RegionID = regionCreateRes.RegionID
            };

            await Client.DeleteAsync(regionDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the region was deleted
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.Regions.Region deletedRegionRes = await Client.GetAsync(regionGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}


