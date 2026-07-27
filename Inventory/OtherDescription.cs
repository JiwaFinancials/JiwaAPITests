using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class OtherDescription : InventoryTestBase
    {
        #region "Other Descriptions"
        [Test]
        public async Task InventoryItem_OtherDescriptions()
        {
            // Create an inventory item for the other description routes.
            InventoryItem item = await CreateInventoryItemAsync("Other Description Item");

            // Read all other descriptions for the inventory item.
            await Client.GetAsync(new InventoryOtherDescriptionsGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Create another description for the inventory item.
            await Client.PostAsync(new InventoryOtherDescriptionPOSTRequest() { InventoryID = item.InventoryID, Description = "Other " + RandomString(4) });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}

