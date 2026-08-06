using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System.Threading.Tasks;

namespace JiwaAPITests.Webhooks
{
    public class Subscriber : JiwaAPITest
    {
        #region "{Subscriber}"
        [Test]
        public async Task Subscriber_CRUD()
        {
            // Create subscriber
            WebhooksSubscriberPOSTRequest subscriberPostReq = new WebhooksSubscriberPOSTRequest()
            {
                Name = "Test Subscriber " + RandomString(8),
                IsEnabled = true
            };

            SY_WebhookSubscriber subscriberPostRes = await Client.PostAsync(subscriberPostReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(subscriberPostRes.RecID, Is.Not.EqualTo(System.Guid.Empty));
            Assert.That(subscriberPostRes.Name, Is.EqualTo(subscriberPostReq.Name));

            string subscriberID = subscriberPostRes.RecID.ToString();

            // Read all subscribers
            QueryResponse<SY_WebhookSubscriber> subscribersGetManyRes = await Client.GetAsync(new WebhooksSubscribersGETManyRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(subscribersGetManyRes.Results, Is.Not.Null);

            // Read subscriber
            WebhookSubscriber subscriberGetRes = await Client.GetAsync(new WebhooksSubscriberGETRequest() { SubscriberID = subscriberID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(subscriberGetRes.RecID.ToString(), Is.EqualTo(subscriberID));
            Assert.That(subscriberGetRes.Name, Is.EqualTo(subscriberPostReq.Name));

            // Update subscriber
            WebhooksSubscriberPATCHRequest subscriberPatchReq = new WebhooksSubscriberPATCHRequest()
            {
                SubscriberID = subscriberID,
                Name = "Updated Subscriber " + RandomString(8),
                IsEnabled = false
            };

            SY_WebhookSubscriber subscriberPatchRes = await Client.PatchAsync(subscriberPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(subscriberPatchRes.RecID.ToString(), Is.EqualTo(subscriberPatchReq.SubscriberID));
            Assert.That(subscriberPatchRes.Name, Is.EqualTo(subscriberPatchReq.Name));
            Assert.That(subscriberPatchRes.IsEnabled, Is.False);

            // Verify update
            subscriberGetRes = await Client.GetAsync(new WebhooksSubscriberGETRequest() { SubscriberID = subscriberID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(subscriberGetRes.Name, Is.EqualTo(subscriberPatchReq.Name));
            Assert.That(subscriberGetRes.IsEnabled, Is.False);

            // Delete subscriber
            await Client.DeleteAsync(new WebhooksSubscriberDELETERequest() { SubscriberID = subscriberID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify deletion
            WebServiceException subscriberDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(new WebhooksSubscriberGETRequest() { SubscriberID = subscriberID });
            });
            Assert.That(subscriberDeleteEx.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}

