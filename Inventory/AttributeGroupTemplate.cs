using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;

namespace JiwaAPITests.Inventory
{
    public class AttributeGroupTemplate : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task InventoryAtrributeGroupTemplate_CRUD()
        {
            // Create an inventory attribute group template with 3 attributes
            InventoryAttributeGroupTemplatePOSTRequest attributeGroupTemplateCreateReq = new InventoryAttributeGroupTemplatePOSTRequest()
            {
                Name = RandomString(5),                
                TemplateAttributes = new List<InventoryAttributeGroupTemplateAttribute>() 
                {  
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 0, ItemNo = 1 },
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 1 },
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 2 },
                }
            };

            InventoryAttributeGroupTemplate attributeGroupTemplateCreateRes = await Client.PostAsync(attributeGroupTemplateCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(attributeGroupTemplateCreateRes.Name, Is.EqualTo(attributeGroupTemplateCreateReq.Name));
            Assert.That(attributeGroupTemplateCreateRes.AttributeGroupTemplateID, !Is.Null);
            Assert.That(attributeGroupTemplateCreateRes.TemplateAttributes.Count, Is.EqualTo(attributeGroupTemplateCreateReq.TemplateAttributes.Count));

            // Read the created item using the AttributeGroupTemplateID
            InventoryAttributeGroupTemplateGETRequest attributeGroupTemplateGetReq = new InventoryAttributeGroupTemplateGETRequest() { AttributeGroupTemplateID = attributeGroupTemplateCreateRes.AttributeGroupTemplateID };
            InventoryAttributeGroupTemplate attributeGroupTemplateGetRes = await Client.GetAsync(attributeGroupTemplateGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(attributeGroupTemplateGetRes.Name, Is.EqualTo(attributeGroupTemplateCreateReq.Name));
            Assert.That(attributeGroupTemplateGetRes.TemplateAttributes.Count, Is.EqualTo(attributeGroupTemplateCreateReq.TemplateAttributes.Count));

            // Update the attribute group template - modify existing attribute and add a new one
            InventoryAttributeGroupTemplatePATCHRequest attributeGroupTemplatePatchReq = new InventoryAttributeGroupTemplatePATCHRequest()
            {
                AttributeGroupTemplateID = attributeGroupTemplateCreateRes.AttributeGroupTemplateID,
                Name = $"Updated {attributeGroupTemplateCreateReq.Name}",
                TemplateAttributes = new List<InventoryAttributeGroupTemplateAttribute>()
                {
                    new InventoryAttributeGroupTemplateAttribute() { Name = attributeGroupTemplateCreateRes.TemplateAttributes[0].Name , ItemNo = 2 },
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 3, ItemNo = 1 },
                }
            };
            InventoryAttributeGroupTemplate attributeGroupTemplatePatchRes = await Client.PatchAsync(attributeGroupTemplatePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(attributeGroupTemplatePatchRes.Name, Is.EqualTo(attributeGroupTemplatePatchReq.Name));
            Assert.That(attributeGroupTemplatePatchRes.TemplateAttributes.Count, Is.EqualTo(4));

            // Remove the created item
            InventoryAttributeGroupTemplateDELETERequest attributeGroupTemplateDeleteReq = new InventoryAttributeGroupTemplateDELETERequest() { AttributeGroupTemplateID = attributeGroupTemplateCreateRes.AttributeGroupTemplateID };
            await Client.DeleteAsync(attributeGroupTemplateDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // ensure the deleted item is not there anymore            
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                InventoryAttributeGroupTemplate getDeletedRes = await Client.GetAsync(attributeGroupTemplateGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Try to GET non-existent item to make sure we get a 404
            attributeGroupTemplateGetReq.AttributeGroupTemplateID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                InventoryAttributeGroupTemplate attributeGroupTemplateGetRes = await Client.GetAsync(attributeGroupTemplateGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "Attributes"
        [Test]
        public async Task InventoryAttributeGroupTemplate_Attributes_CRUD()
        {
            // Create an inventory attribute group template
            InventoryAttributeGroupTemplatePOSTRequest templateCreateReq = new InventoryAttributeGroupTemplatePOSTRequest()
            {
                Name = RandomString(5),
                TemplateAttributes = new List<InventoryAttributeGroupTemplateAttribute>()
                {
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 0, ItemNo = 1 }
                }
            };

            InventoryAttributeGroupTemplate templateCreateRes = await Client.PostAsync(templateCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a template attribute
            InventoryAttributeGroupTemplateAttributePOSTRequest attributePostReq = new InventoryAttributeGroupTemplateAttributePOSTRequest()
            {
                AttributeGroupTemplateID = templateCreateRes.AttributeGroupTemplateID,
                Name = RandomString(5),
                AttributeType = 1
            };
            InventoryAttributeGroupTemplateAttribute attributePostRes = await Client.PostAsync(attributePostReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Read all template attributes
            InventoryAttributeGroupTemplateAttributesGETManyRequest attributesGETManyReq = new InventoryAttributeGroupTemplateAttributesGETManyRequest()
            {
                AttributeGroupTemplateID = templateCreateRes.AttributeGroupTemplateID
            };
            List<InventoryAttributeGroupTemplateAttribute> attributesGETManyRes = await Client.GetAsync(attributesGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(attributesGETManyRes.Any(x => x.TemplateAttributeID == attributePostRes.TemplateAttributeID), Is.True);

            // Read the created template attribute
            InventoryAttributeGroupTemplateAttributeGETRequest attributeGETReq = new InventoryAttributeGroupTemplateAttributeGETRequest()
            {
                AttributeGroupTemplateID = templateCreateRes.AttributeGroupTemplateID,
                TemplateAttributeID = attributePostRes.TemplateAttributeID
            };
            InventoryAttributeGroupTemplateAttribute attributeGETRes = await Client.GetAsync(attributeGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(attributeGETRes.TemplateAttributeID, Is.EqualTo(attributePostRes.TemplateAttributeID));

            // Update the template attribute
            InventoryAttributeGroupTemplateAttributePATCHRequest attributePATCHReq = new InventoryAttributeGroupTemplateAttributePATCHRequest()
            {
                AttributeGroupTemplateID = templateCreateRes.AttributeGroupTemplateID,
                TemplateAttributeID = attributePostRes.TemplateAttributeID,
                Name = "Updated " + RandomString(5)
            };
            InventoryAttributeGroupTemplateAttribute attributePATCHRes = await Client.PatchAsync(attributePATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(attributePATCHRes.TemplateAttributeID, Is.EqualTo(attributePostRes.TemplateAttributeID));

            // Delete the template attribute
            InventoryAttributeGroupTemplateAttributeDELETERequest attributeDELETEReq = new InventoryAttributeGroupTemplateAttributeDELETERequest()
            {
                AttributeGroupTemplateID = templateCreateRes.AttributeGroupTemplateID,
                TemplateAttributeID = attributePostRes.TemplateAttributeID
            };
            await Client.DeleteAsync(attributeDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Remove cache entry for the attribute group template (internal endpoint)
            InventoryAttributeGroupTemplateCACHEDELETERequest cacheDeleteReq = new InventoryAttributeGroupTemplateCACHEDELETERequest()
            {
                AttributeGroupTemplateID = templateCreateRes.AttributeGroupTemplateID
            };
            WebServiceException cacheEx = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                await Client.DeleteAsync(cacheDeleteReq);
            });
            Assert.That(cacheEx.ErrorMessage, Does.Contain("Invalid ClientKey"));

            // Delete the template
            InventoryAttributeGroupTemplateDELETERequest templateDeleteReq = new InventoryAttributeGroupTemplateDELETERequest()
            {
                AttributeGroupTemplateID = templateCreateRes.AttributeGroupTemplateID
            };
            await Client.DeleteAsync(templateDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion

        #region "Queries"
        [Test]
        public async Task IN_AttributeGroupTemplateQuery()
        {
            // Create an inventory attribute group template with 3 attributes to we know the query can return something
            InventoryAttributeGroupTemplatePOSTRequest attributeGroupTemplateCreateReq = new InventoryAttributeGroupTemplatePOSTRequest()
            {
                Name = RandomString(5),
                TemplateAttributes = new List<InventoryAttributeGroupTemplateAttribute>()
                {
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 0, ItemNo = 1 },
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 1 },
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 2 },
                }
            };

            InventoryAttributeGroupTemplate attributeGroupTemplateCreateRes = await Client.PostAsync(attributeGroupTemplateCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(attributeGroupTemplateCreateRes.Name, Is.EqualTo(attributeGroupTemplateCreateReq.Name));
            Assert.That(attributeGroupTemplateCreateRes.AttributeGroupTemplateID, !Is.Null);
            Assert.That(attributeGroupTemplateCreateRes.TemplateAttributes.Count, Is.EqualTo(attributeGroupTemplateCreateReq.TemplateAttributes.Count));

            IN_AttributeGroupTemplateQuery IN_AttributeGroupTemplateQueryRequest = new IN_AttributeGroupTemplateQuery();            
            ServiceStack.QueryResponse<IN_AttributeGroupTemplate> IN_AttributeGroupTemplateQueryResponse;

            //Read all inventory attribute group templates
            IN_AttributeGroupTemplateQueryResponse = await Client.GetAsync(IN_AttributeGroupTemplateQueryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));            
            Assert.That(IN_AttributeGroupTemplateQueryResponse.Results.Count > 0);

            // Now try with filtering to a single item we know exists
            IN_AttributeGroupTemplateQueryRequest.Name = attributeGroupTemplateCreateReq.Name;
            IN_AttributeGroupTemplateQueryResponse = await Client.GetAsync(IN_AttributeGroupTemplateQueryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(IN_AttributeGroupTemplateQueryResponse.Results.Count == 1);

            // Try with an invalid APIKey on to make sure we get a 401
            // Need to use a new client for this, as existing session Id's cookied will bind us to the session from
            // previous requests
            using (ServiceStack.JsonApiClient tempClient = new ServiceStack.JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                var ex = Assert.Throws<ServiceStack.WebServiceException>(() => IN_AttributeGroupTemplateQueryResponse = tempClient.Get(IN_AttributeGroupTemplateQueryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }
        }
        #endregion 
    }
}

