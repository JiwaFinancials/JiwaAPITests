using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Linq;
using System.Threading.Tasks;
using HRStaffDto = JiwaFinancials.Jiwa.JiwaServiceModel.Tables.HR_Staff;
using StaffTimesheetDto = JiwaFinancials.Jiwa.JiwaServiceModel.Staff.Timesheets.Timesheet;

namespace JiwaAPITests.Staff.Timesheets
{
    public class Timesheet : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task StaffTimesheet_CRUD()
        {
            // Read the Admin staff member to use for timesheet creation.
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

            // Create a staff timesheet.
            StaffTimesheetPOSTRequest timesheetCreateReq = new StaffTimesheetPOSTRequest()
            {
                StaffID = adminStaff.StaffID,
                StaffUserName = adminStaff.Username,
                TimeSheetDate = DateTimeOffset.UtcNow,
                Reference = "Timesheet " + RandomString(8),
                IsActivated = false
            };

            StaffTimesheetDto timesheetCreateRes = await Client.PostAsync(timesheetCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(timesheetCreateRes.TimesheetID, Is.Not.Null.And.Not.Empty);
            Assert.That(timesheetCreateRes.Reference, Is.EqualTo(timesheetCreateReq.Reference));

            // Read the created staff timesheet.
            StaffTimesheetGETRequest timesheetGetReq = new StaffTimesheetGETRequest()
            {
                TimesheetID = timesheetCreateRes.TimesheetID
            };

            StaffTimesheetDto timesheetGetRes = await Client.GetAsync(timesheetGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(timesheetGetRes.TimesheetID, Is.EqualTo(timesheetCreateRes.TimesheetID));
            Assert.That(timesheetGetRes.Reference, Is.EqualTo(timesheetCreateReq.Reference));

            // Update the staff timesheet.
            StaffTimesheetPATCHRequest timesheetPatchReq = new StaffTimesheetPATCHRequest()
            {
                TimesheetID = timesheetCreateRes.TimesheetID,
                Reference = "Updated Timesheet " + RandomString(6)
            };

            StaffTimesheetDto timesheetPatchRes = await Client.PatchAsync(timesheetPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(timesheetPatchRes.TimesheetID, Is.EqualTo(timesheetPatchReq.TimesheetID));
            Assert.That(timesheetPatchRes.TimesheetID, Is.EqualTo(timesheetCreateRes.TimesheetID));
            Assert.That(timesheetPatchRes.Reference, Is.EqualTo(timesheetPatchReq.Reference));

            // Verify the staff timesheet was updated.
            timesheetGetRes = await Client.GetAsync(timesheetGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(timesheetGetRes.Reference, Is.EqualTo(timesheetPatchReq.Reference));

            // Delete the staff timesheet.
            StaffTimesheetDELETERequest timesheetDeleteReq = new StaffTimesheetDELETERequest()
            {
                TimesheetID = timesheetCreateRes.TimesheetID
            };

            await Client.DeleteAsync(timesheetDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the staff timesheet was deleted.
            WebServiceException timesheetDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(timesheetGetReq);
            });
            Assert.That(timesheetDeleteEx.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "{Cache}"
        [Test]
        public async Task StaffTimesheet_CacheDelete()
        {
            // Attempt to clear the staff timesheet cache.
            StaffTimesheetCACHEDELETERequest cacheDeleteReq = new StaffTimesheetCACHEDELETERequest()
            {
                TimesheetID = RandomString(10)
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

