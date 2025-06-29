using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
 
    public class AttributeGroupTemplate : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task InventoryAtrributeGroupTemplate_CRUD()
        {
            // Create an inventory attribute group template with 3 attributes
            InventoryAttributeGroupTemplatePOSTRequest itemCreateReq = new InventoryAttributeGroupTemplatePOSTRequest()
            {
                Name = RandomString(5),                
                TemplateAttributes = new List<InventoryAttributeGroupTemplateAttribute>() 
                {  
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 0, ItemNo = 1 },
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 1 },
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 2 },
                }
            };

            InventoryAttributeGroupTemplate itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.Name, Is.EqualTo(itemCreateReq.Name));
            Assert.That(itemCreateRes.AttributeGroupTemplateID, !Is.Null);
            Assert.That(itemCreateRes.TemplateAttributes.Count, Is.EqualTo(itemCreateReq.TemplateAttributes.Count));

            // Read the created item using the AttributeGroupTemplateID
            InventoryAttributeGroupTemplateGETRequest itemGetReq = new InventoryAttributeGroupTemplateGETRequest() { AttributeGroupTemplateID = itemCreateRes.AttributeGroupTemplateID };
            InventoryAttributeGroupTemplate itemGetRes = await Client.GetAsync(itemGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(itemGetRes.Name, Is.EqualTo(itemCreateReq.Name));
            Assert.That(itemGetRes.TemplateAttributes.Count, Is.EqualTo(itemCreateReq.TemplateAttributes.Count));

            // Update the attribute group template - modify existing attribute and add a new one
            InventoryAttributeGroupTemplatePATCHRequest itemPatchReq = new InventoryAttributeGroupTemplatePATCHRequest()
            {
                AttributeGroupTemplateID = itemCreateRes.AttributeGroupTemplateID,
                Name = $"Updated {itemCreateReq.Name}",
                TemplateAttributes = new List<InventoryAttributeGroupTemplateAttribute>()
                {
                    new InventoryAttributeGroupTemplateAttribute() { Name = itemCreateRes.TemplateAttributes[0].Name , ItemNo = 2 },
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 3, ItemNo = 1 },
                }
            };
            InventoryAttributeGroupTemplate itemPatchRes = await Client.PatchAsync(itemPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(itemPatchRes.Name, Is.EqualTo(itemPatchReq.Name));
            Assert.That(itemPatchRes.TemplateAttributes.Count, Is.EqualTo(4));

            // Remove the created item
            InventoryAttributeGroupTemplateDELETERequest itemDeleteReq = new InventoryAttributeGroupTemplateDELETERequest() { AttributeGroupTemplateID = itemCreateRes.AttributeGroupTemplateID };
            await Client.DeleteAsync(itemDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // ensure the deleted item is not there anymore            
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                InventoryAttributeGroupTemplate getDeletedRes = await Client.GetAsync(itemGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Try to GET non-existent item to make sure we get a 404
            itemGetReq.AttributeGroupTemplateID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                InventoryAttributeGroupTemplate itemGetRes = await Client.GetAsync(itemGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}
