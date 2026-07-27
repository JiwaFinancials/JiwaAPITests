using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors.Group;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Debtors
{
    public class GroupMemberships : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Debtor_GroupMemberships_CRUD()
        {
            // Create a debtor we can operate on
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

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

            // Create a second new group
            DebtorGroupPOSTRequest groupPOSTReq2 = new DebtorGroupPOSTRequest()
            {
                Description = "Test Group " + RandomString(5)
            };

            DebtorGroup groupPOSTRes2 = await Client.PostAsync(groupPOSTReq2);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(groupPOSTRes2.GroupID, !Is.Null);
            Assert.That(groupPOSTRes2.Description, Is.EqualTo(groupPOSTReq2.Description));

            string groupID2 = groupPOSTRes2.GroupID;

            try
            {

                // Get the list of group memberships (initially empty or existing)
                DebtorGroupMembershipsGETManyRequest groupMembershipsGetListReq = new DebtorGroupMembershipsGETManyRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID
                };

                List<DebtorGroupMembership> groupMembershipsGetListRes = await Client.GetAsync(groupMembershipsGetListReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(groupMembershipsGetListRes, Is.Not.Null);
                Assert.That(groupMembershipsGetListRes.Count, Is.EqualTo(0)); // Should be empty initially)

                // Try to add a group membership to the debtor
                DebtorGroupMembershipPOSTRequest groupMembershipPOSTReq = new DebtorGroupMembershipPOSTRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    GroupRecID = groupID
                };

                DebtorGroupMembership groupMembershipPOSTRes = await Client.PostAsync(groupMembershipPOSTReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                Assert.That(groupMembershipPOSTRes.GroupMembershipID, !Is.Null);

                string groupMembershipID = groupMembershipPOSTRes.GroupMembershipID;

                // Read the created group membership
                DebtorGroupMembershipGETRequest groupMembershipGETReq = new DebtorGroupMembershipGETRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    GroupMembershipID = groupMembershipID
                };

                DebtorGroupMembership groupMembershipGETRes = await Client.GetAsync(groupMembershipGETReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(groupMembershipGETRes.GroupMembershipID, Is.EqualTo(groupMembershipID));
                Assert.That(groupMembershipGETRes.IsDefault, Is.EqualTo(true)); //First group added to a debtor will be default

                // Try to add a second group membership to the debtor
                DebtorGroupMembershipPOSTRequest groupMembershipPOSTReq2 = new DebtorGroupMembershipPOSTRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    GroupRecID = groupID2
                };

                DebtorGroupMembership groupMembershipPOSTRes2 = await Client.PostAsync(groupMembershipPOSTReq2);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                Assert.That(groupMembershipPOSTRes2.GroupMembershipID, !Is.Null);

                string groupMembershipID2 = groupMembershipPOSTRes2.GroupMembershipID;

                // Read the created group membership
                DebtorGroupMembershipGETRequest groupMembershipGETReq2 = new DebtorGroupMembershipGETRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    GroupMembershipID = groupMembershipID2
                };

                DebtorGroupMembership groupMembershipGETRes2 = await Client.GetAsync(groupMembershipGETReq2);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(groupMembershipGETRes2.GroupMembershipID, Is.EqualTo(groupMembershipID2));
                Assert.That(groupMembershipGETRes2.IsDefault, Is.EqualTo(false)); //Second group added to a debtor will not be default

                // Update the group membership (make the second group membership the default)
                DebtorGroupMembershipPATCHRequest groupMembershipPATCHReq = new DebtorGroupMembershipPATCHRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    GroupMembershipID = groupMembershipID2,
                    IsDefault = true
                };

                DebtorGroupMembership groupMembershipPATCHRes = await Client.PatchAsync(groupMembershipPATCHReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(groupMembershipPATCHRes.IsDefault, Is.True);

                // Read the first created group membership and ensure it is no longer the default
                DebtorGroupMembershipGETRequest groupMembershipGETReq3 = new DebtorGroupMembershipGETRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    GroupMembershipID = groupMembershipID
                };

                DebtorGroupMembership groupMembershipGETRes3 = await Client.GetAsync(groupMembershipGETReq3);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(groupMembershipGETRes3.GroupMembershipID, Is.EqualTo(groupMembershipID));
                Assert.That(groupMembershipGETRes3.IsDefault, Is.EqualTo(false)); //First group added to a debtor will no longer be default

                // Delete the first group membership
                DebtorGroupMembershipDELETERequest groupMembershipDELETEReq = new DebtorGroupMembershipDELETERequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    GroupMembershipID = groupMembershipID
                };

                await Client.DeleteAsync(groupMembershipDELETEReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

                // Verify the deleted group membership is not there anymore
                WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
                {
                    DebtorGroupMembership getDeletedRes = await Client.GetAsync(groupMembershipGETReq);
                });
                Assert.That(ex.StatusCode, Is.EqualTo(404));

                // Delete the second group membership
                DebtorGroupMembershipDELETERequest groupMembershipDELETEReq2 = new DebtorGroupMembershipDELETERequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    GroupMembershipID = groupMembershipID2
                };

                await Client.DeleteAsync(groupMembershipDELETEReq2);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

                // Verify the deleted group membership is not there anymore
                WebServiceException ex2 = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
                {
                    DebtorGroupMembership getDeletedRes2 = await Client.GetAsync(groupMembershipGETReq2);
                });
                Assert.That(ex2.StatusCode, Is.EqualTo(404));
            }
            finally
            {
                // Clean up: Remove the created debtor
                DebtorDELETERequest debtorDeleteReq = new DebtorDELETERequest() { DebtorID = debtorCreateRes.DebtorID };
                await Client.DeleteAsync(debtorDeleteReq);

                // Delete the group
                DebtorGroupDELETERequest groupDELETEReq = new DebtorGroupDELETERequest()
                {
                    GroupID = groupID
                };

                await Client.DeleteAsync(groupDELETEReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

                // Delete the second group
                DebtorGroupDELETERequest groupDELETEReq2 = new DebtorGroupDELETERequest()
                {
                    GroupID = groupID2
                };

                await Client.DeleteAsync(groupDELETEReq2);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
            }
        }
        #endregion
    }
}

