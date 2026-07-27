using JiwaFinancials.Jiwa.JiwaServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesOrderCreditReasonDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.CreditReason;

namespace JiwaAPITests.SalesOrders
{
    public class CreditReason : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task SalesOrderCreditReason_CRUD()
        {
            // Create a sales order credit reason
            SalesOrderCreditReasonsPOSTRequest creditReasonCreateReq = new SalesOrderCreditReasonsPOSTRequest()
            {
                CreditReasonDescription = "Sales Order Credit Reason " + RandomString(8),
                CreditIntoStock = false,
                IsEnabled = true,
                IsDefault = false
            };

            SalesOrderCreditReasonDto creditReasonCreateRes = await Client.PostAsync(creditReasonCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditReasonCreateRes.CreditReasonID, Is.Not.Null);
            Assert.That(creditReasonCreateRes.CreditReasonDescription, Is.EqualTo(creditReasonCreateReq.CreditReasonDescription));

            // Read all sales order credit reasons and ensure the created credit reason is returned
            SalesOrderCreditReasonsGETManyRequest creditReasonsGetManyReq = new SalesOrderCreditReasonsGETManyRequest();
            List<SalesOrderCreditReasonDto> creditReasonsGetManyRes = await Client.GetAsync(creditReasonsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(creditReasonsGetManyRes.Any(x => x.CreditReasonID == creditReasonCreateRes.CreditReasonID), Is.True);

            // Read the created sales order credit reason using the CreditReasonID
            SalesOrderCreditReasonsGETRequest creditReasonGetReq = new SalesOrderCreditReasonsGETRequest()
            {
                CreditReasonID = creditReasonCreateRes.CreditReasonID
            };

            SalesOrderCreditReasonDto creditReasonGetRes = await Client.GetAsync(creditReasonGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(creditReasonGetRes.CreditReasonID, Is.EqualTo(creditReasonCreateRes.CreditReasonID));
            Assert.That(creditReasonGetRes.CreditReasonDescription, Is.EqualTo(creditReasonCreateReq.CreditReasonDescription));

            // Update the sales order credit reason
            SalesOrderCreditReasonsPATCHRequest creditReasonPatchReq = new SalesOrderCreditReasonsPATCHRequest()
            {
                CreditReasonID = creditReasonCreateRes.CreditReasonID,
                CreditReasonDescription = "Updated Sales Order Credit Reason " + RandomString(6),
                CreditIntoStock = true,
                IsEnabled = false
            };

            SalesOrderCreditReasonDto creditReasonPatchRes = await Client.PatchAsync(creditReasonPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(creditReasonPatchRes.CreditReasonID, Is.EqualTo(creditReasonCreateRes.CreditReasonID));
            Assert.That(creditReasonPatchRes.CreditReasonDescription, Is.EqualTo(creditReasonPatchReq.CreditReasonDescription));

            // Read the updated sales order credit reason and confirm the changes were saved
            creditReasonGetRes = await Client.GetAsync(creditReasonGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(creditReasonGetRes.CreditReasonDescription, Is.EqualTo(creditReasonPatchReq.CreditReasonDescription));
            Assert.That(creditReasonGetRes.CreditIntoStock, Is.EqualTo(creditReasonPatchReq.CreditIntoStock));
            Assert.That(creditReasonGetRes.IsEnabled, Is.EqualTo(creditReasonPatchReq.IsEnabled));

            // Remove the created sales order credit reason
            SalesOrderCreditReasonsDELETERequest creditReasonDeleteReq = new SalesOrderCreditReasonsDELETERequest()
            {
                CreditReasonID = creditReasonCreateRes.CreditReasonID
            };

            await Client.DeleteAsync(creditReasonDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted sales order credit reason is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                SalesOrderCreditReasonDto deletedCreditReasonGetRes = await Client.GetAsync(creditReasonGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all sales order credit reasons and ensure the deleted credit reason is no longer returned
            creditReasonsGetManyRes = await Client.GetAsync(creditReasonsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(creditReasonsGetManyRes.Any(x => x.CreditReasonID == creditReasonCreateRes.CreditReasonID), Is.False);
        }
        #endregion

        #region "{Cache}"
        [Test]
        public async Task SalesOrderCreditReasonCacheDelete()
        {
            // Remove the sales order credit reason cache entry (internal endpoint)
            SalesOrderCreditReasonsCACHEDELETERequest cacheDeleteReq = new SalesOrderCreditReasonsCACHEDELETERequest();
            WebServiceException cacheEx = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                await Client.DeleteAsync(cacheDeleteReq);
            });
            Assert.That(cacheEx.ErrorMessage, Does.Contain("Invalid ClientKey"));
        }
        #endregion
    }
}

