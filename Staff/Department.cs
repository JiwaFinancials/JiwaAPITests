using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Staff.Configuration;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System.Linq;
using System.Threading.Tasks;
using HRStaffDto = JiwaFinancials.Jiwa.JiwaServiceModel.Tables.HR_Staff;
using StaffDepartmentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Staff.Configuration.StaffDepartment;

namespace JiwaAPITests.Staff
{
    public class Department : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task StaffDepartment_CRUD()
        {
            // Read the Admin staff member to use as the department manager.
            HR_StaffQuery adminStaffQueryReq = new HR_StaffQuery()
            {
                Username = "Admin",
                Take = 1
            };

            QueryResponse<HRStaffDto> adminStaffQueryRes = await Client.GetAsync(adminStaffQueryReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            HRStaffDto adminStaff = adminStaffQueryRes.Results.FirstOrDefault();
            Assert.That(adminStaff, Is.Not.Null);
            Assert.That(adminStaff.StaffID, Is.Not.Null.And.Not.Empty);

            // Create a staff department.
            StaffDepartmentPOSTRequest departmentCreateReq = new StaffDepartmentPOSTRequest()
            {
                Name = "Department " + RandomString(8),
                IsEnabled = true,
                ManagerStaffID = adminStaff.StaffID
            };

            StaffDepartmentDto departmentCreateRes = await Client.PostAsync(departmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(departmentCreateRes.DepartmentID, Is.Not.Null);
            Assert.That(departmentCreateRes.Name, Is.EqualTo(departmentCreateReq.Name));

            // Read the created staff department.
            StaffDepartmentGETRequest departmentGetReq = new StaffDepartmentGETRequest()
            {
                DepartmentID = departmentCreateRes.DepartmentID
            };

            StaffDepartmentDto departmentGetRes = await Client.GetAsync(departmentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(departmentGetRes.DepartmentID, Is.EqualTo(departmentCreateRes.DepartmentID));
            Assert.That(departmentGetRes.Name, Is.EqualTo(departmentCreateReq.Name));

            // Update the staff department.
            StaffDepartmentPATCHRequest departmentPatchReq = new StaffDepartmentPATCHRequest()
            {
                DepartmentID = departmentCreateRes.DepartmentID,
                Name = "Updated Department " + RandomString(6),
                IsEnabled = false
            };

            StaffDepartmentDto departmentPatchRes = await Client.PatchAsync(departmentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(departmentPatchRes.DepartmentID, Is.EqualTo(departmentCreateRes.DepartmentID));
            Assert.That(departmentPatchRes.Name, Is.EqualTo(departmentPatchReq.Name));

            // Verify the staff department was updated.
            departmentGetRes = await Client.GetAsync(departmentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(departmentGetRes.Name, Is.EqualTo(departmentPatchReq.Name));
            Assert.That(departmentGetRes.IsEnabled, Is.EqualTo(departmentPatchReq.IsEnabled));

            // Delete the staff department.
            StaffDepartmentDELETERequest departmentDeleteReq = new StaffDepartmentDELETERequest()
            {
                DepartmentID = departmentCreateRes.DepartmentID
            };

            await Client.DeleteAsync(departmentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the staff department was deleted.
            WebServiceException departmentDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(departmentGetReq);
            });
            Assert.That(departmentDeleteEx.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "{Cache}"
        [Test]
        public async Task StaffDepartment_CacheDelete()
        {
            // Attempt to clear the staff department cache.
            StaffDepartmentCACHEDELETERequest cacheDeleteReq = new StaffDepartmentCACHEDELETERequest()
            {
                DepartmentID = RandomString(10)
            };

            WebServiceException cacheEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                await Client.DeleteAsync(cacheDeleteReq);
            });
            Assert.That(cacheEx.ErrorMessage, Does.Contain("Invalid ClientKey"));
        }
        #endregion
    }
}
