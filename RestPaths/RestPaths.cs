using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JiwaAPITests.RestPaths
{
    public class RestPaths : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task RestPaths_GET()
        {
            // Retrieve the list of routes
            RestPathsGETManyRequest restPathsReq = new RestPathsGETManyRequest();
            List<RestPath> restPathsRes = await Client.GetAsync(restPathsReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(restPathsRes, Is.Not.Null);
            Assert.That(restPathsRes.Count, Is.GreaterThan(0));
        }
        #endregion
    }
}
