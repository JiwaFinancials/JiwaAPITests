using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using ServiceStack;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class Category : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task InventoryCategory_CRUD_Category1()
        {
            // Create an inventory category
            InventoryCategoryPOSTRequest createReq = new InventoryCategoryPOSTRequest()
            {
                Description = "Category " + RandomString(5),
                CategoryNo = 1
            };
            var createRes = await Client.PostAsync(createReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Read all categories
            var getManyRes = await Client.GetAsync(new InventoryCategoryGETManyRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(getManyRes.Any(x => x.CategoryID == createRes.CategoryID), Is.True);

            // Read and update category
            InventoryCategoryGETRequest getReq = new InventoryCategoryGETRequest() { CategoryID = createRes.CategoryID };
            var getRes = await Client.GetAsync(getReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            InventoryCategoryPATCHRequest patchReq = new InventoryCategoryPATCHRequest()
            {
                CategoryID = createRes.CategoryID,
                Description = "Updated " + RandomString(5)
            };
            var patchRes = await Client.PatchAsync(patchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(patchRes.CategoryID, Is.EqualTo(createRes.CategoryID));

            // Remove category cache entry (internal endpoint)
            WebServiceException cacheEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                await Client.DeleteAsync(new InventoryCategoryCACHEDELETERequest());
            });
            Assert.That(cacheEx.ErrorMessage, Does.Contain("Invalid ClientKey"));

            // Delete the category
            await Client.DeleteAsync(new InventoryCategoryDELETERequest() { CategoryID = createRes.CategoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion

        #region "Category 2"
        [Test]
        public async Task InventoryCategory_CRUD_Category2()
        {
            // Create an inventory category
            InventoryCategoryPOSTRequest createReq = new InventoryCategoryPOSTRequest()
            {
                Description = "Category " + RandomString(5),
                CategoryNo = 2
            };
            var createRes = await Client.PostAsync(createReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Read all categories
            var getManyRes = await Client.GetAsync(new InventoryCategoryGETManyRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(getManyRes.Any(x => x.CategoryID == createRes.CategoryID), Is.True);

            // Read and update category
            InventoryCategoryGETRequest getReq = new InventoryCategoryGETRequest() { CategoryID = createRes.CategoryID };
            var getRes = await Client.GetAsync(getReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            InventoryCategoryPATCHRequest patchReq = new InventoryCategoryPATCHRequest()
            {
                CategoryID = createRes.CategoryID,
                Description = "Updated " + RandomString(5)
            };
            var patchRes = await Client.PatchAsync(patchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(patchRes.CategoryID, Is.EqualTo(createRes.CategoryID));

            // Delete the category
            await Client.DeleteAsync(new InventoryCategoryDELETERequest() { CategoryID = createRes.CategoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion

        #region "Category 3"
        [Test]
        public async Task InventoryCategory_CRUD_Category3()
        {
            // Create an inventory category
            InventoryCategoryPOSTRequest createReq = new InventoryCategoryPOSTRequest()
            {
                Description = "Category " + RandomString(5),
                CategoryNo = 3
            };
            var createRes = await Client.PostAsync(createReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Read all categories
            var getManyRes = await Client.GetAsync(new InventoryCategoryGETManyRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(getManyRes.Any(x => x.CategoryID == createRes.CategoryID), Is.True);

            // Read and update category
            InventoryCategoryGETRequest getReq = new InventoryCategoryGETRequest() { CategoryID = createRes.CategoryID };
            var getRes = await Client.GetAsync(getReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            InventoryCategoryPATCHRequest patchReq = new InventoryCategoryPATCHRequest()
            {
                CategoryID = createRes.CategoryID,
                Description = "Updated " + RandomString(5)
            };
            var patchRes = await Client.PatchAsync(patchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(patchRes.CategoryID, Is.EqualTo(createRes.CategoryID));

            // Delete the category
            await Client.DeleteAsync(new InventoryCategoryDELETERequest() { CategoryID = createRes.CategoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion

        #region "Category 4"
        [Test]
        public async Task InventoryCategory_CRUD_Category4()
        {
            // Create an inventory category
            InventoryCategoryPOSTRequest createReq = new InventoryCategoryPOSTRequest()
            {
                Description = "Category " + RandomString(5),
                CategoryNo = 4
            };
            var createRes = await Client.PostAsync(createReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Read all categories
            var getManyRes = await Client.GetAsync(new InventoryCategoryGETManyRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(getManyRes.Any(x => x.CategoryID == createRes.CategoryID), Is.True);

            // Read and update category
            InventoryCategoryGETRequest getReq = new InventoryCategoryGETRequest() { CategoryID = createRes.CategoryID };
            var getRes = await Client.GetAsync(getReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            InventoryCategoryPATCHRequest patchReq = new InventoryCategoryPATCHRequest()
            {
                CategoryID = createRes.CategoryID,
                Description = "Updated " + RandomString(5)
            };
            var patchRes = await Client.PatchAsync(patchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(patchRes.CategoryID, Is.EqualTo(createRes.CategoryID));

            // Delete the category
            await Client.DeleteAsync(new InventoryCategoryDELETERequest() { CategoryID = createRes.CategoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion

        #region "Category 5"
        [Test]
        public async Task InventoryCategory_CRUD_Category5()
        {
            // Create an inventory category
            InventoryCategoryPOSTRequest createReq = new InventoryCategoryPOSTRequest()
            {
                Description = "Category " + RandomString(5),
                CategoryNo = 5
            };
            var createRes = await Client.PostAsync(createReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Read all categories
            var getManyRes = await Client.GetAsync(new InventoryCategoryGETManyRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(getManyRes.Any(x => x.CategoryID == createRes.CategoryID), Is.True);

            // Read and update category
            InventoryCategoryGETRequest getReq = new InventoryCategoryGETRequest() { CategoryID = createRes.CategoryID };
            var getRes = await Client.GetAsync(getReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            InventoryCategoryPATCHRequest patchReq = new InventoryCategoryPATCHRequest()
            {
                CategoryID = createRes.CategoryID,
                Description = "Updated " + RandomString(5)
            };
            var patchRes = await Client.PatchAsync(patchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(patchRes.CategoryID, Is.EqualTo(createRes.CategoryID));

            // Delete the category
            await Client.DeleteAsync(new InventoryCategoryDELETERequest() { CategoryID = createRes.CategoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

