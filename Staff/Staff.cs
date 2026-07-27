using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System.Linq;
using System.Threading.Tasks;
using HRStaffDto = JiwaFinancials.Jiwa.JiwaServiceModel.Tables.HR_Staff;

namespace JiwaAPITests.Staff
{
    public class Staff : JiwaAPITest
    {
        #region "Queries_HR_Staff"
        [Test]
        public async Task HR_StaffQuery()
        {
            HR_StaffQuery queryRequest = new HR_StaffQuery()
            {
                Take = 10,
                OrderBy = "StaffID"
            };

            QueryResponse<HRStaffDto> queryResponse;

            // Read staff members.
            queryResponse = await Client.GetAsync(queryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(queryResponse, Is.Not.Null);
            Assert.That(queryResponse.Results, Is.Not.Null);
            Assert.That(queryResponse.Results.Count, Is.GreaterThan(0));

            HRStaffDto firstStaff = queryResponse.Results.First();
            Assert.That(firstStaff.StaffID, Is.Not.Null.And.Not.Empty);

            // Read a known staff member using a filter.
            queryRequest = new HR_StaffQuery()
            {
                StaffID = firstStaff.StaffID
            };

            queryResponse = await Client.GetAsync(queryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(queryResponse.Results, Is.Not.Null);
            Assert.That(queryResponse.Results.Any(x => x.StaffID == firstStaff.StaffID), Is.True);

            // Verify an invalid API key is rejected.
            using (JsonApiClient tempClient = new JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                WebServiceException ex = Assert.Throws<WebServiceException>(() => queryResponse = tempClient.Get(queryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }
        }
        #endregion
    }
}
