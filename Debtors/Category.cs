using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors.Category;

namespace JiwaAPITests.Debtors
{
    public class Category : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task DebtorCategory1_CRUD()
        {
            // Run the CRUD operations for debtor category 1
            await DebtorCategory_CRUD(1);
        }

        [Test]
        public async Task DebtorCategory2_CRUD()
        {
            // Run the CRUD operations for debtor category 2
            await DebtorCategory_CRUD(2);
        }

        [Test]
        public async Task DebtorCategory3_CRUD()
        {
            // Run the CRUD operations for debtor category 3
            await DebtorCategory_CRUD(3);
        }

        [Test]
        public async Task DebtorCategory4_CRUD()
        {
            // Run the CRUD operations for debtor category 4
            await DebtorCategory_CRUD(4);
        }

        [Test]
        public async Task DebtorCategory5_CRUD()
        {
            // Run the CRUD operations for debtor category 5
            await DebtorCategory_CRUD(5);
        }

        public async Task DebtorCategory_CRUD(int categoryNo)
        {
            // Ensure the categoryNo is valid
            Assert.That(categoryNo, Is.GreaterThan(0), "CategoryNo must be greater than 0");
            Assert.That(categoryNo, Is.LessThanOrEqualTo(5), "CategoryNo must be less tha or equal to 5");

            // Create a debtor category
            DebtorCategoryPOSTRequest categoryCreateReq = new DebtorCategoryPOSTRequest()
            {
                Description = RandomString(5),
                CategoryNo = categoryNo
            };

            DebtorCategory categoryCreateRes = await Client.PostAsync(categoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(categoryCreateRes.Description, Is.EqualTo(categoryCreateReq.Description));
            Assert.That(categoryCreateRes.CategoryNo, Is.EqualTo(categoryCreateReq.CategoryNo));
            Assert.That(categoryCreateRes.CategoryID, !Is.Null);

            // Read the created item using the CategoryID
            DebtorCategoryGETRequest categoryGetReq = new DebtorCategoryGETRequest() { CategoryID = categoryCreateRes.CategoryID };
            DebtorCategory categoryGetRes = await Client.GetAsync(categoryGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(categoryGetRes.Description, Is.EqualTo(categoryCreateReq.Description));

            // Update the category
            DebtorCategoryPATCHRequest categoryPatchReq = new DebtorCategoryPATCHRequest()
            {
                CategoryID = categoryCreateRes.CategoryID,
                Description = RandomString(5)
            };
            DebtorCategory categoryPatchRes = await Client.PatchAsync(categoryPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(categoryPatchRes.Description, Is.EqualTo(categoryPatchReq.Description));

            // Remove the created category
            DebtorCategoryDELETERequest categoryDELETEReq = new DebtorCategoryDELETERequest() { CategoryID = categoryCreateRes.CategoryID };
            await Client.DeleteAsync(categoryDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // ensure the deleted item is not there anymore            
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                DebtorCategory getDeletedRes = await Client.GetAsync(categoryGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Create another debtor category
            categoryCreateReq = new DebtorCategoryPOSTRequest()
            {
                Description = RandomString(5),
                CategoryNo = categoryNo
            };

            categoryCreateRes = await Client.PostAsync(categoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(categoryCreateRes.Description, Is.EqualTo(categoryCreateReq.Description));
            Assert.That(categoryCreateRes.CategoryNo, Is.EqualTo(categoryCreateReq.CategoryNo));
            Assert.That(categoryCreateRes.CategoryID, !Is.Null);

            // Make the category the default
            categoryPatchReq = new DebtorCategoryPATCHRequest()
            {
                CategoryID = categoryCreateRes.CategoryID,
                IsDefault = true
            };
            categoryPatchRes = await Client.PatchAsync(categoryPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(categoryPatchRes.IsDefault, Is.EqualTo(categoryPatchReq.IsDefault));

            // Ensure that we cannot delete the default category
            categoryDELETEReq = new DebtorCategoryDELETERequest() { CategoryID = categoryCreateRes.CategoryID };

            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                await Client.DeleteAsync(categoryDELETEReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(500));
        }
        #endregion
    }
}
