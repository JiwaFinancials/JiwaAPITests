using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Staff.Configuration;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRStaffDto = JiwaFinancials.Jiwa.JiwaServiceModel.Tables.HR_Staff;
using StaffDepartmentCategoryDto = JiwaFinancials.Jiwa.JiwaServiceModel.Staff.Configuration.StaffDepartmentCategory;
using StaffDepartmentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Staff.Configuration.StaffDepartment;

namespace JiwaAPITests.Staff
{
    public class Categories : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task StaffDepartment_Categories_CRUD()
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
                IsDefault = false,
                ManagerStaffID = adminStaff.StaffID,
            };

            StaffDepartmentDto departmentCreateRes = await Client.PostAsync(departmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(departmentCreateRes.DepartmentID, Is.Not.Null);

            // Create a staff department category.
            StaffDepartmentCategoryPOSTRequest categoryCreateReq = new StaffDepartmentCategoryPOSTRequest()
            {
                DepartmentID = departmentCreateRes.DepartmentID,
                Name = "Category " + RandomString(8),
                ItemNo = 1,
                IsEnabled = true
            };

            StaffDepartmentCategoryDto categoryCreateRes = await Client.PostAsync(categoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(categoryCreateRes.CategoryID, Is.Not.Null);
            Assert.That(categoryCreateRes.Name, Is.EqualTo(categoryCreateReq.Name));

            // Read all categories for the staff department.
            StaffDepartmentCategorysGETManyRequest categoriesGetManyReq = new StaffDepartmentCategorysGETManyRequest()
            {
                DepartmentID = departmentCreateRes.DepartmentID
            };

            List<StaffDepartmentCategoryDto> categoriesGetManyRes = await Client.GetAsync(categoriesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(categoriesGetManyRes.Any(x => x.CategoryID == categoryCreateRes.CategoryID), Is.True);

            // Read the created staff department category.
            StaffDepartmentCategoryGETRequest categoryGetReq = new StaffDepartmentCategoryGETRequest()
            {
                DepartmentID = departmentCreateRes.DepartmentID,
                CategoryID = categoryCreateRes.CategoryID
            };

            StaffDepartmentCategoryDto categoryGetRes = await Client.GetAsync(categoryGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(categoryGetRes.CategoryID, Is.EqualTo(categoryCreateRes.CategoryID));
            Assert.That(categoryGetRes.Name, Is.EqualTo(categoryCreateReq.Name));

            // Update the staff department category.
            StaffDepartmentCategoryPATCHRequest categoryPatchReq = new StaffDepartmentCategoryPATCHRequest()
            {
                DepartmentID = departmentCreateRes.DepartmentID,
                CategoryID = categoryCreateRes.CategoryID,
                Name = "Updated Category " + RandomString(6),
                IsEnabled = false
            };

            StaffDepartmentCategoryDto categoryPatchRes = await Client.PatchAsync(categoryPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(categoryPatchRes.CategoryID, Is.EqualTo(categoryPatchReq.CategoryID));
            Assert.That(categoryPatchRes.CategoryID, Is.EqualTo(categoryCreateRes.CategoryID));
            Assert.That(categoryPatchRes.Name, Is.EqualTo(categoryPatchReq.Name));

            // Verify the category was updated.
            categoryGetRes = await Client.GetAsync(categoryGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(categoryGetRes.Name, Is.EqualTo(categoryPatchReq.Name));
            Assert.That(categoryGetRes.IsEnabled, Is.EqualTo(categoryPatchReq.IsEnabled));

            // Replace the staff department category.
            StaffDepartmentCategoryPUTRequest categoryPutReq = new StaffDepartmentCategoryPUTRequest()
            {
                DepartmentID = departmentCreateRes.DepartmentID,
                CategoryID = categoryCreateRes.CategoryID,
                Name = "Replaced Category " + RandomString(6),
                IsEnabled = true
            };

            StaffDepartmentCategoryDto categoryPutRes = await Client.PutAsync(categoryPutReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(categoryPutRes.CategoryID, Is.EqualTo(categoryCreateRes.CategoryID));
            Assert.That(categoryPutRes.Name, Is.EqualTo(categoryPutReq.Name));

            // Verify the category was replaced.
            categoryGetRes = await Client.GetAsync(categoryGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(categoryGetRes.Name, Is.EqualTo(categoryPutReq.Name));
            Assert.That(categoryGetRes.IsEnabled, Is.EqualTo(categoryPutReq.IsEnabled));

            // Delete the staff department category.
            StaffDepartmentCategoryDELETERequest categoryDeleteReq = new StaffDepartmentCategoryDELETERequest()
            {
                DepartmentID = departmentCreateRes.DepartmentID,
                CategoryID = categoryCreateRes.CategoryID
            };

            await Client.DeleteAsync(categoryDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the staff department category was deleted.
            WebServiceException categoryDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(categoryGetReq);
            });
            Assert.That(categoryDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all categories and ensure the deleted category is no longer returned.
            categoriesGetManyRes = await Client.GetAsync(categoriesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(categoriesGetManyRes.Any(x => x.CategoryID == categoryCreateRes.CategoryID), Is.False);

            // Delete the staff department.
            StaffDepartmentDELETERequest departmentDeleteReq = new StaffDepartmentDELETERequest()
            {
                DepartmentID = departmentCreateRes.DepartmentID
            };

            await Client.DeleteAsync(departmentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

