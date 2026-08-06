using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Web;
using System;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class Picture : InventoryTestBase
    {
        #region "Picture"
        [Test]
        public async Task InventoryItem_Picture()
        {
            // Create an inventory item for the picture route.
            InventoryItem item = await CreateInventoryItemAsync("Picture Item");

            // Read the picture for the inventory item (there will not be one - 404).
            InventoryPictureGETRequest inventoryPictureGetReq = new InventoryPictureGETRequest() { InventoryID = item.InventoryID };
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                IHttpResult itemGetRes = await Client.GetAsync(inventoryPictureGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            //Add a piucture to the inventory item.
            byte[] testPicture = Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADElEQVQI12P4z8AAAAMBAQDJ/pLvAAAAAElFTkSuQmCC");

            InventoryPATCHRequest itemPatchReq = new InventoryPATCHRequest()
            {
                InventoryID = item.InventoryID,
                Description = "Updated Item Test",
                Picture = testPicture
            };
            InventoryItem itemPatchRes = await Client.PatchAsync(itemPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(itemPatchRes.InventoryID, Is.EqualTo(itemPatchReq.InventoryID));
            Assert.That(itemPatchRes.Description, Is.EqualTo(itemPatchReq.Description));
            Assert.That(itemPatchRes.Picture, Is.EqualTo(itemPatchReq.Picture));

            // Read the picture for the inventory item (there is one now).
            await Client.GetAsync(new InventoryPictureGETRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}

