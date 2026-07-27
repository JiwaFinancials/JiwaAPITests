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
    public class TagMembership : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Debtor_TagMembership_CRUD()
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

            // Create a tag to use
            DebtorTagPOSTRequest tagCreateReq = new DebtorTagPOSTRequest()
            {
                Text = RandomString(5)
            };

            DebtorTag tagCreateRes = await Client.PostAsync(tagCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(tagCreateRes.RecID, !Is.Null);

            string tagID = tagCreateRes.RecID;

            try
            {
                // Get the list of tag memberships (initially empty)
                DebtorTagMembershipGETManyRequest tagMembershipsGetListReq = new DebtorTagMembershipGETManyRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID
                };

                List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag> tagMembershipsGetListRes = await Client.GetAsync(tagMembershipsGetListReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(tagMembershipsGetListRes, Is.Not.Null);

                // Add a tag membership to the debtor
                DebtorTagMembershipPOSTRequest tagMembershipPOSTReq = new DebtorTagMembershipPOSTRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    TagID = tagID
                };

                JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag tagMembershipPOSTRes = await Client.PostAsync(tagMembershipPOSTReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                Assert.That(tagMembershipPOSTRes.RecID, Is.EqualTo(tagID));

                // Get the list of tag memberships again (should now have one)
                List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag> tagMembershipsGetListRes2 = await Client.GetAsync(tagMembershipsGetListReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(tagMembershipsGetListRes2.Count, Is.GreaterThanOrEqualTo(1));

                // Create another tag for PUT test
                DebtorTagPOSTRequest tagCreateReq2 = new DebtorTagPOSTRequest()
                {
                    Text = RandomString(5)
                };

                DebtorTag tagCreateRes2 = await Client.PostAsync(tagCreateReq2);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

                string tagID2 = tagCreateRes2.RecID;

                // Set the tag memberships for the debtor using PUT
                DebtorTagMembershipPUTRequest tagMembershipPUTReq = new DebtorTagMembershipPUTRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    Tags = new List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag>()
                    {
                        new JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag() { RecID = tagID },
                        new JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag() { RecID = tagID2 }
                    }
                };

                List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag> tagMembershipPUTRes = await Client.PutAsync(tagMembershipPUTReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(tagMembershipPUTRes.Count, Is.EqualTo(2));

                // Delete a tag membership
                DebtorTagMembershipDELETERequest tagMembershipDELETEReq = new DebtorTagMembershipDELETERequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    TagID = tagID
                };

                await Client.DeleteAsync(tagMembershipDELETEReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

                // Verify the membership was removed
                List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag> tagMembershipsGetListRes3 = await Client.GetAsync(tagMembershipsGetListReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(tagMembershipsGetListRes3.Any(t => t.RecID == tagID), Is.False);

                // Clean up second tag
                DebtorTagDELETERequest tag2DeleteReq = new DebtorTagDELETERequest() { RecID = tagID2 };
                await Client.DeleteAsync(tag2DeleteReq);
            }
            finally
            {
                // Clean up: Remove the created tag
                DebtorTagDELETERequest tagDeleteReq = new DebtorTagDELETERequest() { RecID = tagID };
                try
                {
                    await Client.DeleteAsync(tagDeleteReq);
                }
                catch
                {
                    // Tag may have been deleted as part of membership cleanup
                }

                // Clean up: Remove the created debtor
                DebtorDELETERequest debtorDeleteReq = new DebtorDELETERequest() { DebtorID = debtorCreateRes.DebtorID };
                await Client.DeleteAsync(debtorDeleteReq);
            }
        }
        #endregion
    }
}

