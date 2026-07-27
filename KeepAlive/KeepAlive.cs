using JiwaFinancials.Jiwa.JiwaServiceModel;
using System.Threading.Tasks;

namespace JiwaAPITests.KeepAlive
{
    public class KeepAlive : JiwaAPITest
    {
        #region "Core"
        [Test]
        public async Task KeepAlive_GET()
        {
            // Extend the authenticated user's session.
            await Client.GetAsync(new KeepAliveGETRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

