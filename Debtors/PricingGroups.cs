using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors.PricingGroup;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Debtors
{
    public class PricingGroups : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Debtor_PricingGroups_CRUD()
        {
            // Get the list of pricing groups
            DebtorPricingGroupGETManyRequest pricingGroupsGetListReq = new DebtorPricingGroupGETManyRequest();

            List<DebtorPricingGroup> pricingGroupsGetListRes = await Client.GetAsync(pricingGroupsGetListReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(pricingGroupsGetListRes, Is.Not.Null);

            // Create a new pricing group
            DebtorPricingGroupPOSTRequest pricingGroupPOSTReq = new DebtorPricingGroupPOSTRequest()
            {
                Description = "Test Pricing Group " + RandomString(5)
            };

            DebtorPricingGroup pricingGroupPOSTRes = await Client.PostAsync(pricingGroupPOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(pricingGroupPOSTRes.PricingGroupID, !Is.Null);
            Assert.That(pricingGroupPOSTRes.Description, Is.EqualTo(pricingGroupPOSTReq.Description));

            string pricingGroupID = pricingGroupPOSTRes.PricingGroupID;

            // Read the created pricing group
            DebtorPricingGroupGETRequest pricingGroupGETReq = new DebtorPricingGroupGETRequest()
            {
                PricingGroupID = pricingGroupID
            };

            DebtorPricingGroup pricingGroupGETRes = await Client.GetAsync(pricingGroupGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(pricingGroupGETRes.PricingGroupID, Is.EqualTo(pricingGroupID));
            Assert.That(pricingGroupGETRes.Description, Is.EqualTo(pricingGroupPOSTReq.Description));

            // Create a second new pricing group
            DebtorPricingGroupPOSTRequest pricingGroupPOSTReq2 = new DebtorPricingGroupPOSTRequest()
            {
                Description = "Test Pricing Group " + RandomString(5)
            };

            DebtorPricingGroup pricingGroupPOSTRes2 = await Client.PostAsync(pricingGroupPOSTReq2);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(pricingGroupPOSTRes2.PricingGroupID, !Is.Null);
            Assert.That(pricingGroupPOSTRes2.Description, Is.EqualTo(pricingGroupPOSTReq2.Description));

            string pricingGroupID2 = pricingGroupPOSTRes2.PricingGroupID;

            // Read the second created pricing group
            DebtorPricingGroupGETRequest pricingGroupGETReq2 = new DebtorPricingGroupGETRequest()
            {
                PricingGroupID = pricingGroupID2
            };

            DebtorPricingGroup pricingGroupGETRes2 = await Client.GetAsync(pricingGroupGETReq2);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(pricingGroupGETRes2.PricingGroupID, Is.EqualTo(pricingGroupID2));
            Assert.That(pricingGroupGETRes2.Description, Is.EqualTo(pricingGroupPOSTReq2.Description));
            Assert.That(pricingGroupPOSTRes2.IsDefault, Is.EqualTo(false)); //The second pricing group added will not be set to default.

            // Update the second pricing group to be the default
            DebtorPricingGroupPATCHRequest pricingGroupPATCHReq = new DebtorPricingGroupPATCHRequest()
            {
                PricingGroupID = pricingGroupID2,
                Description = "Updated Test Pricing Group " + RandomString(5),
                IsDefault = true
            };

            DebtorPricingGroup pricingGroupPATCHRes = await Client.PatchAsync(pricingGroupPATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(pricingGroupPATCHRes.PricingGroupID, Is.EqualTo(pricingGroupPATCHReq.PricingGroupID));
            Assert.That(pricingGroupPATCHRes.Description, Is.EqualTo(pricingGroupPATCHReq.Description));

            // Read the second created pricing group
            pricingGroupGETReq2 = new DebtorPricingGroupGETRequest()
            {
                PricingGroupID = pricingGroupID2
            };

            pricingGroupGETRes2 = await Client.GetAsync(pricingGroupGETReq2);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(pricingGroupGETRes2.PricingGroupID, Is.EqualTo(pricingGroupID2));
            Assert.That(pricingGroupGETRes2.Description, Is.EqualTo(pricingGroupPATCHRes.Description));
            Assert.That(pricingGroupGETRes2.IsDefault, Is.EqualTo(true));

            // Delete the first pricing group
            DebtorPricingGroupDELETERequest pricingGroupDELETEReq = new DebtorPricingGroupDELETERequest()
            {
                PricingGroupID = pricingGroupID
            };

            await Client.DeleteAsync(pricingGroupDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the deleted pricing group is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                DebtorPricingGroup getDeletedRes = await Client.GetAsync(pricingGroupGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Try to GET non-existent pricing group to make sure we get a 404
            pricingGroupGETReq.PricingGroupID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                DebtorPricingGroup pricingGroupGetRes = await Client.GetAsync(pricingGroupGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}


