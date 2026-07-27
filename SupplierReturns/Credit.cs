using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Threading.Tasks;

namespace JiwaAPITests.SupplierReturns
{
    public class Credit : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task SupplierReturnCredit_FromShipments_NotFound_ForInvalidShipment()
        {
            // Attempt to create a supplier return credit from a non-existent shipment.
            SupplierReturnCreditCreateFromShipmentsRequest creditCreateReq = new SupplierReturnCreditCreateFromShipmentsRequest()
            {
                ShipmentIDs = new[] { Guid.NewGuid().ToString() }
            };

            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.PostAsync(creditCreateReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task SupplierReturnCredit_GET_NotFound_ForInvalidCreditID()
        {
            // Attempt to read a supplier return credit that does not exist.
            SupplierReturnCreditGETRequest creditGetReq = new SupplierReturnCreditGETRequest()
            {
                CreditID = Guid.NewGuid().ToString()
            };

            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(creditGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task SupplierReturnCredit_Activate_NotFound_ForInvalidCreditID()
        {
            // Attempt to activate a supplier return credit that does not exist.
            SupplierReturnCreditACTIVATERequest creditActivateReq = new SupplierReturnCreditACTIVATERequest()
            {
                CreditID = Guid.NewGuid().ToString()
            };

            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.PostAsync(creditActivateReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}
