using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class Component : InventoryTestBase
    {
        #region "Components"
        [Test]
        public async Task InventoryItem_Components()
        {
            // Create inventory items for the component routes.
            InventoryItem item = await CreateInventoryItemAsync("Component Parent");
            InventoryItem linkedItem = await CreateInventoryItemAsync("Component Linked");

            // Read all components for the inventory item.
            var components = await Client.GetAsync(new InventoryComponentsGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Read and update the first existing component when one is returned.
            if (components.Count > 0)
            {
                string componentId = ReadString(components[0], "ComponentID");
                await Client.GetAsync(new InventoryComponentGETRequest() { InventoryID = item.InventoryID, ComponentID = componentId });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

                await Client.PatchAsync(new InventoryComponentPATCHRequest() { InventoryID = item.InventoryID, ComponentID = componentId });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            }

            // Create a component for the inventory item.
            await Client.PostAsync(new InventoryComponentPOSTRequest() { InventoryID = item.InventoryID, ComponentInventoryID = linkedItem.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Delete the temporary inventory items.
            await DeleteInventoryItemAsync(item.InventoryID);
            await DeleteInventoryItemAsync(linkedItem.InventoryID);
        }
        #endregion
    }
}

