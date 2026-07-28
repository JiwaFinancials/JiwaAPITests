using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders;
using JiwaFinancials.Jiwa.JiwaServiceModel.Bills;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaAPITests.ServiceManager;
using JiwaAPITests.WorkOrders;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRStaffDto = JiwaFinancials.Jiwa.JiwaServiceModel.Tables.HR_Staff;
using StaffTimesheetServiceManagerTaskDto = JiwaFinancials.Jiwa.JiwaServiceModel.Staff.Timesheets.TimesheetServiceManagerTask;
using StaffTimesheetDto = JiwaFinancials.Jiwa.JiwaServiceModel.Staff.Timesheets.Timesheet;
using StaffTimesheetLineDto = JiwaFinancials.Jiwa.JiwaServiceModel.Staff.Timesheets.TimesheetLine;
using StaffTimesheetWorkOrderStageDto = JiwaFinancials.Jiwa.JiwaServiceModel.Staff.Timesheets.TimesheetWorkOrderStage;
using WorkOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.WorkOrders.WorkOrder;

namespace JiwaAPITests.Staff.Timesheets
{
    public class Lines : ServiceManagerTestBase
    {
        private static DateTimeOffset ConvertToApiPersistedValue(DateTimeOffset value)
        {
            return value.Subtract(value.Offset);
        }

        private static void AssertApiTimeEquals(DateTimeOffset? actual, DateTimeOffset expectedRequestValue)
        {
            Assert.That(actual, Is.Not.Null);
            DateTimeOffset expectedPersistedValue = ConvertToApiPersistedValue(expectedRequestValue);
            Assert.That(actual!.Value.TimeOfDay, Is.EqualTo(expectedPersistedValue.TimeOfDay));
        }

        private async Task<WorkOrderDto> CreateWorkOrderForTimesheetAsync()
        {
            // Create an item for the output
            InventoryPOSTRequest outputItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Timesheet Output Item Test",
                DefaultPrice = 99.99M
            };

            InventoryItem outputItemCreateRes = await Client.PostAsync(outputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(outputItemCreateRes.InventoryID, Is.Not.Null);

            // Create an input item
            InventoryPOSTRequest inputItemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Timesheet Input Item Test",
                DefaultPrice = 12.75M
            };

