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
    public class FreightForwarderAddresses : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task DebtorFreightForwarderAddress_CRUD()
        {
            // Create a debtor to associate with the freight forwarder address
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

            // Create a freight forwarder address
            DebtorFreightForwarderAddressPOSTRequest addressCreateReq = new DebtorFreightForwarderAddressPOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                Address1 = "123 Forwarder Lane"
            };

            DebtorFreightForwarderAddress addressCreateRes = await Client.PostAsync(addressCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(addressCreateRes.FreightForwarderAddressID, !Is.Null);
            Assert.That(addressCreateRes.Address1, Is.EqualTo(addressCreateReq.Address1));

            // Read the created freight forwarder address using the FreightForwarderAddressID
            DebtorFreightForwarderAddressGETRequest addressGetReq = new DebtorFreightForwarderAddressGETRequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID,
                FreightForwarderAddressID = addressCreateRes.FreightForwarderAddressID 
            };
            DebtorFreightForwarderAddress addressGetRes = await Client.GetAsync(addressGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(addressGetRes.Address1, Is.EqualTo(addressCreateReq.Address1));

            // Update the freight forwarder address
            DebtorFreightForwarderAddressPATCHRequest addressPatchReq = new DebtorFreightForwarderAddressPATCHRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                FreightForwarderAddressID = addressCreateRes.FreightForwarderAddressID,
                Address1 = "456 Shipping Street"
            };
            DebtorFreightForwarderAddress addressPatchRes = await Client.PatchAsync(addressPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(addressPatchRes.FreightForwarderAddressID, Is.EqualTo(addressPatchReq.FreightForwarderAddressID));
            Assert.That(addressPatchRes.Address1, Is.EqualTo(addressPatchReq.Address1));

            // Delete the freight forwarder address
            DebtorFreightForwarderAddressDELETERequest addressDeleteReq = new DebtorFreightForwarderAddressDELETERequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID,
                FreightForwarderAddressID = addressCreateRes.FreightForwarderAddressID 
            };
            await Client.DeleteAsync(addressDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted address is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                DebtorFreightForwarderAddress getDeletedRes = await Client.GetAsync(addressGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Clean up the test debtor
            DebtorDELETERequest debtorDeleteReq = new DebtorDELETERequest() { DebtorID = debtorCreateRes.DebtorID };
            await Client.DeleteAsync(debtorDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }

        [Test]
        public async Task DebtorFreightForwarderAddresses_GetMany()
        {
            // Create a debtor to associate with freight forwarder addresses
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a few freight forwarder addresses
            List<DebtorFreightForwarderAddress> createdAddresses = new List<DebtorFreightForwarderAddress>();
            for (int i = 0; i < 2; i++)
            {
                DebtorFreightForwarderAddressPOSTRequest addressCreateReq = new DebtorFreightForwarderAddressPOSTRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    Address1 = $"{i * 100} Forwarder Road"
                };

                DebtorFreightForwarderAddress addressCreateRes = await Client.PostAsync(addressCreateReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                createdAddresses.Add(addressCreateRes);
            }

            // Get the list of freight forwarder addresses
            DebtorFreightForwarderAddressesGETManyRequest addressesGetManyReq = new DebtorFreightForwarderAddressesGETManyRequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID 
            };
            List<DebtorFreightForwarderAddress> addressesGetManyRes = await Client.GetAsync(addressesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(addressesGetManyRes.Count, Is.GreaterThanOrEqualTo(2));

            // Clean up - delete the addresses
            foreach (var address in createdAddresses)
            {
                DebtorFreightForwarderAddressDELETERequest addressDeleteReq = new DebtorFreightForwarderAddressDELETERequest() 
                { 
                    DebtorID = debtorCreateRes.DebtorID,
                    FreightForwarderAddressID = address.FreightForwarderAddressID 
                };
                await Client.DeleteAsync(addressDeleteReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
            }

            // Clean up - delete the debtor
            DebtorDELETERequest debtorDeleteReq = new DebtorDELETERequest() { DebtorID = debtorCreateRes.DebtorID };
            await Client.DeleteAsync(debtorDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}


