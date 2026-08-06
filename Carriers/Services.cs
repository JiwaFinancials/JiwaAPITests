using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Carriers;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.Carriers
{
    public class Services : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task CarrierServices_CRUD()
        {
            // Create a carrier
            CarrierPOSTRequest carrierCreateReq = new CarrierPOSTRequest()
            {
                CarrierName = $"Carrier {RandomString(5)}",
                AccountNo = RandomString(6),
                Enabled = false
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Carriers.Carrier carrierCreateRes = await Client.PostAsync(carrierCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(carrierCreateRes.CarrierID, Is.Not.Null);

            // Read the created carrier
            CarrierGETRequest carrierGetReq = new CarrierGETRequest()
            {
                CarrierID = carrierCreateRes.CarrierID
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Carriers.Carrier carrierGetRes = await Client.GetAsync(carrierGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(carrierGetRes.CarrierID, Is.EqualTo(carrierCreateRes.CarrierID));

            // Read carrier services before create
            CarrierServicesGETManyRequest servicesGetManyReq = new CarrierServicesGETManyRequest()
            {
                CarrierID = carrierCreateRes.CarrierID
            };

            List<CarrierService> servicesGetManyRes = await Client.GetAsync(servicesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Create a carrier service
            CarrierServicePOSTRequest serviceCreateReq = new CarrierServicePOSTRequest()
            {
                CarrierID = carrierCreateRes.CarrierID,
                ServiceName = $"Service {RandomString(5)}",
                Enabled = true,
                MaximumWeight = 123.45M,
                DefaultItem = false
            };

            CarrierService serviceCreateRes = await Client.PostAsync(serviceCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(serviceCreateRes.ServiceID, Is.Not.Null);
            Assert.That(serviceCreateRes.ServiceName, Is.EqualTo(serviceCreateReq.ServiceName));

            // Read the created carrier service
            CarrierServiceGETRequest serviceGetReq = new CarrierServiceGETRequest()
            {
                CarrierID = carrierCreateRes.CarrierID,
                ServiceID = serviceCreateRes.ServiceID
            };

            CarrierService serviceGetRes = await Client.GetAsync(serviceGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(serviceGetRes.ServiceID, Is.EqualTo(serviceCreateRes.ServiceID));
            Assert.That(serviceGetRes.ServiceName, Is.EqualTo(serviceCreateReq.ServiceName));

            // Read carrier services after create
            servicesGetManyRes = await Client.GetAsync(servicesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(servicesGetManyRes.Any(x => x.ServiceID == serviceCreateRes.ServiceID), Is.True);

            // Update the created carrier service
            CarrierServicePATCHRequest servicePatchReq = new CarrierServicePATCHRequest()
            {
                CarrierID = carrierCreateRes.CarrierID,
                ServiceID = serviceCreateRes.ServiceID,
                ServiceName = $"Updated Service {RandomString(4)}",
                MaximumWeight = 234.56M,
                Enabled = false
            };

            CarrierService servicePatchRes = await Client.PatchAsync(servicePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(servicePatchRes.ServiceID, Is.EqualTo(servicePatchReq.ServiceID));
            Assert.That(servicePatchRes.ServiceID, Is.EqualTo(serviceCreateRes.ServiceID));
            Assert.That(servicePatchRes.ServiceName, Is.EqualTo(servicePatchReq.ServiceName));

            // Verify the carrier service was updated
            serviceGetRes = await Client.GetAsync(serviceGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(serviceGetRes.ServiceName, Is.EqualTo(servicePatchReq.ServiceName));
            Assert.That(serviceGetRes.MaximumWeight, Is.EqualTo(servicePatchReq.MaximumWeight));

            // Delete the carrier service
            CarrierServiceDELETERequest serviceDeleteReq = new CarrierServiceDELETERequest()
            {
                CarrierID = carrierCreateRes.CarrierID,
                ServiceID = serviceCreateRes.ServiceID
            };

            await Client.DeleteAsync(serviceDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the carrier service was deleted
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                CarrierService deletedServiceRes = await Client.GetAsync(serviceGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read carrier services after delete
            servicesGetManyRes = await Client.GetAsync(servicesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(servicesGetManyRes.Any(x => x.ServiceID == serviceCreateRes.ServiceID), Is.False);
        }
        #endregion
    }
}


