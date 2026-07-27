using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.Creditors
{
    public class WarehouseAddresses : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Creditor_WarehouseAddresses_GETMany()
        {
            // Create a creditor
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Creditor Warehouse Addresses Test"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);

            // Append a warehouse address
            CreditorWarehouseAddressPOSTRequest warehouseAddressCreateReq = new CreditorWarehouseAddressPOSTRequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                Description = "Warehouse List " + RandomString(8),
                Address1 = "Address1",
                Postcode = "3000",
                Country = "AU"
            };

            CreditorWarehouseAddress warehouseAddressCreateRes = await Client.PostAsync(warehouseAddressCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(warehouseAddressCreateRes.WarehouseAddressID, Is.Not.Null);

            // Read warehouse addresses for the creditor
            CreditorWarehouseAddressesGETManyRequest warehouseAddressesGetManyReq = new CreditorWarehouseAddressesGETManyRequest()
            {
                CreditorID = creditorCreateRes.CreditorID
            };

            List<CreditorWarehouseAddress> warehouseAddressesGetManyRes = await Client.GetAsync(warehouseAddressesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(warehouseAddressesGetManyRes.Any(x => x.WarehouseAddressID == warehouseAddressCreateRes.WarehouseAddressID), Is.True);

            // Cleanup
            CreditorWarehouseAddressDELETERequest warehouseAddressDeleteReq = new CreditorWarehouseAddressDELETERequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                WarehouseAddressID = warehouseAddressCreateRes.WarehouseAddressID
            };
            await Client.DeleteAsync(warehouseAddressDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            CreditorDELETERequest creditorDeleteReq = new CreditorDELETERequest()
            {
                CreditorID = creditorCreateRes.CreditorID
            };
            await Client.DeleteAsync(creditorDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

