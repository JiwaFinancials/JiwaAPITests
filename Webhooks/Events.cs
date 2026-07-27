using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiwaAPITests.Webhooks
{
    public class Events : JiwaAPITest
    {
        #region "{Events}"
        [Test]
        public async Task WebhooksEvents_GET()
        {
            // Read resource
            List<WebHookEvent> eventsGetManyRes = await Client.GetAsync(new WebhooksEventsGETManyRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(eventsGetManyRes, Is.Not.Null);
        }

        [Test]
        public async Task WebhooksEvents_POST()
        {
            // Read resource
            List<WebHookEvent> eventsGetManyRes = await Client.GetAsync(new WebhooksEventsGETManyRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(eventsGetManyRes, Is.Not.Null.And.Not.Empty);

            // Create resource
            WebhooksEventPOSTRequest eventPostReq = new WebhooksEventPOSTRequest()
            {
                EventName = eventsGetManyRes[0].Name,
                Body = "{}",
                SourceDTOType = "IntegrationTest",
                SourceDTOID = RandomString(8),
                OriginalDTO = "{}"
            };

            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                await Client.PostAsync(eventPostReq);
            });
            Assert.That(ex.ErrorMessage, Does.Contain("Invalid ClientKey provided."));
        }
        #endregion
    }
}
