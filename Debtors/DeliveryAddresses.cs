using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Debtors
{
    public class DeliveryAddresses : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task DebtorDeliveryAddress_CRUD()
        {
            // Create a debtor to associate with the delivery address
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

            // Create a delivery address
            DebtorDeliveryAddressPOSTRequest addressCreateReq = new DebtorDeliveryAddressPOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressName = "Main Address"
            };

            DebtorDeliveryAddress addressCreateRes = await Client.PostAsync(addressCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(addressCreateRes.DeliveryAddressID, !Is.Null);
            Assert.That(addressCreateRes.DeliveryAddressName, Is.EqualTo(addressCreateReq.DeliveryAddressName));

            // Create a second delivery address and make it the default
            DebtorDeliveryAddressPOSTRequest addressCreateReq2 = new DebtorDeliveryAddressPOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressName = "Real Main Address",
                IsDefault = true
            };

            DebtorDeliveryAddress addressCreateRes2 = await Client.PostAsync(addressCreateReq2);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(addressCreateRes2.DeliveryAddressID, !Is.Null);
            Assert.That(addressCreateRes2.DeliveryAddressName, Is.EqualTo(addressCreateReq2.DeliveryAddressName));

            // Read the first created delivery address using the DeliveryAddressID
            DebtorDeliveryAddressGETRequest addressGetReq = new DebtorDeliveryAddressGETRequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = addressCreateRes.DeliveryAddressID 
            };
            DebtorDeliveryAddress addressGetRes = await Client.GetAsync(addressGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(addressGetRes.DeliveryAddressName, Is.EqualTo(addressCreateReq.DeliveryAddressName));

            // Update the delivery address
            DebtorDeliveryAddressPATCHRequest addressPatchReq = new DebtorDeliveryAddressPATCHRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = addressCreateRes.DeliveryAddressID,
                DeliveryAddressName = "Secondary Address"
            };
            DebtorDeliveryAddress addressPatchRes = await Client.PatchAsync(addressPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(addressPatchRes.DeliveryAddressName, Is.EqualTo(addressPatchReq.DeliveryAddressName));

            // Delete the delivery address
            DebtorDeliveryAddressDELETERequest addressDeleteReq = new DebtorDeliveryAddressDELETERequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = addressCreateRes.DeliveryAddressID 
            };
            await Client.DeleteAsync(addressDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted address is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                DebtorDeliveryAddress getDeletedRes = await Client.GetAsync(addressGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Clean up the test debtor
            DebtorDELETERequest debtorDeleteReq = new DebtorDELETERequest() { DebtorID = debtorCreateRes.DebtorID };
            await Client.DeleteAsync(debtorDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }

        [Test]
        public async Task DebtorDeliveryAddresses_GetMany()
        {
            // Create a debtor to associate with delivery addresses
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a few delivery addresses
            List<DebtorDeliveryAddress> createdAddresses = new List<DebtorDeliveryAddress>();
            for (int i = 0; i < 2; i++)
            {
                DebtorDeliveryAddressPOSTRequest addressCreateReq = new DebtorDeliveryAddressPOSTRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    DeliveryAddressName = $"Address {i}"
                };

                DebtorDeliveryAddress addressCreateRes = await Client.PostAsync(addressCreateReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                createdAddresses.Add(addressCreateRes);
            }

            // Get the list of delivery addresses
            DebtorDeliveryAddressesGETManyRequest addressesGetManyReq = new DebtorDeliveryAddressesGETManyRequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID 
            };
            List<DebtorDeliveryAddress> addressesGetManyRes = await Client.GetAsync(addressesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(addressesGetManyRes.Count, Is.GreaterThanOrEqualTo(2));

            // Clean up - delete the addresses except for the default
            foreach (var address in createdAddresses)
            {
                if(address.IsDefault == false)
                {
                    DebtorDeliveryAddressDELETERequest addressDeleteReq = new DebtorDeliveryAddressDELETERequest()
                    {
                        DebtorID = debtorCreateRes.DebtorID,
                        DeliveryAddressID = address.DeliveryAddressID
                    };
                    await Client.DeleteAsync(addressDeleteReq);
                    Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
                }
            }

            // Clean up - delete the debtor
            DebtorDELETERequest debtorDeleteReq = new DebtorDELETERequest() { DebtorID = debtorCreateRes.DebtorID };
            await Client.DeleteAsync(debtorDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

