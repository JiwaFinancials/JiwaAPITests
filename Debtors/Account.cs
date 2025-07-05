using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using JiwaFinancials.Jiwa.JiwaServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ServiceStack.Diagnostics.Events;
using NUnit.Framework;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using ServiceStack;
using System.Diagnostics.Metrics;
using JiwaFinancials.Jiwa.JiwaServiceModel.Notes;
using System.Net.Mail;
using System.Numerics;

namespace JiwaAPITests.Debtors
{
    [TestFixture]
    class Account : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Debtor_CRUD()
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

            // Read the created debtor using the DebtorID
            DebtorGETRequest accountGetReq = new DebtorGETRequest() { DebtorID = accountCreateRes.DebtorID };
            Debtor accountGetRes = await Client.GetAsync(accountGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(accountGetRes.AccountNo, Is.EqualTo(accountCreateReq.AccountNo));
            Assert.That(accountGetRes.Name, Is.EqualTo(accountCreateReq.Name));

            // Update the debtor
            DebtorPATCHRequest accountPatchReq = new DebtorPATCHRequest()
            {
                DebtorID = accountCreateRes.DebtorID,
                Name = "Updated Debtor Test",
                EmailAddress = "d@e.f"
            };
            Debtor accountPatchRes = await Client.PatchAsync(accountPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(accountPatchRes.Name, Is.EqualTo(accountPatchReq.Name));
            Assert.That(accountPatchRes.EmailAddress, Is.EqualTo(accountPatchReq.EmailAddress));

            // Remove the created debtor
            DebtorDELETERequest accountDeleteReq = new DebtorDELETERequest() { DebtorID = accountCreateRes.DebtorID };
            await Client.DeleteAsync(accountDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // ensure the deleted debtor is not there anymore            
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                Debtor getDeletedRes = await Client.GetAsync(accountGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Try to GET non-existent debtor to make sure we get a 404
            accountGetReq.DebtorID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                Debtor accountGetRes = await Client.GetAsync(accountGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "Contact Names"
        [Test]
        public async Task Debtor_ContactNames_CRUD()
        {
            // Create an account we can operate on
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.AccountNo, Is.EqualTo(debtorCreateReq.AccountNo));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

            // Add an contact name to the account
            DebtorContactNamePOSTRequest debtorContactNamePOSTReq = new DebtorContactNamePOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                Title = "Mr.",
                FirstName = "John",
                Surname = "Citizen"
            };
            DebtorContactName debtorContactNamePOSTRes = await Client.PostAsync(debtorContactNamePOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorContactNamePOSTRes.ContactNameID, !Is.Null);
            Assert.That(debtorContactNamePOSTRes.Title, Is.EqualTo(debtorContactNamePOSTReq.Title));
            Assert.That(debtorContactNamePOSTRes.FirstName, Is.EqualTo(debtorContactNamePOSTReq.FirstName));
            Assert.That(debtorContactNamePOSTRes.Surname, Is.EqualTo(debtorContactNamePOSTReq.Surname));

            // Check to see if the debtor contact name is present via a GET 
            DebtorContactNameGETRequest debtorContactNameGETReq = new DebtorContactNameGETRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                ContactNameID = debtorContactNamePOSTRes.ContactNameID
            };

            DebtorContactName debtorContactNameGETRes = await Client.GetAsync(debtorContactNameGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorContactNameGETRes.ContactNameID, Is.EqualTo(debtorContactNamePOSTRes.ContactNameID));
            Assert.That(debtorContactNameGETRes.Title, Is.EqualTo(debtorContactNamePOSTRes.Title));
            Assert.That(debtorContactNameGETRes.FirstName, Is.EqualTo(debtorContactNamePOSTRes.FirstName));
            Assert.That(debtorContactNameGETRes.Surname, Is.EqualTo(debtorContactNamePOSTRes.Surname));

            // Try also the GET Many - should return the single account we added
            DebtorContactNamesGETManyRequest debtorContactNameGETManyReq = new DebtorContactNamesGETManyRequest()
            {
                DebtorID = debtorCreateRes.DebtorID
            };

            List<DebtorContactName> debtorContactNameGETManyRes = await Client.GetAsync(debtorContactNameGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorContactNameGETManyRes.Count, Is.GreaterThan(0));

            // Try patching the account
            DebtorContactNamePATCHRequest debtorContactNamePATCHReq = new DebtorContactNamePATCHRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                ContactNameID = debtorContactNamePOSTRes.ContactNameID,
                EmailAddress = "g@h.i"
            };
            DebtorContactName debtorContactNamePATCHRes = await Client.PatchAsync(debtorContactNamePATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorContactNamePATCHRes.EmailAddress, Is.EqualTo(debtorContactNamePATCHReq.EmailAddress));

            // Get the patched account and ensure it matches what we patched
            debtorContactNameGETRes = await Client.GetAsync(debtorContactNameGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorContactNameGETRes.EmailAddress, Is.EqualTo(debtorContactNamePATCHReq.EmailAddress));

            // Remove the debtor contact name we added
            DebtorContactNameDELETERequest debtorContactNameDELETEReq = new DebtorContactNameDELETERequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                ContactNameID = debtorContactNamePOSTRes.ContactNameID
            };
            await Client.DeleteAsync(debtorContactNameDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the debtor contact name is no longer present in the list of debtor contact names for the account
            debtorContactNameGETManyRes = await Client.GetAsync(debtorContactNameGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorContactNameGETManyRes.Count, Is.EqualTo(0));

            // Ensure explicitly requesting the debtor contact name 404's
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                debtorContactNameGETRes = await Client.GetAsync(debtorContactNameGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "Delivery Addresses"
        [Test]
        public async Task Debtor_DeliveryAddresses_CRUD()
        {
            // Create an account we can operate on
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.AccountNo, Is.EqualTo(debtorCreateReq.AccountNo));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

            // Add an delivery address to the account
            DebtorDeliveryAddressPOSTRequest debtorDeliveryAddressPOSTReq = new DebtorDeliveryAddressPOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressName = "Name",
                DeliveryAddressCode = "Code",
                Address1 = "Address1",
                Address2 = "Address2",
                Address3 = "Address3",
                Address4 = "Address4",
                Postcode = "Postcode",
                Country = "Country",
                Notes = "Notes",
                CourierDetails = "CourierDetails",
                EDIStoreLocationCode = "EDIStoreLocationCode",
                EmailAddress = "EmailAddress",
                Phone = "Phone",
            };
            DebtorDeliveryAddress debtorDeliveryAddressPOSTRes = await Client.PostAsync(debtorDeliveryAddressPOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorDeliveryAddressPOSTRes.DeliveryAddressID, !Is.Null);
            Assert.That(debtorDeliveryAddressPOSTRes.DeliveryAddressName, Is.EqualTo(debtorDeliveryAddressPOSTReq.DeliveryAddressName));
            Assert.That(debtorDeliveryAddressPOSTRes.DeliveryAddressCode, Is.EqualTo(debtorDeliveryAddressPOSTReq.DeliveryAddressCode));
            Assert.That(debtorDeliveryAddressPOSTRes.Address1, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address1));
            Assert.That(debtorDeliveryAddressPOSTRes.Address2, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address2));
            Assert.That(debtorDeliveryAddressPOSTRes.Address3, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address3));
            Assert.That(debtorDeliveryAddressPOSTRes.Address4, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address4));
            Assert.That(debtorDeliveryAddressPOSTRes.Postcode, Is.EqualTo(debtorDeliveryAddressPOSTReq.Postcode));
            Assert.That(debtorDeliveryAddressPOSTRes.Country, Is.EqualTo(debtorDeliveryAddressPOSTReq.Country));
            Assert.That(debtorDeliveryAddressPOSTRes.Notes, Is.EqualTo(debtorDeliveryAddressPOSTReq.Notes));
            Assert.That(debtorDeliveryAddressPOSTRes.CourierDetails, Is.EqualTo(debtorDeliveryAddressPOSTReq.CourierDetails));
            Assert.That(debtorDeliveryAddressPOSTRes.EDIStoreLocationCode, Is.EqualTo(debtorDeliveryAddressPOSTReq.EDIStoreLocationCode));
            Assert.That(debtorDeliveryAddressPOSTRes.EmailAddress, Is.EqualTo(debtorDeliveryAddressPOSTReq.EmailAddress));
            Assert.That(debtorDeliveryAddressPOSTRes.Phone, Is.EqualTo(debtorDeliveryAddressPOSTReq.Phone));

            // Check to see if the debtor delivery address is present via a GET 
            DebtorDeliveryAddressGETRequest debtorDeliveryAddressGETReq = new DebtorDeliveryAddressGETRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = debtorDeliveryAddressPOSTRes.DeliveryAddressID
            };

            DebtorDeliveryAddress debtorDeliveryAddressGETRes = await Client.GetAsync(debtorDeliveryAddressGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorDeliveryAddressGETRes.DeliveryAddressID, Is.EqualTo(debtorDeliveryAddressPOSTRes.DeliveryAddressID));
            Assert.That(debtorDeliveryAddressGETRes.DeliveryAddressName, Is.EqualTo(debtorDeliveryAddressPOSTReq.DeliveryAddressName));
            Assert.That(debtorDeliveryAddressGETRes.DeliveryAddressCode, Is.EqualTo(debtorDeliveryAddressPOSTReq.DeliveryAddressCode));
            Assert.That(debtorDeliveryAddressGETRes.Address1, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address1));
            Assert.That(debtorDeliveryAddressGETRes.Address2, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address2));
            Assert.That(debtorDeliveryAddressGETRes.Address3, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address3));
            Assert.That(debtorDeliveryAddressGETRes.Address4, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address4));
            Assert.That(debtorDeliveryAddressGETRes.Postcode, Is.EqualTo(debtorDeliveryAddressPOSTReq.Postcode));
            Assert.That(debtorDeliveryAddressGETRes.Country, Is.EqualTo(debtorDeliveryAddressPOSTReq.Country));
            Assert.That(debtorDeliveryAddressGETRes.Notes, Is.EqualTo(debtorDeliveryAddressPOSTReq.Notes));
            Assert.That(debtorDeliveryAddressGETRes.CourierDetails, Is.EqualTo(debtorDeliveryAddressPOSTReq.CourierDetails));
            Assert.That(debtorDeliveryAddressGETRes.EDIStoreLocationCode, Is.EqualTo(debtorDeliveryAddressPOSTReq.EDIStoreLocationCode));
            Assert.That(debtorDeliveryAddressGETRes.EmailAddress, Is.EqualTo(debtorDeliveryAddressPOSTReq.EmailAddress));
            Assert.That(debtorDeliveryAddressGETRes.Phone, Is.EqualTo(debtorDeliveryAddressPOSTReq.Phone));

            // Try also the GET Many - should return the single delivery address we added
            DebtorDeliveryAddressesGETManyRequest debtorDeliveryAddressGETManyReq = new DebtorDeliveryAddressesGETManyRequest()
            {
                DebtorID = debtorCreateRes.DebtorID
            };

            List<DebtorDeliveryAddress> debtorDeliveryAddressGETManyRes = await Client.GetAsync(debtorDeliveryAddressGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorDeliveryAddressGETManyRes.Count, Is.GreaterThan(0));

            // Try patching the delivery address
            DebtorDeliveryAddressPATCHRequest debtorDeliveryAddressPATCHReq = new DebtorDeliveryAddressPATCHRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = debtorDeliveryAddressPOSTRes.DeliveryAddressID,
                DeliveryAddressName = "Name Updated",
                DeliveryAddressCode = "Code Updated",
                Address1 = "Address1 Updated",
                Address2 = "Address2 Updated",
                Address3 = "Address3 Updated",
                Address4 = "Address4 Updated",
                Postcode = "Postcode2",
                Country = "Country Updated",
                Notes = "Notes Updated",
                CourierDetails = "CourierDetails Updated",
                EDIStoreLocationCode = "EDIStoreLocationCode Updated",
                EmailAddress = "EmailAddress Updated",
                Phone = "Phone Updated",
            };
            DebtorDeliveryAddress debtorDeliveryAddressPATCHRes = await Client.PatchAsync(debtorDeliveryAddressPATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorDeliveryAddressPATCHRes.EmailAddress, Is.EqualTo(debtorDeliveryAddressPATCHReq.EmailAddress));

            // Get the patched account and ensure it matches what we patched
            debtorDeliveryAddressGETRes = await Client.GetAsync(debtorDeliveryAddressGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorDeliveryAddressGETRes.EmailAddress, Is.EqualTo(debtorDeliveryAddressPATCHReq.EmailAddress));

            // Add a second delivery address to the account
            DebtorDeliveryAddressPOSTRequest newDebtorDeliveryAddressPOSTReq = new DebtorDeliveryAddressPOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressName = "New Name",
                DeliveryAddressCode = "New Code",
                Address1 = "New Address1",
                Address2 = "New Address2",
                Address3 = "New Address3",
                Address4 = "New Address4",
                Postcode = "Postcode",
                Country = "New Country",
                Notes = "New Notes",
                CourierDetails = "New CourierDetails",
                EDIStoreLocationCode = "New EDIStoreLocationCode",
                EmailAddress = "New EmailAddress",
                Phone = "New Phone",
            };
            DebtorDeliveryAddress newDebtorDeliveryAddressPOSTRes = await Client.PostAsync(newDebtorDeliveryAddressPOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(newDebtorDeliveryAddressPOSTRes.DeliveryAddressID, !Is.Null);
            Assert.That(newDebtorDeliveryAddressPOSTRes.DeliveryAddressName, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.DeliveryAddressName));
            Assert.That(newDebtorDeliveryAddressPOSTRes.DeliveryAddressCode, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.DeliveryAddressCode));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Address1, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Address1));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Address2, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Address2));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Address3, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Address3));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Address4, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Address4));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Postcode, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Postcode));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Country, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Country));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Notes, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Notes));
            Assert.That(newDebtorDeliveryAddressPOSTRes.CourierDetails, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.CourierDetails));
            Assert.That(newDebtorDeliveryAddressPOSTRes.EDIStoreLocationCode, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.EDIStoreLocationCode));
            Assert.That(newDebtorDeliveryAddressPOSTRes.EmailAddress, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.EmailAddress));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Phone, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Phone));

            // Make the new delivery address the default delivery address for the account
            debtorDeliveryAddressPATCHReq = new DebtorDeliveryAddressPATCHRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = newDebtorDeliveryAddressPOSTRes.DeliveryAddressID,
                IsDefault = true
            };
            debtorDeliveryAddressPATCHRes = await Client.PatchAsync(debtorDeliveryAddressPATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorDeliveryAddressPATCHRes.IsDefault, Is.EqualTo(debtorDeliveryAddressPATCHReq.IsDefault));

            // Get the patched account and ensure it matches what we patched
            debtorDeliveryAddressGETReq = new DebtorDeliveryAddressGETRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = newDebtorDeliveryAddressPOSTRes.DeliveryAddressID
            };
            debtorDeliveryAddressGETRes = await Client.GetAsync(debtorDeliveryAddressGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorDeliveryAddressGETRes.IsDefault, Is.EqualTo(debtorDeliveryAddressPATCHReq.IsDefault));

            // Remove the original debtor delivery address we added
            DebtorDeliveryAddressDELETERequest debtorDeliveryAddressDELETEReq = new DebtorDeliveryAddressDELETERequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = debtorDeliveryAddressPOSTRes.DeliveryAddressID
            };
            await Client.DeleteAsync(debtorDeliveryAddressDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the debtor delivery address is no longer present in the list of debtor delivery addresss for the account
            debtorDeliveryAddressGETManyRes = await Client.GetAsync(debtorDeliveryAddressGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorDeliveryAddressGETManyRes.Count, Is.EqualTo(1));

            // Ensure explicitly requesting the debtor delivery address 404's
            debtorDeliveryAddressGETReq = new DebtorDeliveryAddressGETRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = debtorDeliveryAddressPOSTRes.DeliveryAddressID
            };

            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                debtorDeliveryAddressGETRes = await Client.GetAsync(debtorDeliveryAddressGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "Queries"
        [Test]
        public async Task DB_MainQuery()
        {
            DB_MainQuery DB_MainQueryRequest = new DB_MainQuery();
            ServiceStack.QueryResponse<DB_Main> DB_MainQueryResponse;

            //Read all debtor accounts            
            DB_MainQueryResponse = await Client.GetAsync(DB_MainQueryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Let's assume we expect to get at least one debtor account back - demo data has many debtor accounts.
            Assert.That(DB_MainQueryResponse.Results.Count > 0);

            // Try with an invalid APIKey on to make sure we get a 401
            // Need to use a new client for this, as existing session Id's cookied will bind us to the session from
            // previous requests
            using (ServiceStack.JsonApiClient tempClient = new ServiceStack.JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                var ex = Assert.Throws<ServiceStack.WebServiceException>(() => DB_MainQueryResponse = tempClient.Get(DB_MainQueryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }
        }

        [Test]
        public async Task v_Jiwa_Debtor_ListQuery()
        {
            // get first 10 parts
            v_Jiwa_Debtor_ListQuery  v_Jiwa_Inventory_Item_ListQueryRequest = new v_Jiwa_Debtor_ListQuery()
            {
                Take = 10,
                OrderBy = "AccountNo"
            };
            ServiceStack.QueryResponse<v_Jiwa_Debtor_List> v_Jiwa_Debtor_ListQueryResponse;

            v_Jiwa_Debtor_ListQueryResponse = await Client.GetAsync(v_Jiwa_Inventory_Item_ListQueryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Ensure we got only the 10 we asked for
            Assert.That(v_Jiwa_Debtor_ListQueryResponse.Results.Count == 10);

            // Try with an invalid APIKey on to make sure we get a 401
            // Need to use a new client for this, as existing session Id's cookied will bind us to the session from
            // previous requests
            using (ServiceStack.JsonApiClient tempClient = new ServiceStack.JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                var ex = Assert.Throws<ServiceStack.WebServiceException>(() => v_Jiwa_Debtor_ListQueryResponse = tempClient.Get(v_Jiwa_Inventory_Item_ListQueryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }
        }

        #endregion
    }
}
