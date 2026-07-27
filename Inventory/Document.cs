using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class Document : InventoryTestBase
    {
        #region "Documents"
        [Test]
        public async Task InventoryItem_Documents()
        {
            // Create an inventory item for the document routes.
            InventoryItem item = await CreateInventoryItemAsync("Document Item");

            // Read all documents for the inventory item.
            await Client.GetAsync(new InventoryDocumentsGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}
