using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using ServiceStack;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class PricingGroup : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task InventoryPricingGroup_CRUD()
        {
            // Create a pricing group
            InventoryPricingGroupPOSTRequest createReq = new InventoryPricingGroupPOSTRequest()
            {
                Description = "Pricing " + RandomString(5)
            };
            var createRes = await Client.PostAsync(createReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Read all pricing groups
            var getManyRes = await Client.GetAsync(new InventoryPricingGroupsGETManyRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(getManyRes.Any(x => x.PricingGroupID == createRes.PricingGroupID), Is.True);

            // Read and update the pricing group
            InventoryPricingGroupGETRequest getReq = new InventoryPricingGroupGETRequest() { PricingGroupID = createRes.PricingGroupID };
            var getRes = await Client.GetAsync(getReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            InventoryPricingGroupPATCHRequest patchReq = new InventoryPricingGroupPATCHRequest()
            {
                PricingGroupID = createRes.PricingGroupID,
                Description = "Updated " + RandomString(5)
            };
            var patchRes = await Client.PatchAsync(patchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(patchRes.PricingGroupID, Is.EqualTo(patchReq.PricingGroupID));
            Assert.That(patchRes.PricingGroupID, Is.EqualTo(createRes.PricingGroupID));

            // Remove pricing group cache entry (internal endpoint)
            WebServiceException cacheEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                await Client.DeleteAsync(new InventoryPricingGroupCACHEDELETERequest());
            });
            Assert.That(cacheEx.ErrorMessage, Does.Contain("Invalid ClientKey"));

            // Delete the pricing group
            await Client.DeleteAsync(new InventoryPricingGroupDELETERequest() { PricingGroupID = createRes.PricingGroupID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}


