using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory.SOH;
using JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder;
using WorkOrderOutputDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrderOutput;

namespace JiwaAPITests.WorkOrders
{
    public class LineDetails : WorkOrderTestBase
    {
        #region "WorkOrders_Outputs_LineDetails"
        [Test]
        public async Task WorkOrders_Outputs_LineDetails_CRUD()
        {
            // Create an item for the output.
            InventoryPOSTRequest outputItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Work Order Output Line Detail Test Item",
                DefaultPrice = 99.99M
            };

            InventoryItem outputItemCreateRes = await Client.PostAsync(outputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(outputItemCreateRes.InventoryID, Is.Not.Null);

            // Create a work order to use for output line detail tests.
            WorkOrderDto workOrderCreateRes = await CreateWorkOrderAsync();

            // Create a serialised work order output.
            WorkOrderOutputPOSTRequest outputCreateReq = new WorkOrderOutputPOSTRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                PartNo = outputItemCreateRes.PartNo,
                Quantity = 1M,
                IsRatio = true,
                IsSerial = true,
                DecimalPlaces = 0,
                Note = "Work order output line detail test output"
            };

            WorkOrderOutputDto outputCreateRes = await Client.PostAsync(outputCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(outputCreateRes.OutputID, Is.Not.Null);

            // Read all line details for the work order output.
            WorkOrderOutputLineDetailsGETManyRequest lineDetailsGetManyReq = new WorkOrderOutputLineDetailsGETManyRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                OutputID = outputCreateRes.OutputID
            };

            List<InventorySOHLineDetail> lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Append a line detail to the work order output.
            WorkOrderOutputLineDetailPOSTRequest lineDetailCreateReq = new WorkOrderOutputLineDetailPOSTRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                OutputID = outputCreateRes.OutputID,
                Quantity = 1M,
                SerialNo = RandomString(8)
            };

            InventorySOHLineDetail lineDetailCreateRes = await Client.PostAsync(lineDetailCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(lineDetailCreateRes.LineDetailID, Is.Not.Null);

            // Read the created line detail.
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            InventorySOHLineDetail persistedLineDetail = lineDetailsGetManyRes.First(x => x.LineDetailID == lineDetailCreateRes.LineDetailID);

            WorkOrderOutputLineDetailGETRequest lineDetailGetReq = new WorkOrderOutputLineDetailGETRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                OutputID = outputCreateRes.OutputID,
                LineDetailID = persistedLineDetail.LineDetailID
            };

            InventorySOHLineDetail lineDetailGetRes = await Client.GetAsync(lineDetailGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailGetRes.LineDetailID, Is.EqualTo(persistedLineDetail.LineDetailID));

