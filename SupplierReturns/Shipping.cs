using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Threading.Tasks;

namespace JiwaAPITests.SupplierReturns
{
    public class Shipping : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task SupplierReturnShipping_GET_NotFound_ForInvalidShippingID()
        {
            // Attempt to read a supplier return shipping that does not exist.
            SupplierReturnShippingGETRequest shippingGetReq = new SupplierReturnShippingGETRequest()
            {
                ShippingID = Guid.NewGuid().ToString()
            };

            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(shippingGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}
