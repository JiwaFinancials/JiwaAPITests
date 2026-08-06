using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Threading.Tasks;

namespace JiwaAPITests.Creditors
{
    public class Account : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Creditor_CRUD()
        {
            // Create a creditor
            CreditorPOSTRequest accountCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Creditor Test"
            };

            Creditor accountCreateRes = await Client.PostAsync(accountCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(accountCreateRes.AccountNo, Is.EqualTo(accountCreateReq.AccountNo));
            Assert.That(accountCreateRes.Name, Is.EqualTo(accountCreateReq.Name));
            Assert.That(accountCreateRes.CreditorID, Is.Not.Null);

            // Read the created creditor using the CreditorID
            CreditorGETRequest accountGetReq = new CreditorGETRequest() { CreditorID = accountCreateRes.CreditorID };
            Creditor accountGetRes = await Client.GetAsync(accountGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(accountGetRes.AccountNo, Is.EqualTo(accountCreateReq.AccountNo));
            Assert.That(accountGetRes.Name, Is.EqualTo(accountCreateReq.Name));

            // Update the creditor
            CreditorPATCHRequest accountPatchReq = new CreditorPATCHRequest()
            {
                CreditorID = accountCreateRes.CreditorID,
                Name = "Updated Creditor Test"
            };
            Creditor accountPatchRes = await Client.PatchAsync(accountPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(accountPatchRes.CreditorID, Is.EqualTo(accountPatchReq.CreditorID));
            Assert.That(accountPatchRes.Name, Is.EqualTo(accountPatchReq.Name));

            // Remove the created creditor
            CreditorDELETERequest accountDeleteReq = new CreditorDELETERequest() { CreditorID = accountCreateRes.CreditorID };
            await Client.DeleteAsync(accountDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // ensure the deleted creditor is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                Creditor getDeletedRes = await Client.GetAsync(accountGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Try to GET non-existent creditor to make sure we get a 404
            accountGetReq.CreditorID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                Creditor getRes = await Client.GetAsync(accountGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}