            // Update the created line detail.
            WorkOrderOutputLineDetailPATCHRequest lineDetailPatchReq = new WorkOrderOutputLineDetailPATCHRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                OutputID = outputCreateRes.OutputID,
                LineDetailID = persistedLineDetail.LineDetailID,
                SerialNo = RandomString(8)
            };

            InventorySOHLineDetail lineDetailPatchRes = await Client.PatchAsync(lineDetailPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailPatchRes.LineDetailID, Is.EqualTo(lineDetailPatchReq.LineDetailID));
            Assert.That(lineDetailPatchRes.LineDetailID, Is.EqualTo(persistedLineDetail.LineDetailID));
            Assert.That(lineDetailPatchRes.SerialNo, Is.EqualTo(lineDetailPatchReq.SerialNo));

            // Replace line details for the work order output.
            InventorySOHLineDetail lineDetailPutItem = new InventorySOHLineDetail()
            {
                SerialNo = RandomString(8),
                Quantity = lineDetailPatchRes.Quantity,
                DateIn = lineDetailPatchRes.DateIn,
                ExpiryDate = lineDetailPatchRes.ExpiryDate,
                BinLocation = lineDetailPatchRes.BinLocation
            };

            WorkOrderOutputLineDetailPUTRequest lineDetailsPutReq = new WorkOrderOutputLineDetailPUTRequest()
            {
                lineDetailPutItem
            };

            List<InventorySOHLineDetail> lineDetailsPutRes = await Client.PutAsync<List<InventorySOHLineDetail>>($"/WorkOrders/{workOrderCreateRes.WorkOrderID}/Outputs/{outputCreateRes.OutputID}/LineDetails", lineDetailsPutReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsPutRes.Count, Is.EqualTo(1));
            Assert.That(lineDetailsPutRes[0].SerialNo, Is.EqualTo(lineDetailsPutReq[0].SerialNo));

            // Read all line details again and ensure only the replacement detail remains.
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes.Count, Is.EqualTo(1));

            string lineDetailIDToDelete = lineDetailsGetManyRes[0].LineDetailID;

            // Delete the work order output line detail.
            WorkOrderOutputLineDetailDELETERequest lineDetailDeleteReq = new WorkOrderOutputLineDetailDELETERequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                OutputID = outputCreateRes.OutputID,
                LineDetailID = lineDetailIDToDelete
            };

            await Client.DeleteAsync(lineDetailDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the line detail was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(new WorkOrderOutputLineDetailGETRequest()
                {
                    WorkOrderID = workOrderCreateRes.WorkOrderID,
                    OutputID = outputCreateRes.OutputID,
                    LineDetailID = lineDetailIDToDelete
                });
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all line details again and ensure the deleted detail is no longer returned.
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes.Any(x => x.LineDetailID == lineDetailIDToDelete), Is.False);
        }
        #endregion

        #region "WorkOrders_Stages_Inputs_LineDetails"
        [Test]
        public async Task WorkOrders_Stages_Inputs_LineDetails_CRUD()
        {
            // Create an inventory item for the stage input.
            InventoryPOSTRequest inputItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Work Order Stage Input Line Detail Test Item",
                DefaultPrice = 12.50M
            };

            InventoryItem inputItemCreateRes = await Client.PostAsync(inputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inputItemCreateRes.InventoryID, Is.Not.Null);

            // Seed stock for the stage input item so line details can be transacted.
            await EnsureStockOnHandAsync(inputItemCreateRes.PartNo, 5M);

            // Create a work order to use for stage input line detail tests.
            WorkOrderDto workOrderCreateRes = await CreateWorkOrderAsync();
            string workOrderID = workOrderCreateRes.WorkOrderID;
            string stageID = workOrderCreateRes.Stages[0].StageID;

            // Create a stage input.
            WorkOrderInputPOSTRequest inputCreateReq = new WorkOrderInputPOSTRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                PartNo = inputItemCreateRes.PartNo,
                Quantity = 2M,
                IsRatio = true
            };

            WorkOrderInput inputCreateRes = await Client.PostAsync(inputCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inputCreateRes.InputID, Is.Not.Null);

            // Read all line details for the stage input.
            WorkOrderInputLineDetailsGETManyRequest lineDetailsGetManyReq = new WorkOrderInputLineDetailsGETManyRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InputID = inputCreateRes.InputID
            };

            List<InventorySOHLineDetail> lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Append a line detail to the stage input.
            WorkOrderInputLineDetailPOSTRequest lineDetailCreateReq = new WorkOrderInputLineDetailPOSTRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InputID = inputCreateRes.InputID,
                Quantity = 1M
            };

            InventorySOHLineDetail lineDetailCreateRes = await Client.PostAsync(lineDetailCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(lineDetailCreateRes.LineDetailID, Is.Not.Null);

            // Read the created line detail.
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            InventorySOHLineDetail persistedLineDetail = lineDetailsGetManyRes.First(x => x.LineDetailID == lineDetailCreateRes.LineDetailID);

            WorkOrderInputLineDetailGETRequest lineDetailGetReq = new WorkOrderInputLineDetailGETRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InputID = inputCreateRes.InputID,
                LineDetailID = persistedLineDetail.LineDetailID
            };

            InventorySOHLineDetail lineDetailGetRes = await Client.GetAsync(lineDetailGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailGetRes.LineDetailID, Is.EqualTo(persistedLineDetail.LineDetailID));

            // Update the created line detail.
            WorkOrderInputLineDetailPATCHRequest lineDetailPatchReq = new WorkOrderInputLineDetailPATCHRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InputID = inputCreateRes.InputID,
                LineDetailID = persistedLineDetail.LineDetailID,
                Quantity = 2M
            };

            InventorySOHLineDetail lineDetailPatchRes = await Client.PatchAsync(lineDetailPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailPatchRes.LineDetailID, Is.EqualTo(lineDetailPatchReq.LineDetailID));
            Assert.That(lineDetailPatchRes.LineDetailID, Is.EqualTo(persistedLineDetail.LineDetailID));
            Assert.That(lineDetailPatchRes.Quantity, Is.EqualTo(lineDetailPatchReq.Quantity));

            // Replace line details for the stage input line.
            InventorySOHLineDetail lineDetailPutItem = new InventorySOHLineDetail()
            {
                Quantity = 3M,
                DateIn = lineDetailPatchRes.DateIn,
                ExpiryDate = lineDetailPatchRes.ExpiryDate,
                BinLocation = lineDetailPatchRes.BinLocation
            };

            WorkOrderInputLineDetailPUTRequest lineDetailsPutReq = new WorkOrderInputLineDetailPUTRequest()
            {
                lineDetailPutItem
            };

            List<InventorySOHLineDetail> lineDetailsPutRes = await Client.PutAsync<List<InventorySOHLineDetail>>($"/WorkOrders/{workOrderID}/Stages/{stageID}/Inputs/{inputCreateRes.InputID}/LineDetails", lineDetailsPutReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsPutRes.Count, Is.EqualTo(1));
            Assert.That(lineDetailsPutRes[0].Quantity, Is.EqualTo(lineDetailsPutReq[0].Quantity));

            // Read all line details again and ensure only the replacement detail remains.
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes.Count, Is.EqualTo(1));

            string lineDetailIDToDelete = lineDetailsGetManyRes[0].LineDetailID;

            // Delete the stage input line detail.
            WorkOrderInputLineDetailDELETERequest lineDetailDeleteReq = new WorkOrderInputLineDetailDELETERequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InputID = inputCreateRes.InputID,
                LineDetailID = lineDetailIDToDelete
            };

            await Client.DeleteAsync(lineDetailDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the line detail was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(lineDetailGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all line details again and ensure the deleted detail is no longer returned.
            lineDetailsGetManyRes = await Client.GetAsync(lineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(lineDetailsGetManyRes.Any(x => x.LineDetailID == lineDetailIDToDelete), Is.False);
        }
        #endregion
    }
}


