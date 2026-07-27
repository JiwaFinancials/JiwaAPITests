using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using ServiceStack;
using System.Threading.Tasks;

namespace JiwaAPITests.Creditors
{
    public class WarehouseAddress : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Creditor_WarehouseAddress_CRUD()
        {
            // Create a creditor to append a warehouse address to
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Creditor Warehouse Address Test"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);

            // Create a warehouse address
            CreditorWarehouseAddressPOSTRequest warehouseAddressCreateReq = new CreditorWarehouseAddressPOSTRequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                Description = "Warehouse " + RandomString(8),
                Address1 = "Address1",
                Address2 = "Address2",
                Address3 = "Address3",
                Address4 = "Address4",
                Postcode = "3000",
                Country = "AU",
                Notes = "Initial notes",
                CourierDetails = "Initial courier details",
                DefaultDeliveryDays = 2,
                IsDefault = true
            };

            CreditorWarehouseAddress warehouseAddressCreateRes = await Client.PostAsync(warehouseAddressCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(warehouseAddressCreateRes.WarehouseAddressID, Is.Not.Null);
            Assert.That(warehouseAddressCreateRes.Description, Is.EqualTo(warehouseAddressCreateReq.Description));

            // Read the created warehouse address
            CreditorWarehouseAddressGETRequest warehouseAddressGetReq = new CreditorWarehouseAddressGETRequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                WarehouseAddressID = warehouseAddressCreateRes.WarehouseAddressID
            };

            CreditorWarehouseAddress warehouseAddressGetRes = await Client.GetAsync(warehouseAddressGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(warehouseAddressGetRes.WarehouseAddressID, Is.EqualTo(warehouseAddressCreateRes.WarehouseAddressID));
            Assert.That(warehouseAddressGetRes.Description, Is.EqualTo(warehouseAddressCreateReq.Description));

            // Update the warehouse address
            CreditorWarehouseAddressPATCHRequest warehouseAddressPatchReq = new CreditorWarehouseAddressPATCHRequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                WarehouseAddressID = warehouseAddressCreateRes.WarehouseAddressID,
                Description = "Updated Warehouse " + RandomString(6),
                Notes = "Updated notes",
                DefaultDeliveryDays = 5,
                IsDefault = true
            };

            CreditorWarehouseAddress warehouseAddressPatchRes = await Client.PatchAsync(warehouseAddressPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(warehouseAddressPatchRes.WarehouseAddressID, Is.EqualTo(warehouseAddressCreateRes.WarehouseAddressID));
            Assert.That(warehouseAddressPatchRes.Description, Is.EqualTo(warehouseAddressPatchReq.Description));
            Assert.That(warehouseAddressPatchRes.IsDefault, Is.EqualTo(warehouseAddressPatchReq.IsDefault));

            // Read the updated warehouse address
            warehouseAddressGetRes = await Client.GetAsync(warehouseAddressGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(warehouseAddressGetRes.Description, Is.EqualTo(warehouseAddressPatchReq.Description));

            // Create a second warehouse address and make it default so the first can be deleted
            CreditorWarehouseAddressPOSTRequest secondWarehouseAddressCreateReq = new CreditorWarehouseAddressPOSTRequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                Description = "Warehouse Default " + RandomString(6),
                Address1 = "Default Address1",
                Postcode = "3001",
                Country = "AU",
                IsDefault = true
            };

            CreditorWarehouseAddress secondWarehouseAddressCreateRes = await Client.PostAsync(secondWarehouseAddressCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(secondWarehouseAddressCreateRes.WarehouseAddressID, Is.Not.Null);

            // Remove the created warehouse address
            CreditorWarehouseAddressDELETERequest warehouseAddressDeleteReq = new CreditorWarehouseAddressDELETERequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                WarehouseAddressID = warehouseAddressCreateRes.WarehouseAddressID
            };

            await Client.DeleteAsync(warehouseAddressDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted warehouse address is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                CreditorWarehouseAddress getDeletedRes = await Client.GetAsync(warehouseAddressGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Remove the created creditor
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

