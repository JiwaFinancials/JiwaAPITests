using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace JiwaAPITests.Inventory
{
    public class Price : InventoryTestBase
    {
        #region "Price"
        [Test]
        public async Task InventoryItem_Price()
        {
            // Create a debtor
            DebtorPOSTRequest accountCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor accountCreateRes = await Client.PostAsync(accountCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(accountCreateRes.AccountNo, Is.EqualTo(accountCreateReq.AccountNo));
            Assert.That(accountCreateRes.DebtorID, !Is.Null);

            // Create an inventory item for the price route.
            InventoryItem item = await CreateInventoryItemAsync("Price Item");

            // Read the calculated price for the inventory item.
            await Client.GetAsync(new InventoryPriceGETRequest()
            {
                InventoryID = item.InventoryID,
                DebtorID = accountCreateRes.DebtorID,
                IN_LogicalID = "MAIN",
                Date = DateTime.Today,
                Quantity = 1
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Delete the temporary inventory item.
            await DeleteInventoryItemAsync(item.InventoryID);
        }
        #endregion
    }
}

