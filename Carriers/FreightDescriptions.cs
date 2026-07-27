using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Carriers;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.Carriers
{
    public class FreightDescriptions : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task CarrierFreightDescriptions_CRUD()
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

            // Read carrier freight descriptions before create
            CarrierFreightDescriptionsGETManyRequest freightDescriptionsGetManyReq = new CarrierFreightDescriptionsGETManyRequest()
            {
                CarrierID = carrierCreateRes.CarrierID
            };

            List<CarrierFreightDescription> freightDescriptionsGetManyRes = await Client.GetAsync(freightDescriptionsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Create a carrier freight description
            CarrierFreightDescriptionPOSTRequest freightDescriptionCreateReq = new CarrierFreightDescriptionPOSTRequest()
            {
                CarrierID = carrierCreateRes.CarrierID,
                Description = $"Freight {RandomString(5)}",
                Enabled = true,
                DefaultItem = false
            };

            CarrierFreightDescription freightDescriptionCreateRes = await Client.PostAsync(freightDescriptionCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(freightDescriptionCreateRes.FreightDescriptionID, Is.Not.Null);
            Assert.That(freightDescriptionCreateRes.Description, Is.EqualTo(freightDescriptionCreateReq.Description));

            // Read the created carrier freight description
            CarrierFreightDescriptionGETRequest freightDescriptionGetReq = new CarrierFreightDescriptionGETRequest()
            {
                CarrierID = carrierCreateRes.CarrierID,
                FreightDescriptionID = freightDescriptionCreateRes.FreightDescriptionID
            };

            CarrierFreightDescription freightDescriptionGetRes = await Client.GetAsync(freightDescriptionGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(freightDescriptionGetRes.FreightDescriptionID, Is.EqualTo(freightDescriptionCreateRes.FreightDescriptionID));
            Assert.That(freightDescriptionGetRes.Description, Is.EqualTo(freightDescriptionCreateReq.Description));

            // Read carrier freight descriptions after create
            freightDescriptionsGetManyRes = await Client.GetAsync(freightDescriptionsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(freightDescriptionsGetManyRes.Any(x => x.FreightDescriptionID == freightDescriptionCreateRes.FreightDescriptionID), Is.True);

            // Update the created carrier freight description
            CarrierFreightDescriptionPATCHRequest freightDescriptionPatchReq = new CarrierFreightDescriptionPATCHRequest()
            {
                CarrierID = carrierCreateRes.CarrierID,
                FreightDescriptionID = freightDescriptionCreateRes.FreightDescriptionID,
                Description = $"Updated Freight {RandomString(4)}",
                Enabled = false
            };

            CarrierFreightDescription freightDescriptionPatchRes = await Client.PatchAsync(freightDescriptionPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(freightDescriptionPatchRes.FreightDescriptionID, Is.EqualTo(freightDescriptionCreateRes.FreightDescriptionID));
            Assert.That(freightDescriptionPatchRes.Description, Is.EqualTo(freightDescriptionPatchReq.Description));

            // Verify the carrier freight description was updated
            freightDescriptionGetRes = await Client.GetAsync(freightDescriptionGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(freightDescriptionGetRes.Description, Is.EqualTo(freightDescriptionPatchReq.Description));
            Assert.That(freightDescriptionGetRes.Enabled, Is.EqualTo(freightDescriptionPatchReq.Enabled));

            // Delete the carrier freight description
            CarrierFreightDescriptionDELETERequest freightDescriptionDeleteReq = new CarrierFreightDescriptionDELETERequest()
            {
                CarrierID = carrierCreateRes.CarrierID,
                FreightDescriptionID = freightDescriptionCreateRes.FreightDescriptionID
            };

            await Client.DeleteAsync(freightDescriptionDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the carrier freight description was deleted
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                CarrierFreightDescription deletedFreightDescriptionRes = await Client.GetAsync(freightDescriptionGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read carrier freight descriptions after delete
            freightDescriptionsGetManyRes = await Client.GetAsync(freightDescriptionsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(freightDescriptionsGetManyRes.Any(x => x.FreightDescriptionID == freightDescriptionCreateRes.FreightDescriptionID), Is.False);
        }
        #endregion
    }
}

