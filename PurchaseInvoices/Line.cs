using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseInvoiceDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseInvoices.PurchaseInvoice;
using PurchaseInvoiceLineDto = JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseInvoices.PurchaseInvoiceLine;

namespace JiwaAPITests.PurchaseInvoices
{
    public class Line : PurchaseInvoiceTestBase
    {
        #region "PurchaseInvoices_Lines"
        [Test]
        public async Task PurchaseInvoices_Lines_ReadAndUpdate()
        {
            // Create dependencies used to create a purchase invoice.
            (_, _, _, var goodsReceivedNote) = await CreateGoodsReceivedNoteWithDependenciesAsync();

            // Create a purchase invoice that includes lines.
            PurchaseInvoiceDto purchaseInvoiceCreateRes = await CreatePurchaseInvoiceFromGoodsReceivedNoteAsync(goodsReceivedNote);

            // Read all purchase invoice lines.
            PurchaseInvoiceLinesGETManyRequest linesGetManyReq = new PurchaseInvoiceLinesGETManyRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID
            };

            List<PurchaseInvoiceLineDto> linesGetManyRes = await Client.GetAsync(linesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linesGetManyRes.Count, Is.GreaterThan(0));

            PurchaseInvoiceLineDto firstLine = linesGetManyRes.First();
            Assert.That(firstLine.PurchaseInvoiceLineID, Is.Not.Null.And.Not.Empty);
            Assert.That(firstLine.Quantity, Is.Not.Null);

            // Read the created purchase invoice line.
            PurchaseInvoiceLineGETRequest lineGetReq = new PurchaseInvoiceLineGETRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID,
                PurchaseInvoiceLineID = firstLine.PurchaseInvoiceLineID
            };

            PurchaseInvoiceLineDto lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.PurchaseInvoiceLineID, Is.EqualTo(firstLine.PurchaseInvoiceLineID));

            // Update the purchase invoice line quantity.
            decimal updatedQuantity = (lineGetRes.Quantity ?? 0M) + 1M;
            PurchaseInvoiceLinePATCHRequest linePatchReq = new PurchaseInvoiceLinePATCHRequest()
            {
                PurchaseInvoiceID = purchaseInvoiceCreateRes.PurchaseInvoiceID,
                PurchaseInvoiceLineID = lineGetRes.PurchaseInvoiceLineID,
                Quantity = updatedQuantity
            };

            PurchaseInvoiceLineDto linePatchRes = await Client.PatchAsync(linePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linePatchRes.PurchaseInvoiceLineID, Is.EqualTo(lineGetRes.PurchaseInvoiceLineID));
            Assert.That(linePatchRes.Quantity, Is.EqualTo(updatedQuantity));

            // Read the updated purchase invoice line.
            lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.Quantity, Is.EqualTo(updatedQuantity));
        }
        #endregion
    }
}
