using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Carriers;
using ServiceStack;
using System;
using System.Threading.Tasks;

namespace JiwaAPITests.Carriers
{
    public class Carrier : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Carrier_CRUD()
        {
            // Create a carrier
            CarrierPOSTRequest carrierCreateReq = new CarrierPOSTRequest()
            {
                CarrierName = $"Carrier {RandomString(5)}",
                AccountNo = RandomString(6),
                Enabled = false,
                Notes = "Carrier CRUD Test"
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Carriers.Carrier carrierCreateRes = await Client.PostAsync(carrierCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(carrierCreateRes.CarrierID, Is.Not.Null);
            Assert.That(carrierCreateRes.CarrierName, Is.EqualTo(carrierCreateReq.CarrierName));

            // Read the created carrier
            CarrierGETRequest carrierGetReq = new CarrierGETRequest()
            {
                CarrierID = carrierCreateRes.CarrierID
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Carriers.Carrier carrierGetRes = await Client.GetAsync(carrierGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(carrierGetRes.CarrierID, Is.EqualTo(carrierCreateRes.CarrierID));
            Assert.That(carrierGetRes.CarrierName, Is.EqualTo(carrierCreateReq.CarrierName));

            // Update the created carrier
            CarrierPATCHRequest carrierPatchReq = new CarrierPATCHRequest()
            {
                CarrierID = carrierCreateRes.CarrierID,
                CarrierName = $"Updated Carrier {RandomString(4)}",
                Notes = "Carrier CRUD Updated"
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Carriers.Carrier carrierPatchRes = await Client.PatchAsync(carrierPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(carrierPatchRes.CarrierID, Is.EqualTo(carrierPatchReq.CarrierID));
            Assert.That(carrierPatchRes.CarrierID, Is.EqualTo(carrierCreateRes.CarrierID));
            Assert.That(carrierPatchRes.CarrierName, Is.EqualTo(carrierPatchReq.CarrierName));

            // Verify the carrier was updated
            carrierGetRes = await Client.GetAsync(carrierGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(carrierGetRes.CarrierName, Is.EqualTo(carrierPatchReq.CarrierName));
            Assert.That(carrierGetRes.Notes, Is.EqualTo(carrierPatchReq.Notes));

            // Delete the carrier
            CarrierDELETERequest carrierDeleteReq = new CarrierDELETERequest()
            {
                CarrierID = carrierCreateRes.CarrierID
            };

            await Client.DeleteAsync(carrierDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the carrier was deleted
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.Carriers.Carrier deletedCarrierRes = await Client.GetAsync(carrierGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}


