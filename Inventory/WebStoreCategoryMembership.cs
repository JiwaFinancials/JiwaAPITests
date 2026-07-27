using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class WebStoreCategoryMembership : InventoryTestBase
    {
        #region "Web Store Category Memberships"
        [Test]
        public async Task InventoryItem_WebStoreCategoryMemberships()
        {
            // Create the inventory item and web store category dependencies.
            InventoryItem item = await CreateInventoryItemAsync("Membership Item");
            var webStoreCategory = await Client.PostAsync(new InventoryWebStoreCategoryPOSTRequest() { Name = "Membership " + RandomString(4) });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a web store category membership for the inventory item.
            await Client.PostAsync(new InventoryWebStoreCategoryMembershipPOSTRequest()
            {
                InventoryID = item.InventoryID,
                WebStoreCategory_RecID = webStoreCategory.RecID
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Read all memberships for the inventory item.
            var memberships = await Client.GetAsync(new InventoryWebStoreCategoryMembershipGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Replace the membership set and verify the created category is still assigned.
            var setMembershipsRes = await Client.PutAsync(new InventoryWebStoreCategoryMembershipPUTRequest()
            {
                InventoryID = item.InventoryID,
                WebStoreCategoryIDs = new List<string>() { webStoreCategory.RecID }
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(setMembershipsRes.Any(x => x.WebStoreCategory_RecID == webStoreCategory.RecID), Is.True);
            Assert.That(memberships.Any(x => x.WebStoreCategory_RecID == webStoreCategory.RecID), Is.True);

            // Delete the created membership.
            await Client.DeleteAsync(new InventoryWebStoreCategoryMembershipDELETERequest() { InventoryID = item.InventoryID, WebStoreCategoryID = webStoreCategory.RecID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Delete the created web store category and inventory item.
            await Client.DeleteAsync(new InventoryWebStoreCategoryDELETERequest() { RecID = webStoreCategory.RecID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}

