using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class Ledger : InventoryTestBase
    {
        #region "Ledgers"
        [Test]
        public async Task InventoryItem_Ledgers()
        {
            // Create an inventory item for the ledger routes.
            InventoryItem item = await CreateInventoryItemAsync("Ledger Item");

            // Read all ledgers for the inventory item.
            var ledgers = await Client.GetAsync(new InventoryLedgersGETManyRequest() { InventoryID = item.InventoryID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Read and update the first existing ledger when one is returned.
            if (ledgers.Count > 0)
            {
                string ledgerName = ReadString(ledgers[0], "Name");
                await Client.GetAsync(new InventoryLedgerGETRequest() { InventoryID = item.InventoryID, Name = ledgerName });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

                await Client.PatchAsync(new InventoryLedgerPATCHRequest() { InventoryID = item.InventoryID, Name = ledgerName });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            }

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}
