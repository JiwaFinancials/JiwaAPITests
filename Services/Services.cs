using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using System.Threading.Tasks;

namespace JiwaAPITests.Services
{
    public class Services : JiwaAPITest
    {
        #region "{Main}"
        // Commented out because running this test will stop the API service, which mens subsequent tests will fail. Uncomment and run this test only when you want to stop the API service.
        //[Test]
        //public async Task Services_Stop_GET()
        //{
        //    // Stop the API service
        //    await Client.GetAsync(new StopRequest());
        //    Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        //}
        #endregion
    }
}
