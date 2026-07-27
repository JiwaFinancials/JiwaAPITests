using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class UnitOfMeasure : InventoryTestBase
    {
        #region "Unit Of Measures"
        [Test]
        public async Task InventoryItem_UnitOfMeasures()
        {
            // Create an inventory item for the unit of measure routes.
            InventoryItem item = await CreateInventoryItemAsync("Unit Of Measure Item");

            // Read all unit of measures for the inventory item.
            await Client.GetAsync(new InventoryUnitOfMeasuresGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Create a unit of measure for the inventory item.
            await Client.PostAsync(new InventoryUnitOfMeasurePOSTRequest() { InventoryID = item.InventoryID, QuantityInnersPerUnitOfMeasure = 6, Name = "Half Dozen" });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}