            InventoryItem inputItemCreateRes = await Client.PostAsync(inputItemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(inputItemCreateRes.InventoryID, Is.Not.Null);

            // Create a bill
            BillPOSTRequest billCreateReq = new BillPOSTRequest()
            {
                Stages = new List<BillStage>()
                {
                    new BillStage()
                    {
                        Name = "Stage 1",
                        Inputs = new List<BillInput>()
                        {
                            new BillInput()
                            {
                                PartNo = inputItemCreateRes.PartNo,
                                Quantity = 1,
                                IsRatio = true
                            }
                        }
                    }
                },
                Outputs = new List<BillOutput>()
                {
                    new BillOutput()
                    {
                        PartNo = outputItemCreateRes.PartNo,
                        Quantity = 1,
                        IsRatio = true
                    }
                }
            };

            Bill billCreateRes = await Client.PostAsync(billCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(billCreateRes.BillID, Is.Not.Null);

            // Create a work order
            WorkOrderPOSTRequest workOrderCreateReq = new WorkOrderPOSTRequest()
            {
                BillID = billCreateRes.BillID
            };

            WorkOrderDto workOrderCreateRes = await Client.PostAsync(workOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(workOrderCreateRes.WorkOrderID, Is.Not.Null);

            return workOrderCreateRes;
        }

        #region "{Main}"
        [Test]
        public async Task StaffTimesheet_Lines_CRUD()
        {
            string createdTimesheetID = null;
            string createdServiceManagerJobID = null;

            try
            {
                // Read the Admin staff member to use for timesheet creation.
                HR_StaffQuery adminStaffQueryReq = new HR_StaffQuery()
                {
                    Username = "Admin",
                    Take = 1
                };

                QueryResponse<HRStaffDto> adminStaffQueryRes = await Client.GetAsync(adminStaffQueryReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                HRStaffDto adminStaff = adminStaffQueryRes.Results.FirstOrDefault();
                Assert.That(adminStaff, Is.Not.Null);
                Assert.That(adminStaff.StaffID, Is.Not.Null.And.Not.Empty);

                // Create a service manager job and use its task 1 for timesheet line operations.
                Job serviceManagerJobCreateRes = await CreateJobAsync();
                createdServiceManagerJobID = serviceManagerJobCreateRes.JobID;

                ServiceManagerTasksGETManyRequest serviceManagerTasksGetManyReq = new ServiceManagerTasksGETManyRequest()
                {
                    JobID = serviceManagerJobCreateRes.JobID
                };

                List<ServiceManagerTask> serviceManagerTasksGetManyRes = await Client.GetAsync(serviceManagerTasksGetManyReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

                if (serviceManagerTasksGetManyRes.Count == 0)
                {
                    _ = await CreateTaskAsync(serviceManagerJobCreateRes.JobID);
                    serviceManagerTasksGetManyRes = await Client.GetAsync(serviceManagerTasksGetManyReq);
                    Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                }

                ServiceManagerTask serviceManagerTaskOne = serviceManagerTasksGetManyRes
                    .OrderBy(x => x.ItemNo ?? int.MaxValue)
                    .FirstOrDefault(x => x.ItemNo == 1)
                    ?? serviceManagerTasksGetManyRes.FirstOrDefault();

                Assert.That(serviceManagerTaskOne, Is.Not.Null);
                Assert.That(serviceManagerTaskOne.TaskID, Is.Not.Null.And.Not.Empty);

                StaffTimesheetServiceManagerTaskDto timesheetServiceManagerTask = new StaffTimesheetServiceManagerTaskDto()
                {
                    TaskID = serviceManagerTaskOne.TaskID,
                    TaskNo = 1,
                    Description = serviceManagerTaskOne.Description
                };

                DateTimeOffset timeSheetDate = new DateTimeOffset(DateTime.UtcNow.Date);

                // Create a staff timesheet.
                StaffTimesheetPOSTRequest timesheetCreateReq = new StaffTimesheetPOSTRequest()
                {
                    StaffID = adminStaff.StaffID,
                    StaffUserName = adminStaff.Username,
                    TimeSheetDate = timeSheetDate,
                    Reference = "Timesheet Lines " + RandomString(6),
                    IsActivated = false
                };

                StaffTimesheetDto timesheetCreateRes = await Client.PostAsync(timesheetCreateReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                Assert.That(timesheetCreateRes.TimesheetID, Is.Not.Null.And.Not.Empty);
                createdTimesheetID = timesheetCreateRes.TimesheetID;

                // Append a line to the staff timesheet.
                //DateTimeOffset? lineCreateStartTime = new DateTimeOffset(
                //    DateTimeOffset.Now.Year,
                //    DateTimeOffset.Now.Month,
                //    DateTimeOffset.Now.Day,
                //    8,
                //    30,
                //    0, DateTimeOffset.Now.Offset);

                DateTimeOffset? lineCreateStartTime = new DateTimeOffset(timeSheetDate.Year, timeSheetDate.Month, timeSheetDate.Day, 13, 0, 0, TimeZoneInfo.Local.BaseUtcOffset);
                //DateTimeOffset? lineCreateStartTime = new DateTimeOffset(value.Value.Year, value.Value.Month, value.Value.Day, value.Value.Hour, value.Value.Minute, 0, TimeZoneInfo.Local.BaseUtcOffset); // Remove milliseconds and seconds

                StaffTimesheetLinePOSTRequest lineCreateReq = new StaffTimesheetLinePOSTRequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    Description = "Timesheet line " + RandomString(6),
                    StartTime = lineCreateStartTime,
                    EndTime = lineCreateStartTime.Value!.AddHours(1),
                    ServiceManagerTask = timesheetServiceManagerTask
                };

                StaffTimesheetLineDto lineCreateRes = await Client.PostAsync(lineCreateReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                Assert.That(lineCreateRes.TimesheetLineID, Is.Not.Null.And.Not.Empty);
                Assert.That(lineCreateRes.Description, Is.EqualTo(lineCreateReq.Description));
                AssertApiTimeEquals(lineCreateRes.StartTime, lineCreateReq.StartTime!.Value);
                AssertApiTimeEquals(lineCreateRes.EndTime, lineCreateReq.EndTime!.Value);

                // Read all lines for the staff timesheet.
                StaffTimesheetLinesGETManyRequest linesGetManyReq = new StaffTimesheetLinesGETManyRequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID
                };

                List<StaffTimesheetLineDto> linesGetManyRes = await Client.GetAsync(linesGetManyReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(linesGetManyRes.Any(x => x.TimesheetLineID == lineCreateRes.TimesheetLineID), Is.True);

                // Read the appended line.
                StaffTimesheetLineGETRequest lineGetReq = new StaffTimesheetLineGETRequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    TimesheetLineID = lineCreateRes.TimesheetLineID
                };

                StaffTimesheetLineDto lineGetRes = await Client.GetAsync(lineGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(lineGetRes.TimesheetLineID, Is.EqualTo(lineCreateRes.TimesheetLineID));
                Assert.That(lineGetRes.Description, Is.EqualTo(lineCreateReq.Description));

                // Update the line with PATCH.
                DateTimeOffset linePatchStartTime = new DateTimeOffset(timeSheetDate.Year, timeSheetDate.Month, timeSheetDate.Day, 15, 0, 0, TimeZoneInfo.Local.BaseUtcOffset);

                StaffTimesheetLinePATCHRequest linePatchReq = new StaffTimesheetLinePATCHRequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    TimesheetLineID = lineCreateRes.TimesheetLineID,
                    Description = "Updated line " + RandomString(6),
                    StartTime = linePatchStartTime,
                    EndTime = linePatchStartTime.AddHours(1),
                    ServiceManagerTask = timesheetServiceManagerTask
                };

                StaffTimesheetLineDto linePatchRes = await Client.PatchAsync(linePatchReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(linePatchRes.TimesheetLineID, Is.EqualTo(lineCreateRes.TimesheetLineID));
                Assert.That(linePatchRes.Description, Is.EqualTo(linePatchReq.Description));

                // Verify the line was updated with PATCH.
                lineGetRes = await Client.GetAsync(lineGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(lineGetRes.Description, Is.EqualTo(linePatchReq.Description));
                AssertApiTimeEquals(lineGetRes.StartTime, linePatchReq.StartTime!.Value);
                AssertApiTimeEquals(lineGetRes.EndTime, linePatchReq.EndTime!.Value);

                // Update the line with PUT.
                DateTimeOffset linePutStartTime = new DateTimeOffset(timeSheetDate.Year, timeSheetDate.Month, timeSheetDate.Day, 17, 0, 0, TimeZoneInfo.Local.BaseUtcOffset);

                StaffTimesheetLinePUTRequest linePutReq = new StaffTimesheetLinePUTRequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    TimesheetLineID = lineCreateRes.TimesheetLineID,
                    Description = "Replaced line " + RandomString(6),
                    StartTime = linePutStartTime,
                    EndTime = linePutStartTime.AddHours(2),
                    ServiceManagerTask = timesheetServiceManagerTask
                };

                StaffTimesheetLineDto linePutRes = await Client.PutAsync(linePutReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(linePutRes.TimesheetLineID, Is.EqualTo(lineCreateRes.TimesheetLineID));
                Assert.That(linePutRes.Description, Is.EqualTo(linePutReq.Description));

                // Verify the line was updated with PUT.
                lineGetRes = await Client.GetAsync(lineGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(lineGetRes.Description, Is.EqualTo(linePutReq.Description));
                AssertApiTimeEquals(lineGetRes.StartTime, linePutReq.StartTime!.Value);
                AssertApiTimeEquals(lineGetRes.EndTime, linePutReq.EndTime!.Value);

                // Create a line using dynamic route behavior for existing or new daily timesheets.
                DateTimeOffset dynamicLineCreateStartTime = new DateTimeOffset(timeSheetDate.Year, timeSheetDate.Month, timeSheetDate.Day, 20, 0, 0, TimeZoneInfo.Local.BaseUtcOffset);

                StaffTimesheetLineDynamicPOSTRequest dynamicLineCreateReq = new StaffTimesheetLineDynamicPOSTRequest()
                {
                    StaffID = adminStaff.StaffID,
                    Description = "Dynamic line " + RandomString(6),
                    StartTime = dynamicLineCreateStartTime,
                    EndTime = dynamicLineCreateStartTime.AddMinutes(30),
                    ServiceManagerTask = timesheetServiceManagerTask
                };

                StaffTimesheetDto dynamicLineCreateRes = await Client.PostAsync(dynamicLineCreateReq);
                Assert.That(LastHttpStatusCode == System.Net.HttpStatusCode.OK || LastHttpStatusCode == System.Net.HttpStatusCode.Created, Is.True);
                Assert.That(dynamicLineCreateRes.TimesheetID, Is.Not.Null.And.Not.Empty);
                Assert.That(dynamicLineCreateRes.Lines.Any(x => x.Description == dynamicLineCreateReq.Description), Is.True);

                // Delete the appended line.
                StaffTimesheetLineDELETERequest lineDeleteReq = new StaffTimesheetLineDELETERequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    TimesheetLineID = lineCreateRes.TimesheetLineID
                };

                await Client.DeleteAsync(lineDeleteReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

                // Verify the line was deleted.
                WebServiceException lineDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
                {
                    _ = await Client.GetAsync(lineGetReq);
                });
                Assert.That(lineDeleteEx.StatusCode, Is.EqualTo(404));

                // Read all lines and ensure the deleted line is no longer returned.
                linesGetManyRes = await Client.GetAsync(linesGetManyReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(linesGetManyRes.Any(x => x.TimesheetLineID == lineCreateRes.TimesheetLineID), Is.False);

                // Delete a dynamically-created timesheet when it differs from the original timesheet.
                if (dynamicLineCreateRes.TimesheetID != timesheetCreateRes.TimesheetID)
                {
                    StaffTimesheetDELETERequest dynamicTimesheetDeleteReq = new StaffTimesheetDELETERequest()
                    {
                        TimesheetID = dynamicLineCreateRes.TimesheetID
                    };

                    await Client.DeleteAsync(dynamicTimesheetDeleteReq);
                    Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
                }

                // Create a work order for timesheet line operations with work order stage.
                WorkOrderDto workOrderCreateRes = await CreateWorkOrderForTimesheetAsync();
                string createdWorkOrderID = workOrderCreateRes.WorkOrderID;

                // Read all stages for the work order.
                WorkOrderStagesGETManyRequest workOrderStagesGetManyReq = new WorkOrderStagesGETManyRequest()
                {
                    WorkOrderID = workOrderCreateRes.WorkOrderID
                };

                List<WorkOrderStage> workOrderStagesGetManyRes = await Client.GetAsync(workOrderStagesGetManyReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(workOrderStagesGetManyRes.Count, Is.GreaterThan(0));

                WorkOrderStage workOrderStageOne = workOrderStagesGetManyRes.FirstOrDefault();
                Assert.That(workOrderStageOne, Is.Not.Null);
                Assert.That(workOrderStageOne.StageID, Is.Not.Null.And.Not.Empty);

                // Create a line with a work order stage.
                DateTimeOffset woLineCreateStartTime = new DateTimeOffset(timeSheetDate.Year, timeSheetDate.Month, timeSheetDate.Day, 11, 0, 0, TimeZoneInfo.Local.BaseUtcOffset);

                StaffTimesheetLinePOSTRequest woLineCreateReq = new StaffTimesheetLinePOSTRequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    Description = "Timesheet WorkOrder line " + RandomString(6),
                    StartTime = woLineCreateStartTime,
                    EndTime = woLineCreateStartTime.AddHours(2),
                    WorkOrderStage = new StaffTimesheetWorkOrderStageDto()
                    {
                        WorkOrderStageID = workOrderStageOne.StageID,
                        ItemNo = workOrderStageOne.ItemNo,
                        Name = workOrderStageOne.Name,
                        WorkOrder = new JiwaFinancials.Jiwa.JiwaServiceModel.Staff.Timesheets.TimesheetWorkOrder()
                        {
                            WorkOrderID = workOrderCreateRes.WorkOrderID,
                            WorkOrderNo = workOrderCreateRes.WorkOrderNo
                        }
                    }
                };

                StaffTimesheetLineDto woLineCreateRes = await Client.PostAsync(woLineCreateReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                Assert.That(woLineCreateRes.TimesheetLineID, Is.Not.Null.And.Not.Empty);
                Assert.That(woLineCreateRes.Description, Is.EqualTo(woLineCreateReq.Description));
                Assert.That(woLineCreateRes.WorkOrderStage, Is.Not.Null);
                Assert.That(woLineCreateRes.WorkOrderStage.WorkOrderStageID, Is.EqualTo(workOrderStageOne.StageID));

                // Read all lines and verify the work order line is included.
                linesGetManyRes = await Client.GetAsync(linesGetManyReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(linesGetManyRes.Any(x => x.TimesheetLineID == woLineCreateRes.TimesheetLineID), Is.True);

                // Read the work order line.
                StaffTimesheetLineGETRequest woLineGetReq = new StaffTimesheetLineGETRequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    TimesheetLineID = woLineCreateRes.TimesheetLineID
                };

                StaffTimesheetLineDto woLineGetRes = await Client.GetAsync(woLineGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(woLineGetRes.TimesheetLineID, Is.EqualTo(woLineCreateRes.TimesheetLineID));
                Assert.That(woLineGetRes.Description, Is.EqualTo(woLineCreateReq.Description));
                Assert.That(woLineGetRes.WorkOrderStage, Is.Not.Null);
                Assert.That(woLineGetRes.WorkOrderStage.WorkOrderStageID, Is.EqualTo(workOrderStageOne.StageID));
                AssertApiTimeEquals(woLineGetRes.StartTime, woLineCreateReq.StartTime!.Value);
                AssertApiTimeEquals(woLineGetRes.EndTime, woLineCreateReq.EndTime!.Value);

                // Update the work order line with PATCH.
                DateTimeOffset woLinePatchStartTime = new DateTimeOffset(timeSheetDate.Year, timeSheetDate.Month, timeSheetDate.Day, 14, 0, 0, TimeZoneInfo.Local.BaseUtcOffset);

                StaffTimesheetLinePATCHRequest woLinePatchReq = new StaffTimesheetLinePATCHRequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    TimesheetLineID = woLineCreateRes.TimesheetLineID,
                    Description = "Updated WorkOrder line " + RandomString(6),
                    StartTime = woLinePatchStartTime,
                    EndTime = woLinePatchStartTime.AddHours(3),
                    WorkOrderStage = woLineCreateReq.WorkOrderStage
                };

                StaffTimesheetLineDto woLinePatchRes = await Client.PatchAsync(woLinePatchReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(woLinePatchRes.TimesheetLineID, Is.EqualTo(woLineCreateRes.TimesheetLineID));
                Assert.That(woLinePatchRes.Description, Is.EqualTo(woLinePatchReq.Description));

                // Verify the work order line was updated with PATCH.
                woLineGetRes = await Client.GetAsync(woLineGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(woLineGetRes.Description, Is.EqualTo(woLinePatchReq.Description));
                AssertApiTimeEquals(woLineGetRes.StartTime, woLinePatchReq.StartTime!.Value);
                AssertApiTimeEquals(woLineGetRes.EndTime, woLinePatchReq.EndTime!.Value);

                // Update the work order line with PUT.
                DateTimeOffset woLinePutStartTime = new DateTimeOffset(timeSheetDate.Year, timeSheetDate.Month, timeSheetDate.Day, 18, 0, 0, TimeZoneInfo.Local.BaseUtcOffset);

                StaffTimesheetLinePUTRequest woLinePutReq = new StaffTimesheetLinePUTRequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    TimesheetLineID = woLineCreateRes.TimesheetLineID,
                    Description = "Replaced WorkOrder line " + RandomString(6),
                    StartTime = woLinePutStartTime,
                    EndTime = woLinePutStartTime.AddHours(1),
                    WorkOrderStage = woLineCreateReq.WorkOrderStage
                };

                StaffTimesheetLineDto woLinePutRes = await Client.PutAsync(woLinePutReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(woLinePutRes.TimesheetLineID, Is.EqualTo(woLineCreateRes.TimesheetLineID));
                Assert.That(woLinePutRes.Description, Is.EqualTo(woLinePutReq.Description));

                // Verify the work order line was updated with PUT.
                woLineGetRes = await Client.GetAsync(woLineGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(woLineGetRes.Description, Is.EqualTo(woLinePutReq.Description));
                AssertApiTimeEquals(woLineGetRes.StartTime, woLinePutReq.StartTime!.Value);
                AssertApiTimeEquals(woLineGetRes.EndTime, woLinePutReq.EndTime!.Value);

                // Delete the work order line.
                StaffTimesheetLineDELETERequest woLineDeleteReq = new StaffTimesheetLineDELETERequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    TimesheetLineID = woLineCreateRes.TimesheetLineID
                };

                await Client.DeleteAsync(woLineDeleteReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

                // Verify the work order line was deleted.
                WebServiceException woLineDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
                {
                    _ = await Client.GetAsync(woLineGetReq);
                });
                Assert.That(woLineDeleteEx.StatusCode, Is.EqualTo(404));

                // Read all lines and ensure the deleted work order line is no longer returned.
                linesGetManyRes = await Client.GetAsync(linesGetManyReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(linesGetManyRes.Any(x => x.TimesheetLineID == woLineCreateRes.TimesheetLineID), Is.False);

                // Delete the work order created for this test.
                WorkOrderDELETERequest workOrderDeleteReq = new WorkOrderDELETERequest()
                {
                    WorkOrderID = createdWorkOrderID
                };

                await Client.DeleteAsync(workOrderDeleteReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(createdTimesheetID))
                {
                    // Delete the timesheet created for this test.
                    StaffTimesheetDELETERequest timesheetDeleteReq = new StaffTimesheetDELETERequest()
                    {
                        TimesheetID = createdTimesheetID
                    };

                    await Client.DeleteAsync(timesheetDeleteReq);
                    Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
                }

                if (!string.IsNullOrWhiteSpace(createdServiceManagerJobID))
                {
                    // Delete the service manager job created for this test.
                    await Client.DeleteAsync(new ServiceManagerJobDELETERequest() { JobID = createdServiceManagerJobID });
                    Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
                }
            }
        }
        #endregion

        #region "{WorkOrder}"
        [Test]
        public async Task StaffTimesheet_Lines_CRUD_WithWorkOrder()
        {
            string createdTimesheetID = null;
            string createdWorkOrderID = null;

            try
            {
                // Read the Admin staff member to use for timesheet creation.
                HR_StaffQuery adminStaffQueryReq = new HR_StaffQuery()
                {
                    Username = "Admin",
                    Take = 1
                };

                QueryResponse<HRStaffDto> adminStaffQueryRes = await Client.GetAsync(adminStaffQueryReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                HRStaffDto adminStaff = adminStaffQueryRes.Results.FirstOrDefault();
                Assert.That(adminStaff, Is.Not.Null);
                Assert.That(adminStaff.StaffID, Is.Not.Null.And.Not.Empty);

                // Create a work order for timesheet line operations.
                WorkOrderDto workOrderCreateRes = await CreateWorkOrderForTimesheetAsync();
                createdWorkOrderID = workOrderCreateRes.WorkOrderID;

                // Read all stages for the work order.
                WorkOrderStagesGETManyRequest workOrderStagesGetManyReq = new WorkOrderStagesGETManyRequest()
                {
                    WorkOrderID = workOrderCreateRes.WorkOrderID
                };

                List<WorkOrderStage> workOrderStagesGetManyRes = await Client.GetAsync(workOrderStagesGetManyReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(workOrderStagesGetManyRes.Count, Is.GreaterThan(0));

                WorkOrderStage workOrderStageOne = workOrderStagesGetManyRes.FirstOrDefault();
                Assert.That(workOrderStageOne, Is.Not.Null);
                Assert.That(workOrderStageOne.StageID, Is.Not.Null.And.Not.Empty);

                // Create a staff timesheet.
                StaffTimesheetPOSTRequest timesheetCreateReq = new StaffTimesheetPOSTRequest()
                {
                    StaffID = adminStaff.StaffID,
                    StaffUserName = adminStaff.Username,
                    TimeSheetDate = new DateTimeOffset(DateTime.UtcNow.Date),
                    Reference = "Timesheet WorkOrder Lines " + RandomString(6),
                    IsActivated = false
                };

                StaffTimesheetDto timesheetCreateRes = await Client.PostAsync(timesheetCreateReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                Assert.That(timesheetCreateRes.TimesheetID, Is.Not.Null.And.Not.Empty);
                createdTimesheetID = timesheetCreateRes.TimesheetID;

                // Create a line using the work order stage.
                DateTimeOffset lineCreateStartTime = new DateTimeOffset(DateTime.UtcNow.Date.AddHours(9));
                lineCreateStartTime = new DateTimeOffset(lineCreateStartTime.Year, lineCreateStartTime.Month, lineCreateStartTime.Day, lineCreateStartTime.Hour, lineCreateStartTime.Minute, 0, lineCreateStartTime.Offset);

                StaffTimesheetLinePOSTRequest lineCreateReq = new StaffTimesheetLinePOSTRequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    Description = "Timesheet WorkOrder line " + RandomString(6),
                    StartTime = lineCreateStartTime,
                    EndTime = lineCreateStartTime.AddHours(2),
                    WorkOrderStage = new StaffTimesheetWorkOrderStageDto()
                    {
                        WorkOrderStageID = workOrderStageOne.StageID,
                        ItemNo = workOrderStageOne.ItemNo,
                        Name = workOrderStageOne.Name,
                        WorkOrder = new JiwaFinancials.Jiwa.JiwaServiceModel.Staff.Timesheets.TimesheetWorkOrder()
                        {
                            WorkOrderID = workOrderCreateRes.WorkOrderID,
                            WorkOrderNo = workOrderCreateRes.WorkOrderNo
                        }
                    }
                };

                StaffTimesheetLineDto lineCreateRes = await Client.PostAsync(lineCreateReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                Assert.That(lineCreateRes.TimesheetLineID, Is.Not.Null.And.Not.Empty);
                Assert.That(lineCreateRes.Description, Is.EqualTo(lineCreateReq.Description));
                Assert.That(lineCreateRes.WorkOrderStage, Is.Not.Null);
                Assert.That(lineCreateRes.WorkOrderStage.WorkOrderStageID, Is.EqualTo(workOrderStageOne.StageID));

                // Read all lines for the staff timesheet.
                StaffTimesheetLinesGETManyRequest linesGetManyReq = new StaffTimesheetLinesGETManyRequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID
                };

                List<StaffTimesheetLineDto> linesGetManyRes = await Client.GetAsync(linesGetManyReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(linesGetManyRes.Any(x => x.TimesheetLineID == lineCreateRes.TimesheetLineID), Is.True);

                // Read the appended line.
                StaffTimesheetLineGETRequest lineGetReq = new StaffTimesheetLineGETRequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    TimesheetLineID = lineCreateRes.TimesheetLineID
                };

                StaffTimesheetLineDto lineGetRes = await Client.GetAsync(lineGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(lineGetRes.TimesheetLineID, Is.EqualTo(lineCreateRes.TimesheetLineID));
                Assert.That(lineGetRes.Description, Is.EqualTo(lineCreateReq.Description));
                Assert.That(lineGetRes.WorkOrderStage, Is.Not.Null);
                Assert.That(lineGetRes.WorkOrderStage.WorkOrderStageID, Is.EqualTo(workOrderStageOne.StageID));

                // Update the line with PATCH.
                DateTimeOffset linePatchStartTime = new DateTimeOffset(DateTime.UtcNow.Date.AddHours(10));
                linePatchStartTime = new DateTimeOffset(linePatchStartTime.Year, linePatchStartTime.Month, linePatchStartTime.Day, linePatchStartTime.Hour, linePatchStartTime.Minute, 0, linePatchStartTime.Offset);

                StaffTimesheetLinePATCHRequest linePatchReq = new StaffTimesheetLinePATCHRequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    TimesheetLineID = lineCreateRes.TimesheetLineID,
                    Description = "Updated WorkOrder line " + RandomString(6),
                    StartTime = linePatchStartTime,
                    EndTime = linePatchStartTime.AddHours(3),
                    WorkOrderStage = lineCreateReq.WorkOrderStage
                };

                StaffTimesheetLineDto linePatchRes = await Client.PatchAsync(linePatchReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(linePatchRes.TimesheetLineID, Is.EqualTo(lineCreateRes.TimesheetLineID));
                Assert.That(linePatchRes.Description, Is.EqualTo(linePatchReq.Description));

                // Verify the line was updated with PATCH.
                lineGetRes = await Client.GetAsync(lineGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(lineGetRes.Description, Is.EqualTo(linePatchReq.Description));

                // Update the line with PUT.
                DateTimeOffset linePutStartTime = new DateTimeOffset(DateTime.UtcNow.Date.AddHours(11));
                linePutStartTime = new DateTimeOffset(linePutStartTime.Year, linePutStartTime.Month, linePutStartTime.Day, linePutStartTime.Hour, linePutStartTime.Minute, 0, linePutStartTime.Offset);

                StaffTimesheetLinePUTRequest linePutReq = new StaffTimesheetLinePUTRequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    TimesheetLineID = lineCreateRes.TimesheetLineID,
                    Description = "Replaced WorkOrder line " + RandomString(6),
                    StartTime = linePutStartTime,
                    EndTime = linePutStartTime.AddHours(1),
                    WorkOrderStage = lineCreateReq.WorkOrderStage
                };

                StaffTimesheetLineDto linePutRes = await Client.PutAsync(linePutReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(linePutRes.TimesheetLineID, Is.EqualTo(lineCreateRes.TimesheetLineID));
                Assert.That(linePutRes.Description, Is.EqualTo(linePutReq.Description));

                // Verify the line was updated with PUT.
                lineGetRes = await Client.GetAsync(lineGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(lineGetRes.Description, Is.EqualTo(linePutReq.Description));

                // Delete the line.
                StaffTimesheetLineDELETERequest lineDeleteReq = new StaffTimesheetLineDELETERequest()
                {
                    TimesheetID = timesheetCreateRes.TimesheetID,
                    TimesheetLineID = lineCreateRes.TimesheetLineID
                };

                await Client.DeleteAsync(lineDeleteReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

                // Verify the line was deleted.
                WebServiceException lineDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
                {
                    _ = await Client.GetAsync(lineGetReq);
                });
                Assert.That(lineDeleteEx.StatusCode, Is.EqualTo(404));

                // Read all lines and ensure the deleted line is no longer returned.
                linesGetManyRes = await Client.GetAsync(linesGetManyReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(linesGetManyRes.Any(x => x.TimesheetLineID == lineCreateRes.TimesheetLineID), Is.False);
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(createdTimesheetID))
                {
                    // Delete the timesheet created for this test.
                    StaffTimesheetDELETERequest timesheetDeleteReq = new StaffTimesheetDELETERequest()
                    {
                        TimesheetID = createdTimesheetID
                    };

                    await Client.DeleteAsync(timesheetDeleteReq);
                    Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
                }

                if (!string.IsNullOrWhiteSpace(createdWorkOrderID))
                {
                    // Delete the work order created for this test.
                    WorkOrderDELETERequest workOrderDeleteReq = new WorkOrderDELETERequest()
                    {
                        WorkOrderID = createdWorkOrderID
                    };

                    await Client.DeleteAsync(workOrderDeleteReq);
                    Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
                }
            }
        }
        #endregion
    }
}
