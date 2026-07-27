using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class Supplier : InventoryTestBase
    {
        #region "Suppliers"
        [Test]
        public async Task InventoryItem_Suppliers()
        {
            // Create an inventory item for the supplier routes.
            InventoryItem item = await CreateInventoryItemAsync("Supplier Item");

            // Read all regions so a supplier route can be targeted.
            var regions = await Client.GetAsync(new InventoryRegionsGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Read and update the first existing supplier when a region and supplier are returned.
            if (regions.Count > 0)
            {
                string regionName = ReadString(regions[0], "RegionName");
                var suppliers = await Client.GetAsync(new InventorySuppliersGETManyRequest() { InventoryID = item.InventoryID, RegionName = regionName });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

                if (suppliers.Count > 0)
                {
                    string supplierId = ReadString(suppliers[0], "SupplierID");
                    await Client.GetAsync(new InventorySupplierGETRequest() { InventoryID = item.InventoryID, RegionName = regionName, SupplierID = supplierId });
                    Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

                    await Client.PatchAsync(new InventorySupplierPATCHRequest() { InventoryID = item.InventoryID, RegionName = regionName, SupplierID = supplierId });
                    Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                }
            }

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}
