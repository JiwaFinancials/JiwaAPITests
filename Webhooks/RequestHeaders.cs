using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using JiwaFinancials.Jiwa.JiwaServiceModel.Webhooks;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiwaAPITests.Webhooks
{
    public class RequestHeaders : JiwaAPITest
    {
        #region "{RequestHeaders}"
        [Test]
        public async Task RequestHeaders_Lifecycle()
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
            Guid subscriberGuid = subscriberPostRes.RecID;
            string subscriberID = subscriberGuid.ToString();

            // Create subscription
            SY_WebhookSubscription subscriptionPostRes = await Client.PostAsync(new WebhooksSubscriptionPOSTRequest()
            {
                SubscriberID = subscriberID,
                EventName = eventsGetManyRes[0].Name,
                URL = "https://example.com/webhook/" + RandomString(8)
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Guid subscriptionGuid = subscriptionPostRes.RecID;

            // Set request headers for subscription
            List<SubscriptionRequestHeader> putHeaders = new List<SubscriptionRequestHeader>()
            {
                new SubscriptionRequestHeader() { Name = "X-Test-Header", Value = "TestValue-" + RandomString(6) }
            };

            List<SubscriptionRequestHeader> headersRes = await Client.PutAsync(new WebhooksSubscriptionRequestHeadersPUTRequest()
            {
                SubscriberID = subscriberGuid,
                SubscriptionID = subscriptionGuid,
                RequestHeaders = putHeaders
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(headersRes, Is.Not.Null.And.Not.Empty);
            Assert.That(headersRes[0].Name, Is.EqualTo(putHeaders[0].Name));
            Assert.That(headersRes[0].Value, Is.EqualTo(putHeaders[0].Value));

            Guid headerID = headersRes[0].SubscriptionRequestHeaderID;

            // Delete request header
            await Client.DeleteAsync(new WebhooksSubscriptionRequestHeaderDELETERequest()
            {
                SubscriberID = subscriberGuid,
                SubscriptionID = subscriptionGuid,
                SubscriptionRequestHeaderID = headerID
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify header was removed by setting empty headers list
            List<SubscriptionRequestHeader> headersAfterDelete = await Client.PutAsync(new WebhooksSubscriptionRequestHeadersPUTRequest()
            {
                SubscriberID = subscriberGuid,
                SubscriptionID = subscriptionGuid,
                RequestHeaders = new List<SubscriptionRequestHeader>()
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(headersAfterDelete, Is.Empty);

            // Delete subscriber
            await Client.DeleteAsync(new WebhooksSubscriberDELETERequest() { SubscriberID = subscriberID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}
