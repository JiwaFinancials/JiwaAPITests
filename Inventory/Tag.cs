using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class Tag : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Tag_CRUD()
        {
            // Create a tag
            InventoryTagPOSTRequest tagCreateReq = new InventoryTagPOSTRequest()
            {
                Text = RandomString(5)
            };

            InventoryTag tagCreateRes = await Client.PostAsync(tagCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(tagCreateRes.Text, Is.EqualTo(tagCreateReq.Text));            
            Assert.That(tagCreateRes.RecID, !Is.Null);

            // Read the created tag using the RecID
            InventoryTagGETRequest tagGetReq = new InventoryTagGETRequest() { RecID = tagCreateRes.RecID };
            InventoryTag tagGetRes = await Client.GetAsync(tagGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagGetRes.Text, Is.EqualTo(tagCreateReq.Text));            

            // Update the tag
            InventoryTagPATCHRequest tagPatchReq = new InventoryTagPATCHRequest()
            {
                RecID = tagCreateRes.RecID,
                Text = "Updated Tag Test"                
            };
            InventoryTag tagPatchRes = await Client.PatchAsync(tagPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagPatchRes.Text, Is.EqualTo(tagPatchReq.Text));            

            // Remove the created tag
            InventoryTagDELETERequest tagDeleteReq = new InventoryTagDELETERequest() { RecID = tagCreateRes.RecID };
            await Client.DeleteAsync(tagDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // ensure the deleted tag is not there anymore            
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                InventoryTag getDeletedRes = await Client.GetAsync(tagGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Try to GET non-existent tag to make sure we get a 404
            tagGetReq.RecID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                InventoryTag tagGetRes = await Client.GetAsync(tagGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}
