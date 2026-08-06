using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.BookIns;
using JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookInDto = JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.BookIns.BookIn;

namespace JiwaAPITests.BookIns
{
    public class BookIn : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task BookIn_CRUD()
        {
            // Create a shipment to use for the book in
            LandedCostShipmentPOSTRequest shipmentCreateReq = new LandedCostShipmentPOSTRequest();
            Shipment shipmentCreateRes = await Client.PostAsync(shipmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentCreateRes.ShipmentID, Is.Not.Null);

            // Create a book in from the shipment
            LandedCostBookInCREATEFromShipmentIDRequest bookInCreateReq = new LandedCostBookInCREATEFromShipmentIDRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID
            };

            BookInDto bookInCreateRes = await Client.PostAsync(bookInCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(bookInCreateRes.BookInID, Is.Not.Null);
            Assert.That(bookInCreateRes.Shipment.ShipmentID, Is.EqualTo(shipmentCreateRes.ShipmentID));

            // Read the created book in using the BookInID
            LandedCostBookInGETRequest bookInGetReq = new LandedCostBookInGETRequest()
            {
                BookInID = bookInCreateRes.BookInID
            };

            BookInDto bookInGetRes = await Client.GetAsync(bookInGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(bookInGetRes.BookInID, Is.EqualTo(bookInCreateRes.BookInID));
            Assert.That(bookInGetRes.Shipment.ShipmentID, Is.EqualTo(shipmentCreateRes.ShipmentID));

            // Update the book in
            LandedCostBookInPATCHRequest bookInPatchReq = new LandedCostBookInPATCHRequest()
            {
                BookInID = bookInCreateRes.BookInID,
                BookInDate = DateTime.Today.AddDays(-1)
            };

            BookInDto bookInPatchRes = await Client.PatchAsync(bookInPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(bookInPatchRes.BookInID, Is.EqualTo(bookInPatchReq.BookInID));
            Assert.That(bookInPatchRes.BookInID, Is.EqualTo(bookInCreateRes.BookInID));
            Assert.That(bookInPatchRes.BookInDate, Is.Not.Null);
            Assert.That(bookInPatchReq.BookInDate, Is.Not.Null);
            Assert.That(bookInPatchRes.BookInDate.Value.Date, Is.EqualTo(bookInPatchReq.BookInDate.Value.Date));

            // Read the updated book in and confirm the date was changed
            bookInGetRes = await Client.GetAsync(bookInGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(bookInGetRes.BookInDate, Is.Not.Null);
            Assert.That(bookInGetRes.BookInDate.Value.Date, Is.EqualTo(bookInPatchReq.BookInDate.Value.Date));
        }
        #endregion

        #region "{Activate}"
        [Test]
        public async Task BookIn_Activate()
        {
            // Create a shipment to use for the book in
            LandedCostShipmentPOSTRequest shipmentCreateReq = new LandedCostShipmentPOSTRequest();
            Shipment shipmentCreateRes = await Client.PostAsync(shipmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentCreateRes.ShipmentID, Is.Not.Null);

            // Create a book in from the shipment
            LandedCostBookInCREATEFromShipmentIDRequest bookInCreateReq = new LandedCostBookInCREATEFromShipmentIDRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID
            };

            BookInDto bookInCreateRes = await Client.PostAsync(bookInCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(bookInCreateRes.BookInID, Is.Not.Null);

            // Activate the book in
            LandedCostBookInACTIVATERequest bookInActivateReq = new LandedCostBookInACTIVATERequest()
            {
                BookInID = bookInCreateRes.BookInID
            };

            BookInDto bookInActivateRes = await Client.PostAsync(bookInActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            if (bookInActivateRes != null)
            {
                Assert.That(bookInActivateRes.Activated, Is.True);
            }
        }
        #endregion

        #region "{FromShipmentID}"
        [Test]
        public async Task BookIn_CreateFromShipmentID()
        {
            // Create a shipment to use for the book in
            LandedCostShipmentPOSTRequest shipmentCreateReq = new LandedCostShipmentPOSTRequest();
            Shipment shipmentCreateRes = await Client.PostAsync(shipmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentCreateRes.ShipmentID, Is.Not.Null);

            // Create a book in from the shipment ID
            LandedCostBookInCREATEFromShipmentIDRequest bookInCreateReq = new LandedCostBookInCREATEFromShipmentIDRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID
            };

            BookInDto bookInCreateRes = await Client.PostAsync(bookInCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(bookInCreateRes.BookInID, Is.Not.Null);
            Assert.That(bookInCreateRes.Shipment.ShipmentID, Is.EqualTo(shipmentCreateRes.ShipmentID));
        }
        #endregion

        #region "{FromShipmentNo}"
        [Test]
        public async Task BookIn_CreateFromShipmentNo()
        {
            // Create a shipment to use for the book in
            LandedCostShipmentPOSTRequest shipmentCreateReq = new LandedCostShipmentPOSTRequest();
            Shipment shipmentCreateRes = await Client.PostAsync(shipmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentCreateRes.ShipmentID, Is.Not.Null);
            Assert.That(shipmentCreateRes.ShipmentNo, Is.Not.Null);

            // Create a book in from the shipment No
            LandedCostBookInCREATEFromShipmentNoRequest bookInCreateReq = new LandedCostBookInCREATEFromShipmentNoRequest()
            {
                ShipmentNo = shipmentCreateRes.ShipmentNo
            };

            BookInDto bookInCreateRes = await Client.PostAsync(bookInCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(bookInCreateRes.BookInID, Is.Not.Null);
            Assert.That(bookInCreateRes.Shipment.ShipmentNo, Is.EqualTo(shipmentCreateRes.ShipmentNo));
        }
        #endregion
    }
}


