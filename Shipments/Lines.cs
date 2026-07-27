using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShipmentDto = JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments.Shipment;
using ShipmentLineDto = JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments.ShipmentLine;

namespace JiwaAPITests.Shipments
{
    public class Lines : JiwaAPITest
    {
        #region "{Lines}"
        [Test]
        public async Task ShipmentLines_ReadAndUpdate()
        {
            // Create a shipment.
            LandedCostShipmentPOSTRequest shipmentCreateReq = new LandedCostShipmentPOSTRequest()
            {
                DeliveryNotes = "Shipment " + RandomString(8)
            };

            ShipmentDto shipmentCreateRes = await Client.PostAsync(shipmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentCreateRes.ShipmentID, Is.Not.Null);

            // Read all shipment lines for the created shipment.
            LandedCostShipmentLinesGETManyRequest shipmentLinesGetManyReq = new LandedCostShipmentLinesGETManyRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID
            };

            List<ShipmentLineDto> shipmentLinesGetManyRes = await Client.GetAsync(shipmentLinesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(shipmentLinesGetManyRes, Is.Not.Null);

            // Clean up - delete the shipment.
            LandedCostShipmentDELETERequest shipmentDeleteReq = new LandedCostShipmentDELETERequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID
            };

            await Client.DeleteAsync(shipmentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}
