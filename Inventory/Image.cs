using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class Image : InventoryTestBase
    {
        #region "Images"
        [Test]
        public async Task InventoryItem_Images()
        {
            // Create an inventory item for the image routes.
            InventoryItem item = await CreateInventoryItemAsync("Image Item");

            // Read all images for the inventory item.
            await Client.GetAsync(new InventoryImageGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}
