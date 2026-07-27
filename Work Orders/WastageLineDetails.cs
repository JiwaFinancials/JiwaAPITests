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
    public class WastageLineDetails : WorkOrderTestBase
    {
        #region "WorkOrders_Outputs_WastageLineDetails"
        [Test]
        public async Task WorkOrders_Outputs_WastageLineDetails_CRUD()
        {
            // Create an item for the output.
            InventoryPOSTRequest outputItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Work Order Wastage Line Detail Test Item",
                DefaultPrice = 99.99M
            };

            InventoryItem outputItemCreateRes = await Client.PostAsync(outputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(outputItemCreateRes.InventoryID, Is.Not.Null);

            // Create a work order to use for wastage line detail tests.
            WorkOrderDto workOrderCreateRes = await CreateWorkOrderAsync();

            // Create a serial/expiry-enabled work order output.
            WorkOrderOutputPOSTRequest outputCreateReq = new WorkOrderOutputPOSTRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                PartNo = outputItemCreateRes.PartNo,
                Quantity = 1M,
                IsRatio = true,
                IsSerial = true,
                UseExpiryDate = true,
                DecimalPlaces = 0,
                Note = "Work order wastage line detail test output"
            };

            WorkOrderOutputDto outputCreateRes = await Client.PostAsync(outputCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(outputCreateRes.OutputID, Is.Not.Null);

            // Read all wastage line details for the work order output.
            WorkOrderOutputWastageLineDetailsGETManyRequest wastageLineDetailsGetManyReq = new WorkOrderOutputWastageLineDetailsGETManyRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                OutputID = outputCreateRes.OutputID
            };

            List<InventorySOHLineDetail> wastageLineDetailsGetManyRes = await Client.GetAsync(wastageLineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Append a wastage line detail to the work order output.
            WorkOrderOutputWastageLineDetailPOSTRequest wastageLineDetailCreateReq = new WorkOrderOutputWastageLineDetailPOSTRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                OutputID = outputCreateRes.OutputID,
                Quantity = 1M,
                SerialNo = RandomString(8),
                ExpiryDate = DateTime.Today.AddDays(1)
            };

            InventorySOHLineDetail wastageLineDetailCreateRes = await Client.PostAsync(wastageLineDetailCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(wastageLineDetailCreateRes.LineDetailID, Is.Not.Null);

            // Read the created wastage line detail.
            wastageLineDetailsGetManyRes = await Client.GetAsync(wastageLineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            InventorySOHLineDetail persistedWastageLineDetail = wastageLineDetailsGetManyRes.First(x => x.LineDetailID == wastageLineDetailCreateRes.LineDetailID);

            WorkOrderOutputWastageLineDetailGETRequest wastageLineDetailGetReq = new WorkOrderOutputWastageLineDetailGETRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                OutputID = outputCreateRes.OutputID,
                LineDetailID = persistedWastageLineDetail.LineDetailID
            };

            InventorySOHLineDetail wastageLineDetailGetRes = await Client.GetAsync(wastageLineDetailGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(wastageLineDetailGetRes.LineDetailID, Is.EqualTo(persistedWastageLineDetail.LineDetailID));

            // Update the created wastage line detail.
            WorkOrderOutputWastageLineDetailPATCHRequest wastageLineDetailPatchReq = new WorkOrderOutputWastageLineDetailPATCHRequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                OutputID = outputCreateRes.OutputID,
                LineDetailID = persistedWastageLineDetail.LineDetailID,
                SerialNo = RandomString(8),
                ExpiryDate = DateTime.Today.AddDays(2)
            };

            InventorySOHLineDetail wastageLineDetailPatchRes = await Client.PatchAsync(wastageLineDetailPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(wastageLineDetailPatchRes.LineDetailID, Is.EqualTo(persistedWastageLineDetail.LineDetailID));
            Assert.That(wastageLineDetailPatchRes.SerialNo, Is.EqualTo(wastageLineDetailPatchReq.SerialNo));

            // Replace wastage line details for the work order output.
            InventorySOHLineDetail wastageLineDetailPutItem = new InventorySOHLineDetail()
            {
                SerialNo = RandomString(8),
                Quantity = wastageLineDetailPatchRes.Quantity,
                ExpiryDate = wastageLineDetailPatchRes.ExpiryDate,
                DateIn = wastageLineDetailPatchRes.DateIn,
                BinLocation = wastageLineDetailPatchRes.BinLocation
            };

            WorkOrderOutputWastageLineDetailPUTRequest wastageLineDetailsPutReq = new WorkOrderOutputWastageLineDetailPUTRequest()
            {
                wastageLineDetailPutItem
            };

            List<InventorySOHLineDetail> wastageLineDetailsPutRes = await Client.PutAsync<List<InventorySOHLineDetail>>($"/WorkOrders/{workOrderCreateRes.WorkOrderID}/Outputs/{outputCreateRes.OutputID}/WastageLineDetails", wastageLineDetailsPutReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(wastageLineDetailsPutRes.Count, Is.EqualTo(1));
            Assert.That(wastageLineDetailsPutRes[0].SerialNo, Is.EqualTo(wastageLineDetailsPutReq[0].SerialNo));

            // Read all wastage line details again and ensure only the replacement detail remains.
            wastageLineDetailsGetManyRes = await Client.GetAsync(wastageLineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(wastageLineDetailsGetManyRes.Count, Is.EqualTo(1));

            string wastageLineDetailIDToDelete = wastageLineDetailsGetManyRes[0].LineDetailID;

            // Delete the work order output wastage line detail.
            WorkOrderOutputWastageLineDetailDELETERequest wastageLineDetailDeleteReq = new WorkOrderOutputWastageLineDetailDELETERequest()
            {
                WorkOrderID = workOrderCreateRes.WorkOrderID,
                OutputID = outputCreateRes.OutputID,
                LineDetailID = wastageLineDetailIDToDelete
            };

            await Client.DeleteAsync(wastageLineDetailDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the wastage line detail was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(new WorkOrderOutputWastageLineDetailGETRequest()
                {
                    WorkOrderID = workOrderCreateRes.WorkOrderID,
                    OutputID = outputCreateRes.OutputID,
                    LineDetailID = wastageLineDetailIDToDelete
                });
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all wastage line details again and ensure the deleted detail is no longer returned.
            wastageLineDetailsGetManyRes = await Client.GetAsync(wastageLineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(wastageLineDetailsGetManyRes.Any(x => x.LineDetailID == wastageLineDetailIDToDelete), Is.False);
        }
        #endregion

        #region "WorkOrders_Stages_Inputs_WastageLineDetails"
        [Test]
        public async Task WorkOrders_Stages_Inputs_WastageLineDetails_CRUD()
        {
            // Create an inventory item for the stage input.
            InventoryPOSTRequest inputItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Work Order Stage Input Wastage Line Detail Test Item",
                DefaultPrice = 12.50M
            };

            InventoryItem inputItemCreateRes = await Client.PostAsync(inputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inputItemCreateRes.InventoryID, Is.Not.Null);

            // Seed stock for the stage input item so wastage line details can be transacted.
            await EnsureStockOnHandAsync(inputItemCreateRes.PartNo, 5M);

            // Create a work order to use for stage input wastage line detail tests.
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

            // Read all wastage line details for the stage input.
            WorkOrderInputWastageLineDetailsGETManyRequest wastageLineDetailsGetManyReq = new WorkOrderInputWastageLineDetailsGETManyRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InputID = inputCreateRes.InputID
            };

            List<InventorySOHLineDetail> wastageLineDetailsGetManyRes = await Client.GetAsync(wastageLineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Append a wastage line detail to the stage input.
            WorkOrderInputWastageLineDetailPOSTRequest wastageLineDetailCreateReq = new WorkOrderInputWastageLineDetailPOSTRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InputID = inputCreateRes.InputID,
                Quantity = 1M
            };

            InventorySOHLineDetail wastageLineDetailCreateRes = await Client.PostAsync(wastageLineDetailCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(wastageLineDetailCreateRes.LineDetailID, Is.Not.Null);

            // Read the created wastage line detail.
            wastageLineDetailsGetManyRes = await Client.GetAsync(wastageLineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            InventorySOHLineDetail persistedWastageLineDetail = wastageLineDetailsGetManyRes.First(x => x.LineDetailID == wastageLineDetailCreateRes.LineDetailID);

            WorkOrderInputWastageLineDetailGETRequest wastageLineDetailGetReq = new WorkOrderInputWastageLineDetailGETRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InputID = inputCreateRes.InputID,
                LineDetailID = persistedWastageLineDetail.LineDetailID
            };

            InventorySOHLineDetail wastageLineDetailGetRes = await Client.GetAsync(wastageLineDetailGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(wastageLineDetailGetRes.LineDetailID, Is.EqualTo(persistedWastageLineDetail.LineDetailID));

            // Update the created wastage line detail.
            WorkOrderInputWastageLineDetailPATCHRequest wastageLineDetailPatchReq = new WorkOrderInputWastageLineDetailPATCHRequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InputID = inputCreateRes.InputID,
                LineDetailID = persistedWastageLineDetail.LineDetailID,
                Quantity = 2M
            };

            InventorySOHLineDetail wastageLineDetailPatchRes = await Client.PatchAsync(wastageLineDetailPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(wastageLineDetailPatchRes.LineDetailID, Is.EqualTo(persistedWastageLineDetail.LineDetailID));
            Assert.That(wastageLineDetailPatchRes.Quantity, Is.EqualTo(wastageLineDetailPatchReq.Quantity));

            // Replace wastage line details for the stage input line.
            InventorySOHLineDetail wastageLineDetailPutItem = new InventorySOHLineDetail()
            {
                Quantity = 3M,
                DateIn = wastageLineDetailPatchRes.DateIn,
                ExpiryDate = wastageLineDetailPatchRes.ExpiryDate,
                BinLocation = wastageLineDetailPatchRes.BinLocation
            };

            WorkOrderInputWastageLineDetailPUTRequest wastageLineDetailsPutReq = new WorkOrderInputWastageLineDetailPUTRequest()
            {
                wastageLineDetailPutItem
            };

            List<InventorySOHLineDetail> wastageLineDetailsPutRes = await Client.PutAsync<List<InventorySOHLineDetail>>($"/WorkOrders/{workOrderID}/Stages/{stageID}/Inputs/{inputCreateRes.InputID}/WastageLineDetails", wastageLineDetailsPutReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(wastageLineDetailsPutRes.Count, Is.EqualTo(1));
            Assert.That(wastageLineDetailsPutRes[0].Quantity, Is.EqualTo(wastageLineDetailsPutReq[0].Quantity));

            // Read all wastage line details again and ensure only the replacement detail remains.
            wastageLineDetailsGetManyRes = await Client.GetAsync(wastageLineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(wastageLineDetailsGetManyRes.Count, Is.EqualTo(1));

            string wastageLineDetailIDToDelete = wastageLineDetailsGetManyRes[0].LineDetailID;

            // Delete the stage input wastage line detail.
            WorkOrderInputWastageLineDetailDELETERequest wastageLineDetailDeleteReq = new WorkOrderInputWastageLineDetailDELETERequest()
            {
                WorkOrderID = workOrderID,
                StageID = stageID,
                InputID = inputCreateRes.InputID,
                LineDetailID = wastageLineDetailIDToDelete
            };

            await Client.DeleteAsync(wastageLineDetailDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the wastage line detail was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(wastageLineDetailGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all wastage line details again and ensure the deleted detail is no longer returned.
            wastageLineDetailsGetManyRes = await Client.GetAsync(wastageLineDetailsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(wastageLineDetailsGetManyRes.Any(x => x.LineDetailID == wastageLineDetailIDToDelete), Is.False);
        }
        #endregion
    }
}

