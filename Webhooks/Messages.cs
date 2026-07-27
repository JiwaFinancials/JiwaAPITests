using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiwaAPITests.Webhooks
{
    public class Messages : JiwaAPITest
    {
        #region "{Messages}"
        [Test]
        public async Task Messages_GET()
        {
            // Create subscriber
            SY_WebhookSubscriber subscriberPostRes = await Client.PostAsync(new WebhooksSubscriberPOSTRequest()
            {
                Name = "Test Subscriber " + RandomString(8),
                IsEnabled = true
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            string subscriberID = subscriberPostRes.RecID.ToString();

            // Read messages for subscriber
            QueryResponse<v_SY_WebhookSubscriber_Messages> messagesGetRes = await Client.GetAsync(new WebhooksMessagesGETManyRequest() { SubscriberID = subscriberID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(messagesGetRes, Is.Not.Null);

            // Read message responses for subscriber
            QueryResponse<v_SY_WebhookSubscriber_MessageResponses> messageResponsesGetRes = await Client.GetAsync(new WebhooksMessageResponsesGETRequest() { SubscriberID = subscriberID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(messageResponsesGetRes, Is.Not.Null);

            // Delete subscriber
            await Client.DeleteAsync(new WebhooksSubscriberDELETERequest() { SubscriberID = subscriberID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }

        [Test]
        public async Task Message_DELETE()
        {
            // Read available webhook events to get a valid event name
            List<WebHookEvent> eventsGetManyRes = await Client.GetAsync(new WebhooksEventsGETManyRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(eventsGetManyRes, Is.Not.Null.And.Not.Empty);

            // Create subscriber
            SY_WebhookSubscriber subscriberPostRes = await Client.PostAsync(new WebhooksSubscriberPOSTRequest()
            {
                Name = "Test Subscriber " + RandomString(8),
                IsEnabled = true
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            string subscriberID = subscriberPostRes.RecID.ToString();

            // Create subscription
            SY_WebhookSubscription subscriptionPostRes = await Client.PostAsync(new WebhooksSubscriptionPOSTRequest()
            {
                SubscriberID = subscriberID,
                EventName = eventsGetManyRes[0].Name,
                URL = "https://example.com/webhook/" + RandomString(8)
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            string subscriptionID = subscriptionPostRes.RecID.ToString();

            // Delete a message (non-existent MessageID)
            await Client.DeleteAsync(new WebhooksMessageDELETERequest()
            {
                SubscriberID = subscriberID,
                SubscriptionID = subscriptionID,
                MessageID = System.Guid.NewGuid().ToString()
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Delete subscriber
            await Client.DeleteAsync(new WebhooksSubscriberDELETERequest() { SubscriberID = subscriberID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}
