using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory.SOH;
using JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.BookIns;
using JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.Shipments;
using JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookInDto = JiwaFinancials.Jiwa.JiwaServiceModel.LandedCost.BookIns.BookIn;

namespace JiwaAPITests.BookIns
{
    public class Line : JiwaAPITest
    {
        #region "{Lines}"
        [Test]
        public async Task BookIn_Lines_CRUD()
        {
            // Create an inventory item to use for the purchase order line
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Book In Line Test Item",
                DefaultPrice = 10.00M
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.InventoryID, Is.Not.Null);

            // Create a creditor to use for the purchase order
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Book In Test Creditor"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);

            // Create a purchase order with a line for the inventory item
            PurchaseOrderPOSTRequest purchaseOrderCreateReq = new PurchaseOrderPOSTRequest()
            {
                CreditorAccountNo = creditorCreateRes.AccountNo,
                Lines = new List<PurchaseOrderLine>()
                {
                    new PurchaseOrderLine()
                    {
                        PartNo = itemCreateRes.PartNo,
                        Quantity = 5M
                    }
                }
            };

            PurchaseOrder purchaseOrderCreateRes = await Client.PostAsync(purchaseOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseOrderCreateRes.PurchaseOrderID, Is.Not.Null);
            Assert.That(purchaseOrderCreateRes.Lines.Count, Is.EqualTo(1));

            // Create a shipment
            LandedCostShipmentPOSTRequest shipmentCreateReq = new LandedCostShipmentPOSTRequest();
            Shipment shipmentCreateRes = await Client.PostAsync(shipmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(shipmentCreateRes.ShipmentID, Is.Not.Null);

            // Add the purchase order to the shipment to create shipment lines
            LandedCostShipmentPurchaseOrderPOSTRequest shipmentPOAddReq = new LandedCostShipmentPurchaseOrderPOSTRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID,
                OrderID = purchaseOrderCreateRes.PurchaseOrderID
            };

            await Client.PostAsync(shipmentPOAddReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a book in from the shipment — book in lines are created from the shipment lines
            LandedCostBookInCREATEFromShipmentIDRequest bookInCreateReq = new LandedCostBookInCREATEFromShipmentIDRequest()
            {
                ShipmentID = shipmentCreateRes.ShipmentID
            };

            BookInDto bookInCreateRes = await Client.PostAsync(bookInCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(bookInCreateRes.BookInID, Is.Not.Null);
            Assert.That(bookInCreateRes.Lines.Count, Is.GreaterThan(0));

            string bookInID = bookInCreateRes.BookInID;
            string lineID = bookInCreateRes.Lines[0].LineID;

            // Read all book in lines and ensure the line is returned
            LandedCostBookInLinesGETManyRequest linesGetManyReq = new LandedCostBookInLinesGETManyRequest()
            {
                BookInID = bookInID
            };

            List<BookInLine> linesGetManyRes = await Client.GetAsync(linesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linesGetManyRes.Any(x => x.LineID == lineID), Is.True);

            // Read the book in line using the LineID
            LandedCostBookInLineGETRequest lineGetReq = new LandedCostBookInLineGETRequest()
            {
                BookInID = bookInID,
                LineID = lineID
            };

            BookInLine lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.LineID, Is.EqualTo(lineID));

            // Update the book in line quantity
            LandedCostBookInLinePATCHRequest linePatchReq = new LandedCostBookInLinePATCHRequest()
            {
                BookInID = bookInID,
                LineID = lineID,
                Quantity = 3M
            };

            BookInLine linePatchRes = await Client.PatchAsync(linePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linePatchRes.LineID, Is.EqualTo(lineID));
            Assert.That(linePatchRes.Quantity, Is.EqualTo(linePatchReq.Quantity));

            // Read the updated book in line and confirm the quantity was changed
            lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.Quantity, Is.EqualTo(linePatchReq.Quantity));

            // Read all line details for the book in line
            LandedCostBookInLineDetailsGETManyRequest lineDetailsGetManyReq = new LandedCostBookInLineDetailsGETManyRequest()
            {
                BookInID = bookInID,
                LineID = lineID
            };

            List<InventorySOHLineDetail> lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Append a line detail to the book in line
            LandedCostBookInLineDetailPOSTRequest lineDetailCreateReq = new LandedCostBookInLineDetailPOSTRequest()
            {
                BookInID = bookInID,
                LineID = lineID,
                Quantity = 1M
            };

            InventorySOHLineDetail lineDetailCreateRes = await Client.PostAsync(lineDetailCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(lineDetailCreateRes.LineDetailID, Is.Not.Null);

            // Read the created line detail using the LineDetailID
            LandedCostBookInLineDetailGETRequest lineDetailGetReq = new LandedCostBookInLineDetailGETRequest()
            {
                BookInID = bookInID,
                LineID = lineID,
                LineDetailID = lineDetailCreateRes.LineDetailID
            };

            InventorySOHLineDetail lineDetailGetRes = await Client.GetAsync(lineDetailGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailGetRes.LineDetailID, Is.EqualTo(lineDetailCreateRes.LineDetailID));

            // Read all line details and ensure the created detail is returned
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes.Any(x => x.LineDetailID == lineDetailCreateRes.LineDetailID), Is.True);

            // Update the book in line detail
            LandedCostBookInLineDetailPATCHRequest lineDetailPatchReq = new LandedCostBookInLineDetailPATCHRequest()
            {
                BookInID = bookInID,
                LineID = lineID,
                LineDetailID = lineDetailCreateRes.LineDetailID,
                Quantity = 2M
            };

            InventorySOHLineDetail lineDetailPatchRes = await Client.PatchAsync(lineDetailPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailPatchRes.LineDetailID, Is.EqualTo(lineDetailCreateRes.LineDetailID));

            // Remove the created line detail
            LandedCostBookInLineDetailDELETERequest lineDetailDeleteReq = new LandedCostBookInLineDetailDELETERequest()
            {
                BookInID = bookInID,
                LineID = lineID,
                LineDetailID = lineDetailCreateRes.LineDetailID
            };

            await Client.DeleteAsync(lineDetailDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted line detail is not there anymore
            WebServiceException lineDetailDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                InventorySOHLineDetail getDeletedRes = await Client.GetAsync(lineDetailGetReq);
            });
            Assert.That(lineDetailDeleteEx.StatusCode, Is.EqualTo(404));

            // Remove the book in line
            LandedCostBookInLineDELETERequest lineDeleteReq = new LandedCostBookInLineDELETERequest()
            {
                BookInID = bookInID,
                LineID = lineID
            };

            await Client.DeleteAsync(lineDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted line is no longer returned in the list
            linesGetManyRes = await Client.GetAsync(linesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linesGetManyRes.Any(x => x.LineID == lineID), Is.False);
        }
        #endregion
    }
}

