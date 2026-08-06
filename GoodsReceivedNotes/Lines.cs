using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoodsReceivedNoteDto = JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes.GoodsReceivedNote;

namespace JiwaAPITests.GoodsReceivedNotes
{
    public class Lines : GoodsReceivedNotesTestBase
    {
        #region "GoodsReceivedNotes_Lines"
        [Test]
        public async Task GoodsReceivedNotes_Lines_CRUD()
        {
            // Create the required purchase order dependency.
            (_, JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrder) = await CreatePurchaseOrderWithLineAsync();

            // Activate the purchase order before receiving stock from its line.
            await ActivatePurchaseOrderAsync(purchaseOrder.PurchaseOrderID);

            // Create a goods received note from a purchase order line.
            GoodsReceivedNoteDto grnCreateRes = await CreateGoodsReceivedNoteFromPurchaseOrderLineAsync(purchaseOrder, 2M);

            // Read all lines for the goods received note.
            GoodsReceivedNoteLinesGETManyRequest linesGetManyReq = new GoodsReceivedNoteLinesGETManyRequest()
            {
                GRNID = grnCreateRes.GRNID
            };

            List<GoodsReceivedNoteLine> linesGetManyRes = await Client.GetAsync(linesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linesGetManyRes.Count, Is.GreaterThan(0));

            string existingLineID = linesGetManyRes[0].LineID;

            // Read a specific line for the goods received note.
            GoodsReceivedNoteLineGETRequest lineGetReq = new GoodsReceivedNoteLineGETRequest()
            {
                GRNID = grnCreateRes.GRNID,
                LineID = existingLineID
            };

            GoodsReceivedNoteLine lineGetRes = await Client.GetAsync(lineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineGetRes.LineID, Is.EqualTo(existingLineID));

            // Update the existing goods received note line.
            GoodsReceivedNoteLinePATCHRequest linePatchReq = new GoodsReceivedNoteLinePATCHRequest()
            {
                GRNID = grnCreateRes.GRNID,
                LineID = existingLineID,
                Quantity = 1M
            };

            GoodsReceivedNoteLine linePatchRes = await Client.PatchAsync(linePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linePatchRes.LineID, Is.EqualTo(linePatchReq.LineID));
            Assert.That(linePatchRes.LineID, Is.EqualTo(existingLineID));
            Assert.That(linePatchRes.Quantity, Is.EqualTo(linePatchReq.Quantity));

            // Append a line to the goods received note.
            GoodsReceivedNoteLinePOSTRequest lineCreateReq = new GoodsReceivedNoteLinePOSTRequest()
            {
                GRNID = grnCreateRes.GRNID,
                PartNo = purchaseOrder.Lines[0].PartNo,
                Quantity = 1M
            };

            GoodsReceivedNoteLine lineCreateRes = await Client.PostAsync(lineCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(lineCreateRes.LineID, Is.Not.Null);
            Assert.That(lineCreateRes.PartNo, Is.EqualTo(lineCreateReq.PartNo));

            // Read all line details for the created line.
            GoodsReceivedNoteLineDetailsGETManyRequest lineDetailsGetManyReq = new GoodsReceivedNoteLineDetailsGETManyRequest()
            {
                GRNID = grnCreateRes.GRNID,
                LineID = lineCreateRes.LineID
            };

            List<GoodsReceivedNoteLineDetail> lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Append a line detail to the created line.
            GoodsReceivedNoteLineDetailPOSTRequest lineDetailCreateReq = new GoodsReceivedNoteLineDetailPOSTRequest()
            {
                GRNID = grnCreateRes.GRNID,
                LineID = lineCreateRes.LineID,
                Quantity = 1M
            };

            GoodsReceivedNoteLineDetail lineDetailCreateRes = await Client.PostAsync(lineDetailCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(lineDetailCreateRes.LineDetailID, Is.Not.Null);

            // Read the created line detail.
            GoodsReceivedNoteLineDetailGETRequest lineDetailGetReq = new GoodsReceivedNoteLineDetailGETRequest()
            {
                GRNID = grnCreateRes.GRNID,
                LineID = lineCreateRes.LineID,
                LineDetailID = lineDetailCreateRes.LineDetailID
            };

            GoodsReceivedNoteLineDetail lineDetailGetRes = await Client.GetAsync(lineDetailGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailGetRes.LineDetailID, Is.EqualTo(lineDetailCreateRes.LineDetailID));

            // Update the created line detail.
            GoodsReceivedNoteLineDetailPATCHRequest lineDetailPatchReq = new GoodsReceivedNoteLineDetailPATCHRequest()
            {
                GRNID = grnCreateRes.GRNID,
                LineID = lineCreateRes.LineID,
                LineDetailID = lineDetailCreateRes.LineDetailID,
                Quantity = 2M
            };

            GoodsReceivedNoteLineDetail lineDetailPatchRes = await Client.PatchAsync(lineDetailPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailPatchRes.LineDetailID, Is.EqualTo(lineDetailPatchReq.LineDetailID));
            Assert.That(lineDetailPatchRes.LineDetailID, Is.EqualTo(lineDetailCreateRes.LineDetailID));
            Assert.That(lineDetailPatchRes.Quantity, Is.EqualTo(lineDetailPatchReq.Quantity));

            // Replace line details for the created line.
            GoodsReceivedNoteLineDetailPUTRequest lineDetailsPutReq = new GoodsReceivedNoteLineDetailPUTRequest()
            {
                new GoodsReceivedNoteLineDetail()
                {
                    Quantity = 3M
                }
            };

            List<GoodsReceivedNoteLineDetail> lineDetailsPutRes = await Client.PutAsync<List<GoodsReceivedNoteLineDetail>>($"/GoodsReceivedNotes/{grnCreateRes.GRNID}/Lines/{lineCreateRes.LineID}/LineDetails", lineDetailsPutReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsPutRes.Count, Is.EqualTo(1));
            Assert.That(lineDetailsPutRes[0].Quantity, Is.EqualTo(3M));

            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes.Count, Is.EqualTo(1));

            string lineDetailIDToDelete = lineDetailsGetManyRes[0].LineDetailID;

            // Delete a line detail from the goods received note line.
            GoodsReceivedNoteLineDetailDELETERequest lineDetailDeleteReq = new GoodsReceivedNoteLineDetailDELETERequest()
            {
                GRNID = grnCreateRes.GRNID,
                LineID = lineCreateRes.LineID,
                LineDetailID = lineDetailIDToDelete
            };

            await Client.DeleteAsync(lineDetailDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the line detail was deleted.
            WebServiceException lineDetailDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(new GoodsReceivedNoteLineDetailGETRequest()
                {
                    GRNID = grnCreateRes.GRNID,
                    LineID = lineCreateRes.LineID,
                    LineDetailID = lineDetailIDToDelete
                });
            });
            Assert.That(lineDetailDeleteEx.StatusCode, Is.EqualTo(404));

            // Delete the appended line from the goods received note.
            GoodsReceivedNoteLineDELETERequest lineDeleteReq = new GoodsReceivedNoteLineDELETERequest()
            {
                GRNID = grnCreateRes.GRNID,
                LineID = lineCreateRes.LineID
            };

            await Client.DeleteAsync(lineDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the line was deleted.
            WebServiceException lineDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(new GoodsReceivedNoteLineGETRequest()
                {
                    GRNID = grnCreateRes.GRNID,
                    LineID = lineCreateRes.LineID
                });
            });
            Assert.That(lineDeleteEx.StatusCode, Is.EqualTo(404));

            linesGetManyRes = await Client.GetAsync(linesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(linesGetManyRes.Any(x => x.LineID == lineCreateRes.LineID), Is.False);
        }
        #endregion
    }
}


