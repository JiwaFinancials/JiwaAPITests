using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors.Group;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiwaAPITests.Debtors
{
    public class Groups : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Debtor_Groups_CRUD()
        {
            // Get the list of groups
            DebtorGroupGETManyRequest groupsGetListReq = new DebtorGroupGETManyRequest();

            List<DebtorGroup> groupsGetListRes = await Client.GetAsync(groupsGetListReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(groupsGetListRes, Is.Not.Null);

            // Create a new group
            DebtorGroupPOSTRequest groupPOSTReq = new DebtorGroupPOSTRequest()
            {
                Description = "Test Group " + RandomString(5)
            };

            DebtorGroup groupPOSTRes = await Client.PostAsync(groupPOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(groupPOSTRes.GroupID, !Is.Null);
            Assert.That(groupPOSTRes.Description, Is.EqualTo(groupPOSTReq.Description));

            string groupID = groupPOSTRes.GroupID;

            // Read the created group
            DebtorGroupGETRequest groupGETReq = new DebtorGroupGETRequest()
            {
                GroupID = groupID
            };

            DebtorGroup groupGETRes = await Client.GetAsync(groupGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(groupGETRes.GroupID, Is.EqualTo(groupID));
            Assert.That(groupGETRes.Description, Is.EqualTo(groupPOSTReq.Description));

            // Update the group
            DebtorGroupPATCHRequest groupPATCHReq = new DebtorGroupPATCHRequest()
            {
                GroupID = groupID,
                Description = "Updated Test Group " + RandomString(5)
            };

            DebtorGroup groupPATCHRes = await Client.PatchAsync(groupPATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(groupPATCHRes.Description, Is.EqualTo(groupPATCHReq.Description));

            // Delete the group
            DebtorGroupDELETERequest groupDELETEReq = new DebtorGroupDELETERequest()
            {
                GroupID = groupID
            };

            await Client.DeleteAsync(groupDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the deleted group is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                DebtorGroup getDeletedRes = await Client.GetAsync(groupGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Try to GET a non-existent group to make sure we get a 404
            groupGETReq.GroupID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                DebtorGroup getNotFoundRes = await Client.GetAsync(groupGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}

