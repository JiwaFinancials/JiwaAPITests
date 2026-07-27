using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class SupplierWarehouse : InventoryTestBase
    {
        #region "Supplier Warehouses"
        [Test]
        public async Task InventoryItem_SupplierWarehouses()
        {
            // Create an inventory item for the supplier warehouse routes.
            InventoryItem item = await CreateInventoryItemAsync("Supplier Warehouse Item");

            // Read all regions and suppliers so a supplier warehouse route can be targeted.
            var regions = await Client.GetAsync(new InventoryRegionsGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Read and update the first existing supplier warehouse when a region, supplier and warehouse are returned.
            if (regions.Count > 0)
            {
                string regionName = ReadString(regions[0], "RegionName");
                var suppliers = await Client.GetAsync(new InventorySuppliersGETManyRequest() { InventoryID = item.InventoryID, RegionName = regionName });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

                if (suppliers.Count > 0)
                {
                    string supplierId = ReadString(suppliers[0], "SupplierID");
                    var supplierWarehouses = await Client.GetAsync(new InventorySupplierWarehousesGETManyRequest() { InventoryID = item.InventoryID, RegionName = regionName, SupplierID = supplierId });
                    Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

                    if (supplierWarehouses.Count > 0)
                    {
                        string supplierWarehouseId = ReadString(supplierWarehouses[0], "SupplierWarehouseID");
                        await Client.GetAsync(new InventorySupplierWarehouseGETRequest() { InventoryID = item.InventoryID, RegionName = regionName, SupplierID = supplierId, SupplierWarehouseID = supplierWarehouseId });
                        Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

                        await Client.PatchAsync(new InventorySupplierWarehousePATCHRequest() { InventoryID = item.InventoryID, RegionName = regionName, SupplierID = supplierId, SupplierWarehouseID = supplierWarehouseId });
                        Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                    }
                }
            }

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}
