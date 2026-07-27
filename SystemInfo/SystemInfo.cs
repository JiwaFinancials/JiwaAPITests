using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace JiwaAPITests.SystemInfo
{
    public class SystemInfo : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task SystemInfo_GET()
        {
            // Retrieve system information.
            SystemInformationGETResponse systemInfoRes = await Client.GetAsync(new SystemInformationGETRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(systemInfoRes, Is.Not.Null);
            Assert.That(systemInfoRes.JiwaVersion, Is.Not.Null.And.Not.Empty);
            Assert.That(systemInfoRes.SQLServerDateTime, Is.Not.EqualTo(default(DateTime)));
        }
        #endregion
    }
}
