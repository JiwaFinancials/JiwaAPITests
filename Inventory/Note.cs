using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class Note : InventoryTestBase
    {
        #region "Notes"
        [Test]
        public async Task InventoryItem_Notes()
        {
            // Create an inventory item for the note routes.
            InventoryItem item = await CreateInventoryItemAsync("Note Item");

            // Read all notes for the inventory item.
            await Client.GetAsync(new InventoryNotesGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}
