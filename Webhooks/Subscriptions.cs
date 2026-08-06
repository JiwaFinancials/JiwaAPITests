using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using JiwaFinancials.Jiwa.JiwaServiceModel.Webhooks;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiwaAPITests.Webhooks
{
    public class Subscriptions : JiwaAPITest
    {
        #region "{Subscriptions}"
        [Test]
        public async Task Subscriptions_CRUD()
        {
            // Read available webhook events to get a valid event name
            List<WebHookEvent> eventsGetManyRes = await Client.GetAsync(new WebhooksEventsGETManyRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(eventsGetManyRes, Is.Not.Null.And.Not.Empty);
            string eventName = eventsGetManyRes[0].Name;

            // Create subscriber
            SY_WebhookSubscriber subscriberPostRes = await Client.PostAsync(new WebhooksSubscriberPOSTRequest()
            {
                Name = "Test Subscriber " + RandomString(8),
                IsEnabled = true
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            string subscriberID = subscriberPostRes.RecID.ToString();

            // Create subscription
            WebhooksSubscriptionPOSTRequest subscriptionPostReq = new WebhooksSubscriptionPOSTRequest()
            {
                SubscriberID = subscriberID,
                EventName = eventName,
                URL = "https://example.com/webhook/" + RandomString(8)
            };

            SY_WebhookSubscription subscriptionPostRes = await Client.PostAsync(subscriptionPostReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(subscriptionPostRes.RecID, Is.Not.EqualTo(System.Guid.Empty));
            Assert.That(subscriptionPostRes.EventName, Is.EqualTo(eventName));

            string subscriptionID = subscriptionPostRes.RecID.ToString();

            // Read all subscriptions for subscriber
            List<SY_WebhookSubscription> subscriptionsGetManyRes = await Client.GetAsync(new WebhooksSubscriptionsGETManyRequest() { SubscriberID = subscriberID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(subscriptionsGetManyRes, Is.Not.Null.And.Not.Empty);

            // Read subscription
            Subscription subscriptionGetRes = await Client.GetAsync(new WebhooksSubscriptionGETRequest()
            {
                SubscriberID = subscriberID,
                SubscriptionID = subscriptionID
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(subscriptionGetRes.SubscriptionID.ToString(), Is.EqualTo(subscriptionID));
            Assert.That(subscriptionGetRes.EventName, Is.EqualTo(eventName));

            // Update subscription
            string updatedURL = "https://example.com/webhook/updated/" + RandomString(8);
            WebhooksSubscriptionPATCHRequest subscriptionPatchReq = new WebhooksSubscriptionPATCHRequest()
            {
                SubscriberID = subscriptionGetRes.SubscriberID,
                SubscriptionID = subscriptionGetRes.SubscriptionID,
                URL = updatedURL
            };

            Subscription subscriptionPatchRes = await Client.PatchAsync(subscriptionPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(subscriptionPatchRes.SubscriptionID, Is.EqualTo(subscriptionPatchReq.SubscriptionID));
            Assert.That(subscriptionPatchRes.URL, Is.EqualTo(updatedURL));

            // Verify update
            subscriptionGetRes = await Client.GetAsync(new WebhooksSubscriptionGETRequest()
            {
                SubscriberID = subscriberID,
                SubscriptionID = subscriptionID
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(subscriptionGetRes.URL, Is.EqualTo(updatedURL));

            // Delete subscription
            await Client.DeleteAsync(new WebhooksSubscriptionDELETERequest()
            {
                SubscriberID = subscriberID,
                SubscriptionID = subscriptionID
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify deletion
            WebServiceException subscriptionDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(new WebhooksSubscriptionGETRequest()
                {
                    SubscriberID = subscriberID,
                    SubscriptionID = subscriptionID
                });
            });
            Assert.That(subscriptionDeleteEx.StatusCode, Is.EqualTo(404));

            // Delete subscriber
            await Client.DeleteAsync(new WebhooksSubscriberDELETERequest() { SubscriberID = subscriberID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

