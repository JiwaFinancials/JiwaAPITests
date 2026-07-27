using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using ServiceStack;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class WebStoreCategory : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task InventoryWebStoreCategory_CRUD()
        {
            // Create a web store category
            InventoryWebStoreCategoryPOSTRequest createReq = new InventoryWebStoreCategoryPOSTRequest()
            {
                Name = "WebCat " + RandomString(5)
            };
            var createRes = await Client.PostAsync(createReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Read all web store categories
            var getManyRes = await Client.GetAsync(new InventoryWebStoreCategoryGETManyRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(getManyRes.Any(x => x.RecID == createRes.RecID), Is.True);

            // Read and update the web store category
            InventoryWebStoreCategoryGETRequest getReq = new InventoryWebStoreCategoryGETRequest() { RecID = createRes.RecID };
            var getRes = await Client.GetAsync(getReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            InventoryWebStoreCategoryPATCHRequest patchReq = new InventoryWebStoreCategoryPATCHRequest()
            {
                RecID = createRes.RecID,
                Name = "Updated " + RandomString(5)
            };
            var patchRes = await Client.PatchAsync(patchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(patchRes.RecID, Is.EqualTo(createRes.RecID));

            // Remove web store category cache entry (internal endpoint)
            WebServiceException cacheEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                await Client.DeleteAsync(new InventoryWebStoreCategoryCACHEDELETERequest());
            });
            Assert.That(cacheEx.ErrorMessage, Does.Contain("Invalid ClientKey"));

            // Delete the web store category
            await Client.DeleteAsync(new InventoryWebStoreCategoryDELETERequest() { RecID = createRes.RecID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

