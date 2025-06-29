﻿using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using JiwaFinancials.Jiwa.JiwaServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ServiceStack.Diagnostics.Events;
using NUnit.Framework;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using ServiceStack;

namespace JiwaAPITests.Inventory
{
    [TestFixture]
    class Item : JiwaAPITest
    {
        #region "{Main}"
        [Test]       
        public async Task InventoryItem_CRUD()
        {
            // Create an item
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest() 
            { 
                PartNo = RandomString(5), 
                Description = "Item Test",
                DefaultPrice = 125.67M 
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.PartNo, Is.EqualTo(itemCreateReq.PartNo));
            Assert.That(itemCreateRes.InventoryID, !Is.Null);

            // Read the created item using the inventory ID
            InventoryGETRequest itemGetReq = new InventoryGETRequest() { InventoryID = itemCreateRes.InventoryID };
            InventoryItem itemGetRes = await Client.GetAsync(itemGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(itemGetRes.PartNo, Is.EqualTo(itemCreateReq.PartNo));
            Assert.That(itemGetRes.Description, Is.EqualTo(itemCreateReq.Description));

            // Update the inventory item
            InventoryPATCHRequest itemPatchReq = new InventoryPATCHRequest() 
            { 
                InventoryID = itemCreateRes.InventoryID, 
                Description = "Updated Item Test", 
                DefaultPrice = 321.45M 
            };
            InventoryItem itemPatchRes =  await Client.PatchAsync(itemPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(itemPatchRes.Description, Is.EqualTo(itemPatchReq.Description));
            Assert.That(itemPatchRes.DefaultPrice, Is.EqualTo(itemPatchReq.DefaultPrice));

            // Remove the created item
            InventoryDELETERequest itemDeleteReq = new InventoryDELETERequest() { InventoryID = itemCreateRes.InventoryID };
            await Client.DeleteAsync(itemDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // ensure the deleted item is not there anymore            
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                InventoryItem getDeletedRes = await Client.GetAsync(itemGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Try to GET non-existent item to make sure we get a 404
            itemGetReq.InventoryID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                InventoryItem itemGetRes = await Client.GetAsync(itemGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "Alternate Children"
        [Test]
        public async Task InventoryItem_AlternateChildren_CRUD()
        {
            // Create an item we can operate on
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Item Test",
                DefaultPrice = 125.67M
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.PartNo, Is.EqualTo(itemCreateReq.PartNo));
            Assert.That(itemCreateRes.InventoryID, !Is.Null);

            // Create a second item we can add as an Alternate Child
            InventoryPOSTRequest alternateChildItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Child Item Test",
                DefaultPrice = 321.45M
            };

            InventoryItem alternateChildItemCreateRes = await Client.PostAsync(alternateChildItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));    

            // Add an Alternate child to the item
            InventoryAlternateChildPOSTRequest alternateChildPOSTReq = new InventoryAlternateChildPOSTRequest()
            {
                InventoryID = itemCreateRes.InventoryID,
                LinkedInventoryID = alternateChildItemCreateRes.InventoryID,
                Notes = "Alternate Child Test"
            };
            InventoryAlternateChild alternateChildPOSTRes = await Client.PostAsync(alternateChildPOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(alternateChildPOSTRes.LinkedInventoryID, Is.EqualTo(alternateChildItemCreateRes.InventoryID));
            Assert.That(alternateChildPOSTRes.LinkedInventoryPartNo, Is.EqualTo(alternateChildItemCreateRes.PartNo));
            Assert.That(alternateChildPOSTRes.LinkedInventoryDescription, Is.EqualTo(alternateChildItemCreateRes.Description));
            Assert.That(alternateChildPOSTRes.Notes, Is.EqualTo(alternateChildPOSTRes.Notes));

            // Check to see if the inventory alternate child is present via a GET 
            InventoryAlternateChildGETRequest alternateChildGETReq = new InventoryAlternateChildGETRequest() 
            {                 
                InventoryID = itemCreateRes.InventoryID,
                AlternateChildID = alternateChildPOSTRes.AlternateChildID
            };

            InventoryAlternateChild InventoryAlternateChildGETRes = await Client.GetAsync(alternateChildGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(InventoryAlternateChildGETRes.LinkedInventoryID, Is.EqualTo(alternateChildItemCreateRes.InventoryID));
            Assert.That(InventoryAlternateChildGETRes.LinkedInventoryPartNo, Is.EqualTo(alternateChildItemCreateRes.PartNo));
            Assert.That(InventoryAlternateChildGETRes.LinkedInventoryDescription, Is.EqualTo(alternateChildItemCreateRes.Description));
            Assert.That(InventoryAlternateChildGETRes.Notes, Is.EqualTo(alternateChildPOSTRes.Notes));

            // Try also the GET Many - should return the single item we added
            InventoryAlternateChildrenGETManyRequest alternateChildGETManyReq = new InventoryAlternateChildrenGETManyRequest()
            {
                InventoryID = itemCreateRes.InventoryID
            };

            List<InventoryAlternateChild> InventoryAlternateChildGETManyRes = await Client.GetAsync(alternateChildGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(InventoryAlternateChildGETManyRes.Count, Is.GreaterThan(0));

            // Try patching the item
            InventoryAlternateChildPATCHRequest InventoryAlternateChildPATCHReq = new InventoryAlternateChildPATCHRequest()
            {
                 InventoryID = itemCreateRes.InventoryID,
                 AlternateChildID = alternateChildPOSTRes.AlternateChildID,
                 Notes = "Updated Notes"
            };
            InventoryAlternateChild InventoryAlternateChildPATCHRes = await Client.PatchAsync(InventoryAlternateChildPATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(InventoryAlternateChildPATCHRes.Notes, Is.EqualTo(InventoryAlternateChildPATCHReq.Notes));

            // Get the patched item and ensure it matches what we patched
            InventoryAlternateChildGETRes = await Client.GetAsync(alternateChildGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(InventoryAlternateChildGETRes.Notes, Is.EqualTo(InventoryAlternateChildPATCHReq.Notes));

            // Remove the alternate child we added
            InventoryAlternateChildDELETERequest alternateChildDELETEReq = new InventoryAlternateChildDELETERequest()
            {
                InventoryID = itemCreateRes.InventoryID,
                AlternateChildID = alternateChildPOSTRes.AlternateChildID
            };
            await Client.DeleteAsync(alternateChildDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the child is no longer present in the list of children for the item
            InventoryAlternateChildGETManyRes = await Client.GetAsync(alternateChildGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(InventoryAlternateChildGETManyRes.Count, Is.EqualTo(0));

            // Ensure explicitly requesting the child 404's
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                InventoryAlternateChildGETRes = await Client.GetAsync(alternateChildGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Remove the test alternate child item we created
            InventoryDELETERequest itemDeleteReq = new InventoryDELETERequest() 
            { 
                InventoryID = itemCreateRes.InventoryID 
            };
            await Client.DeleteAsync(itemDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Remove the test item we created
            itemDeleteReq = new InventoryDELETERequest() 
            { 
                InventoryID = alternateChildItemCreateRes.InventoryID 
            };
            await Client.DeleteAsync(itemDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion

        #region "Alternate Parents"
        [Test]
        public async Task InventoryItem_AlternateParents_CRUD()
        {
            // Create an item we can operate on
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Item Test",
                DefaultPrice = 125.67M
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.PartNo, Is.EqualTo(itemCreateReq.PartNo));
            Assert.That(itemCreateRes.InventoryID, !Is.Null);

            // Create a second item we can add as an Alternate Child
            InventoryPOSTRequest alternateChildItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Child Item Test",
                DefaultPrice = 321.45M
            };

            InventoryItem alternateChildItemCreateRes = await Client.PostAsync(alternateChildItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Add an Alternate child to the item
            InventoryAlternateChildPOSTRequest alternateChildPOSTReq = new InventoryAlternateChildPOSTRequest()
            {
                InventoryID = itemCreateRes.InventoryID,
                LinkedInventoryID = alternateChildItemCreateRes.InventoryID,
                Notes = "Alternate Parent Test"
            };
            InventoryAlternateChild alternateChildPOSTRes = await Client.PostAsync(alternateChildPOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Check to see if the inventory alternate parent is present via a GET 
            InventoryAlternateParentGETRequest alternateParentGETReq = new InventoryAlternateParentGETRequest()
            {
                InventoryID = alternateChildItemCreateRes.InventoryID,
                LinkedInventoryID = itemCreateRes.InventoryID
            };

            InventoryAlternateParent InventoryAlternateParentGETRes = await Client.GetAsync(alternateParentGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            Assert.That(InventoryAlternateParentGETRes.LinkedInventoryID, Is.EqualTo(itemCreateRes.InventoryID));
            Assert.That(InventoryAlternateParentGETRes.LinkedInventoryPartNo, Is.EqualTo(itemCreateRes.PartNo));
            Assert.That(InventoryAlternateParentGETRes.LinkedInventoryDescription, Is.EqualTo(itemCreateRes.Description));

            // Try also the GET Many - should return the single item we added
            InventoryAlternateParentsGETManyRequest alternateParentGETManyReq = new InventoryAlternateParentsGETManyRequest()
            {
                InventoryID = alternateChildItemCreateRes.InventoryID
            };
            List<InventoryAlternateParent> alternateParentGETManyRes = await Client.GetAsync(alternateParentGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(alternateParentGETManyRes.Count, Is.GreaterThan(0));

            // Remove the alternate child we added
            InventoryAlternateChildDELETERequest alternateChildDELETEReq = new InventoryAlternateChildDELETERequest()
            {
                InventoryID = itemCreateRes.InventoryID,
                AlternateChildID = alternateChildPOSTRes.AlternateChildID
            };
            await Client.DeleteAsync(alternateChildDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the parent is no longer present in the list of parents when looking at the child item
            alternateParentGETManyRes = await Client.GetAsync(alternateParentGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(alternateParentGETManyRes.Count, Is.EqualTo(0));

            // Ensure explicitly requesting the child 404's
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                InventoryAlternateParentGETRes = await Client.GetAsync(alternateParentGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Remove the test alternate child item we created
            InventoryDELETERequest itemDeleteReq = new InventoryDELETERequest()
            {
                InventoryID = itemCreateRes.InventoryID
            };
            await Client.DeleteAsync(itemDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Remove the test item we created
            itemDeleteReq = new InventoryDELETERequest()
            {
                InventoryID = alternateChildItemCreateRes.InventoryID
            };
            await Client.DeleteAsync(itemDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion

        #region "Attribute Groups"
        [Test]
        public async Task InventoryItem_AttributeGroups_CRUD()
        {
            // Create an inventory attribute group template with 3 attributes
            InventoryAttributeGroupTemplatePOSTRequest attributeGroupTemplateCreateReq = new InventoryAttributeGroupTemplatePOSTRequest()
            {
                Name = RandomString(5),
                TemplateAttributes = new List<InventoryAttributeGroupTemplateAttribute>()
                {
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 1, ItemNo = 1 },
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 0 },
                    new InventoryAttributeGroupTemplateAttribute() { Name = RandomString(5), AttributeType = 2 },
                }
            };

            InventoryAttributeGroupTemplate attributeGroupTemplateCreateRes = await Client.PostAsync(attributeGroupTemplateCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(attributeGroupTemplateCreateRes.Name, Is.EqualTo(attributeGroupTemplateCreateReq.Name));
            Assert.That(attributeGroupTemplateCreateRes.AttributeGroupTemplateID, !Is.Null);
            Assert.That(attributeGroupTemplateCreateRes.TemplateAttributes.Count, Is.EqualTo(attributeGroupTemplateCreateReq.TemplateAttributes.Count));

            // Create an inventory item we can operate on with the attribute group template included
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Item Test",
                DefaultPrice = 125.67M,
                AttributeGroups = new List<InventoryAttributeGroup>() { new InventoryAttributeGroup() { Template = attributeGroupTemplateCreateRes } }
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.PartNo, Is.EqualTo(itemCreateReq.PartNo));
            Assert.That(itemCreateRes.InventoryID, !Is.Null);
            Assert.That(itemCreateRes.AttributeGroups, !Is.Null);
            Assert.That(itemCreateRes.AttributeGroups.Count, Is.EqualTo(1));

            // Get all the attribute groups for the item
            InventoryAttributeGroupsGETManyRequest inventoryAttributeGroupsGETManyReq = new InventoryAttributeGroupsGETManyRequest() { InventoryID = itemCreateRes.InventoryID };
            List<InventoryAttributeGroup> inventoryAttributeGroupsGETManyRes = await Client.GetAsync(inventoryAttributeGroupsGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(inventoryAttributeGroupsGETManyRes, !Is.Null);
            Assert.That(inventoryAttributeGroupsGETManyRes.Count, Is.EqualTo(1));

            // Get a single attribute group from the item
            InventoryAttributeGroupGETRequest inventoryAttributeGroupGETReq = new InventoryAttributeGroupGETRequest() { InventoryID = itemCreateRes.InventoryID, AttributeGroupID = itemCreateRes.AttributeGroups[0].AttributeGroupID };
            InventoryAttributeGroup inventoryAttributeGroupGETRes = await Client.GetAsync(inventoryAttributeGroupGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(inventoryAttributeGroupGETRes, !Is.Null);
            Assert.That(inventoryAttributeGroupGETRes.AttributeGroupID, Is.EqualTo(itemCreateRes.AttributeGroups[0].AttributeGroupID));
            Assert.That(inventoryAttributeGroupGETRes.Template.AttributeGroupTemplateID, Is.EqualTo(itemCreateRes.AttributeGroups[0].Template.AttributeGroupTemplateID));

            // Try patching the item
            InventoryAttributeGroupPATCHRequest inventoryAttributeGroupPATCHReq = new InventoryAttributeGroupPATCHRequest()
            {
                InventoryID = itemCreateRes.InventoryID,
                AttributeGroupID = itemCreateRes.AttributeGroups[0].AttributeGroupID,
                Description = "Test Description"                
            };
            // And set the contents of the first attribute group field
            inventoryAttributeGroupPATCHReq.Attributes.Add(new InventoryAttributeGroupAttribute() { AttributeID = inventoryAttributeGroupGETRes.Attributes[0].AttributeID, Contents = "Test" });
            InventoryAttributeGroup InventoryAlternateChildPATCHRes = await Client.PatchAsync(inventoryAttributeGroupPATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(inventoryAttributeGroupPATCHReq.Description, Is.EqualTo(inventoryAttributeGroupPATCHReq.Description));

            // Get the patched item and ensure it matches what we just patched            
            inventoryAttributeGroupGETRes = await Client.GetAsync(inventoryAttributeGroupGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(inventoryAttributeGroupGETRes.Description, Is.EqualTo(inventoryAttributeGroupPATCHReq.Description));            
            Assert.That(inventoryAttributeGroupGETRes.Attributes[0].Contents, Is.EqualTo(inventoryAttributeGroupPATCHReq.Attributes[0].Contents));

            // Remove the attribute group we added
            InventoryAttributeGroupDELETERequest InventoryAttributeGroupDELETEReq = new InventoryAttributeGroupDELETERequest()
            {
                InventoryID = itemCreateRes.InventoryID,
                AttributeGroupID = itemCreateRes.AttributeGroups[0].AttributeGroupID
            };
            await Client.DeleteAsync(InventoryAttributeGroupDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the child is no longer present in the list of children for the item
            inventoryAttributeGroupsGETManyRes = await Client.GetAsync(inventoryAttributeGroupsGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(inventoryAttributeGroupsGETManyRes.Count, Is.EqualTo(0));

            // Ensure explicitly requesting the attribute group 404's
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                inventoryAttributeGroupGETRes = await Client.GetAsync(inventoryAttributeGroupGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Add the group again using the explicit route, instead of bundling it in with the item create 
            InventoryAttributeGroupPOSTRequest inventoryAttributeGroupPOSTReq = new InventoryAttributeGroupPOSTRequest()
            {
                InventoryID = itemCreateRes.InventoryID,
                Template = new InventoryAttributeGroupTemplate() { AttributeGroupTemplateID = attributeGroupTemplateCreateRes.AttributeGroupTemplateID }
            };
            InventoryAttributeGroup inventoryAttributeGroupPOSTRes = await Client.PostAsync(inventoryAttributeGroupPOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inventoryAttributeGroupPOSTRes, !Is.Null);

            // Check that the group is indeed in the list of attribute groups
            inventoryAttributeGroupsGETManyRes = await Client.GetAsync(inventoryAttributeGroupsGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(inventoryAttributeGroupsGETManyRes, !Is.Null);
            Assert.That(inventoryAttributeGroupsGETManyRes.Count, Is.EqualTo(1));
            Assert.That(inventoryAttributeGroupsGETManyRes[0].Template.AttributeGroupTemplateID, Is.EqualTo(attributeGroupTemplateCreateRes.AttributeGroupTemplateID));

            // Remove the test attribute group template item we created
            InventoryAttributeGroupTemplateDELETERequest inventoryAttributeGroupTemplateDELETEReq = new InventoryAttributeGroupTemplateDELETERequest()
            {                
                 AttributeGroupTemplateID = attributeGroupTemplateCreateRes.AttributeGroupTemplateID
            };
            await Client.DeleteAsync(inventoryAttributeGroupTemplateDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Remove the test inventory item we created
            InventoryDELETERequest itemDeleteReq = new InventoryDELETERequest()
            {
                InventoryID = itemCreateRes.InventoryID
            };
            await Client.DeleteAsync(itemDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion        

        #region "Queries"
        [Test]
        public async Task IN_MainQuery()
        {
            IN_MainQuery IN_MainQueryRequest = new IN_MainQuery();
            ServiceStack.QueryResponse<IN_Main> IN_MainQueryResponse;

            //Read all inventory items            
            IN_MainQueryResponse = await Client.GetAsync(IN_MainQueryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Let's assume we expect to get at least one inventory item back - demo data has many inventory items.
            Assert.That(IN_MainQueryResponse.Results.Count > 0);

            // Try with an invalid APIKey on to make sure we get a 401
            // Need to use a new client for this, as existing session Id's cookied will bind us to the session from
            // previous requests
            using (ServiceStack.JsonApiClient tempClient = new ServiceStack.JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                var ex = Assert.Throws<ServiceStack.WebServiceException>(() => IN_MainQueryResponse = tempClient.Get(IN_MainQueryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }                
        }

        [Test]
        public async Task v_Jiwa_Inventory_Item_ListQuery()
        {
            // get first 10 parts
            v_Jiwa_Inventory_Item_ListQuery v_Jiwa_Inventory_Item_ListQueryRequest = new v_Jiwa_Inventory_Item_ListQuery() 
            { 
                Take = 10, 
                OrderBy = "PartNo" 
            };
            ServiceStack.QueryResponse<v_Jiwa_Inventory_Item_List> v_Jiwa_Inventory_Item_ListQueryResponse;
            
            v_Jiwa_Inventory_Item_ListQueryResponse = await Client.GetAsync(v_Jiwa_Inventory_Item_ListQueryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Ensure we got only the 10 we asked for
            Assert.That(v_Jiwa_Inventory_Item_ListQueryResponse.Results.Count == 10);

            // Try with an invalid APIKey on to make sure we get a 401
            // Need to use a new client for this, as existing session Id's cookied will bind us to the session from
            // previous requests
            using (ServiceStack.JsonApiClient tempClient = new ServiceStack.JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                var ex = Assert.Throws<ServiceStack.WebServiceException>(() => v_Jiwa_Inventory_Item_ListQueryResponse = tempClient.Get(v_Jiwa_Inventory_Item_ListQueryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }
        }
        
        #endregion
    }
}
