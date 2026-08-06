using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.Creditors
{
    public class Tag : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task CreditorTag_CRUD()
        {
            // Create a tag
            CreditorTagPOSTRequest tagCreateReq = new CreditorTagPOSTRequest()
            {
                Text = RandomString(5)
            };

            CreditorTag tagCreateRes = await Client.PostAsync(tagCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(tagCreateRes.Text, Is.EqualTo(tagCreateReq.Text));
            Assert.That(tagCreateRes.RecID, !Is.Null);

            // Read all tags and ensure the created tag is returned
            CreditorTagGETManyRequest tagGetManyReq = new CreditorTagGETManyRequest();
            List<CreditorTag> tagGetManyRes = await Client.GetAsync(tagGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagGetManyRes.Any(x => x.RecID == tagCreateRes.RecID), Is.True);

            // Read the created tag using the RecID
            CreditorTagGETRequest tagGetReq = new CreditorTagGETRequest() { RecID = tagCreateRes.RecID };
            CreditorTag tagGetRes = await Client.GetAsync(tagGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagGetRes.Text, Is.EqualTo(tagCreateReq.Text));

            // Update the tag
            CreditorTagPATCHRequest tagPatchReq = new CreditorTagPATCHRequest()
            {
                RecID = tagCreateRes.RecID,
                Text = "Updated Tag " + RandomString(6)
            };
            CreditorTag tagPatchRes = await Client.PatchAsync(tagPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagPatchRes.RecID, Is.EqualTo(tagPatchReq.RecID));
            Assert.That(tagPatchRes.Text, Is.EqualTo(tagPatchReq.Text));

            // Remove cache entry for creditor tags (internal endpoint)
            CreditorTagCACHEDELETERequest cacheDeleteReq = new CreditorTagCACHEDELETERequest();
            WebServiceException cacheEx = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                await Client.DeleteAsync(cacheDeleteReq);
            });
            Assert.That(cacheEx.ErrorMessage, Does.Contain("Invalid ClientKey"));

            // Remove the created tag
            CreditorTagDELETERequest tagDeleteReq = new CreditorTagDELETERequest() { RecID = tagCreateRes.RecID };
            await Client.DeleteAsync(tagDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted tag is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                CreditorTag getDeletedRes = await Client.GetAsync(tagGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all tags and ensure the deleted tag is no longer returned
            tagGetManyRes = await Client.GetAsync(tagGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagGetManyRes.Any(x => x.RecID == tagCreateRes.RecID), Is.False);

            // Try to GET non-existent tag to make sure we get a 404
            tagGetReq.RecID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                CreditorTag getRes = await Client.GetAsync(tagGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}


