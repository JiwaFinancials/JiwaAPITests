using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiwaAPITests.Sessions
{
    public class Sessions : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Sessions_GET()
        {
            // Retrieve the list of sessions
            AuthSessionsGETRequest sessionsReq = new AuthSessionsGETRequest();
            List<JiwaAuthUserSessionResponse> sessionsRes = await Client.GetAsync(sessionsReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(sessionsRes, Is.Not.Null);
        }

        [Test]
        public async Task Sessions_Current_GET()
        {
            // Retrieve the current user session
            AuthCurrentSessionGETRequest currentSessionReq = new AuthCurrentSessionGETRequest();
            JiwaAuthUserSessionResponse currentSessionRes = await Client.GetAsync(currentSessionReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(currentSessionRes, Is.Not.Null);
            Assert.That(currentSessionRes.Id, Is.Not.Null);
        }
        #endregion
    }
}
