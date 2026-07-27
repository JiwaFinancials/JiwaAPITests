using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders;
using NUnit.Framework;
using PurchaseOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.PurchaseOrders
{
    public class Line : PurchaseOrderTestBase
    {
        #region "PurchaseOrders_Lines"
        [Test]
        public async Task PurchaseOrders_Lines_CRUD()
        {
            // Create an inventory item for the purchase order line.
            InventoryPOSTRequest inventoryCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Purchase Order Line Test Item",
                DefaultPrice = 10.00M
            };

            InventoryItem inventoryCreateRes = await Client.PostAsync(inventoryCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inventoryCreateRes.InventoryID, Is.Not.Null);

            // Create a creditor for the purchase order.
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Purchase Order Line Test Creditor"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);

            // Create a purchase order without lines.
            PurchaseOrderPOSTRequest purchaseOrderCreateReq = new PurchaseOrderPOSTRequest()
            {
                CreditorAccountNo = creditorCreateRes.AccountNo,
                Reference = "PO-Lines-" + RandomString(8),
                OrderDate = DateTime.Today
            };

            PurchaseOrderDto purchaseOrderCreateRes = await Client.PostAsync(purchaseOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseOrderCreateRes.PurchaseOrderID, Is.Not.Null);

            // Append a line to the purchase order.
            PurchaseOrderLinePOSTRequest lineCreateReq = new PurchaseOrderLinePOSTRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID,
                PartNo = inventoryCreateRes.PartNo,
                Quantity = 5M
            };

            PurchaseOrderLine lineCreateRes = await Client.PostAsync(lineCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(lineCreateRes.PurchaseOrderLineID, Is.Not.Null);
            Assert.That(lineCreateRes.PartNo, Is.EqualTo(inventoryCreateRes.PartNo));
            Assert.That(lineCreateRes.Quantity, Is.EqualTo(lineCreateReq.Quantity));

            // Read all purchase order lines and ensure the created line is returned.
            PurchaseOrderLinesGETManyRequest linesGetManyReq = new PurchaseOrderLinesGETManyRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID
            };

            List<PurchaseOrderLine> linesGetManyRes = await Client.GetAsync(linesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linesGetManyRes.Any(x => x.PurchaseOrderLineID == lineCreateRes.PurchaseOrderLineID), Is.True);

            // Read the created purchase order line.
            PurchaseOrderLineGETRequest lineGetReq = new PurchaseOrderLineGETRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID,
                PurchaseOrderLineID = lineCreateRes.PurchaseOrderLineID
            };

            PurchaseOrderLine lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.PurchaseOrderLineID, Is.EqualTo(lineCreateRes.PurchaseOrderLineID));
            Assert.That(lineGetRes.PartNo, Is.EqualTo(inventoryCreateRes.PartNo));

            // Update the purchase order line quantity.
            PurchaseOrderLinePATCHRequest linePatchReq = new PurchaseOrderLinePATCHRequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID,
                PurchaseOrderLineID = lineCreateRes.PurchaseOrderLineID,
                Quantity = 10M
            };

            PurchaseOrderLine linePatchRes = await Client.PatchAsync(linePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linePatchRes.PurchaseOrderLineID, Is.EqualTo(lineCreateRes.PurchaseOrderLineID));
            Assert.That(linePatchRes.Quantity, Is.EqualTo(linePatchReq.Quantity));

            // Read the updated purchase order line.
            lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.Quantity, Is.EqualTo(linePatchReq.Quantity));

            // Delete the purchase order line.
            PurchaseOrderLineDELETERequest lineDeleteReq = new PurchaseOrderLineDELETERequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID,
                PurchaseOrderLineID = lineCreateRes.PurchaseOrderLineID
            };

            await Client.DeleteAsync(lineDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the purchase order line was deleted.
            WebServiceException lineDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(lineGetReq);
            });
            Assert.That(lineDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all lines and ensure the deleted line is no longer returned.
            linesGetManyRes = await Client.GetAsync(linesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linesGetManyRes.Any(x => x.PurchaseOrderLineID == lineCreateRes.PurchaseOrderLineID), Is.False);
        }
        #endregion
    }
}

