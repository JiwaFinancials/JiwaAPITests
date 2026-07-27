using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using ServiceStack;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiwaAPITests.Creditors
{
    public class TagMembership : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Creditor_TagMembership_CRUD()
        {
            // Create a creditor we can operate on
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Creditor Tag Membership Test"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.AccountNo, Is.EqualTo(creditorCreateReq.AccountNo));
            Assert.That(creditorCreateRes.CreditorID, !Is.Null);

            // Create a tag
            CreditorTagPOSTRequest tagCreateReq = new CreditorTagPOSTRequest()
            {
                Text = RandomString(5)
            };

            CreditorTag tagCreateRes = await Client.PostAsync(tagCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(tagCreateRes.Text, Is.EqualTo(tagCreateReq.Text));
            Assert.That(tagCreateRes.RecID, !Is.Null);

            // Add a tag membership to the creditor
            CreditorTagMembershipPOSTRequest tagMembershipPOSTRequest = new CreditorTagMembershipPOSTRequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                TagID = tagCreateRes.RecID
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag tagMembershipCreateRes = await Client.PostAsync(tagMembershipPOSTRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(tagMembershipCreateRes.Text, Is.EqualTo(tagCreateReq.Text));
            Assert.That(tagMembershipCreateRes.RecID, Is.EqualTo(tagCreateRes.RecID));

            // Get the tag memberships for the creditor
            CreditorTagMembershipGETManyRequest tagMembershipGetManyReq = new CreditorTagMembershipGETManyRequest()
            {
                CreditorID = creditorCreateRes.CreditorID
            };

            List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag> tagMembershipGetManyRes = await Client.GetAsync(tagMembershipGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagMembershipGetManyRes, !Is.Null);
            Assert.That(tagMembershipGetManyRes.Count, Is.EqualTo(1));
            Assert.That(tagMembershipGetManyRes[0].RecID, Is.EqualTo(tagCreateRes.RecID));

            // Replace all the tag memberships with an empty list
            CreditorTagMembershipPUTRequest tagMembershipPutReq = new CreditorTagMembershipPUTRequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                Tags = new List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag>()
            };

            List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag> tagMembershipPutRes = await Client.PutAsync(tagMembershipPutReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagMembershipPutRes, !Is.Null);
            Assert.That(tagMembershipPutRes.Count, Is.EqualTo(0));

            CreditorTag firstTagCreateRes = tagCreateRes.CreateCopy();

            // Create a second tag so we can add it with a PUT later
            tagCreateReq = new CreditorTagPOSTRequest()
            {
                Text = RandomString(5)
            };

            tagCreateRes = await Client.PostAsync(tagCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(tagCreateRes.Text, Is.EqualTo(tagCreateReq.Text));
            Assert.That(tagCreateRes.RecID, !Is.Null);

            // Replace all the tag memberships with the two tags we created
            tagMembershipPutReq = new CreditorTagMembershipPUTRequest()
            {
                CreditorID = creditorCreateRes.CreditorID,
                Tags = new List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag()
                    {
                        RecID = firstTagCreateRes.RecID
                    },
                    new JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag()
                    {
                        RecID = tagCreateRes.RecID
                    }
                }
            };

            tagMembershipPutRes = await Client.PutAsync(tagMembershipPutReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagMembershipPutRes, !Is.Null);
            Assert.That(tagMembershipPutRes.Count, Is.EqualTo(2));
            Assert.That(tagMembershipPutRes[0].RecID, Is.EqualTo(firstTagCreateRes.RecID));
            Assert.That(tagMembershipPutRes[1].RecID, Is.EqualTo(tagCreateRes.RecID));

            // Remove a tag membership
            CreditorTagMembershipDELETERequest tagMembershipDeleteReq = new CreditorTagMembershipDELETERequest() { CreditorID = creditorCreateRes.CreditorID, TagID = tagCreateRes.RecID };
            await Client.DeleteAsync(tagMembershipDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted tag membership is not there anymore
            tagMembershipGetManyReq = new CreditorTagMembershipGETManyRequest()
            {
                CreditorID = creditorCreateRes.CreditorID
            };

            tagMembershipGetManyRes = await Client.GetAsync(tagMembershipGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagMembershipGetManyRes, !Is.Null);
            Assert.That(tagMembershipGetManyRes.Count, Is.EqualTo(1));
            Assert.That(tagMembershipGetManyRes[0].RecID, Is.EqualTo(firstTagCreateRes.RecID));

            // Remove the creditor and tags we created
            CreditorDELETERequest creditorDeleteReq = new CreditorDELETERequest() { CreditorID = creditorCreateRes.CreditorID };
            await Client.DeleteAsync(creditorDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            CreditorTagDELETERequest firstTagDeleteReq = new CreditorTagDELETERequest() { RecID = firstTagCreateRes.RecID };
            await Client.DeleteAsync(firstTagDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            CreditorTagDELETERequest secondTagDeleteReq = new CreditorTagDELETERequest() { RecID = tagCreateRes.RecID };
            await Client.DeleteAsync(secondTagDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

