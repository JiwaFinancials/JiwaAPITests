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
            // Create an item we can operate on
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

            // Add an contact name to the item
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

            // Try also the GET Many - should return the single item we added
            DebtorContactNamesGETManyRequest debtorContactNameGETManyReq = new DebtorContactNamesGETManyRequest()
            {
                DebtorID = debtorCreateRes.DebtorID
            };

            List<DebtorContactName> debtorContactNameGETManyRes = await Client.GetAsync(debtorContactNameGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorContactNameGETManyRes.Count, Is.GreaterThan(0));

            // Try patching the item
            DebtorContactNamePATCHRequest debtorContactNamePATCHReq = new DebtorContactNamePATCHRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                ContactNameID = debtorContactNamePOSTRes.ContactNameID,
                EmailAddress = "g@h.i"
            };
            DebtorContactName debtorContactNamePATCHRes = await Client.PatchAsync(debtorContactNamePATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorContactNamePATCHRes.EmailAddress, Is.EqualTo(debtorContactNamePATCHReq.EmailAddress));

            // Get the patched item and ensure it matches what we patched
            debtorContactNameGETRes = await Client.GetAsync(debtorContactNameGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorContactNameGETRes.EmailAddress, Is.EqualTo(debtorContactNamePATCHReq.EmailAddress));

            // Remove the alternate child we added
            DebtorContactNameDELETERequest debtorContactNameDELETEReq = new DebtorContactNameDELETERequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                ContactNameID = debtorContactNamePOSTRes.ContactNameID
            };
            await Client.DeleteAsync(debtorContactNameDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the child is no longer present in the list of children for the item
            debtorContactNameGETManyRes = await Client.GetAsync(debtorContactNameGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorContactNameGETManyRes.Count, Is.EqualTo(0));

            // Ensure explicitly requesting the child 404's
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                debtorContactNameGETRes = await Client.GetAsync(debtorContactNameGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}
