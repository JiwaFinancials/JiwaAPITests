using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Bills;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes;
using JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager.Configuration;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.ServiceManager
{
    public class ServiceManagerTestBase : JiwaAPITest
    {
        protected async Task<Debtor> CreateDebtorAsync()
        {
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Service Manager Debtor " + RandomString(5)
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, Is.Not.Null);

            return debtorCreateRes;
        }

        protected async Task<Job> CreateJobAsync()
        {
            Debtor debtor = await CreateDebtorAsync();
            IN_Logical currentWarehouse = await Client.GetAsync(new LogicalWarehousesCurrentGETRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(currentWarehouse, Is.Not.Null);
            Assert.That(currentWarehouse.IN_LogicalID, Is.Not.Null.And.Not.Empty);

            ServiceManagerJobPOSTRequest jobCreateReq = new ServiceManagerJobPOSTRequest()
            {
                DebtorID = debtor.DebtorID,
                AccountNo = debtor.AccountNo,
                DebtorName = debtor.Name,
                ContactName = "Contact " + RandomString(5),
                DateLogged = DateTime.Today,
                WarehouseID = currentWarehouse.IN_LogicalID
            };

            Job jobCreateRes = await Client.PostAsync(jobCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(jobCreateRes.JobID, Is.Not.Null);

            return jobCreateRes;
        }

        protected async Task<ServiceManagerTask> CreateTaskAsync(string jobID)
        {
            ServiceManagerPrioritiesGETManyRequest prioritiesGetManyReq = new ServiceManagerPrioritiesGETManyRequest();
            List<ServiceManagerPriority> prioritiesGetManyRes = await Client.GetAsync(prioritiesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(prioritiesGetManyRes.Count, Is.GreaterThan(0));

            ServiceManagerStatusesGETManyRequest statusesGetManyReq = new ServiceManagerStatusesGETManyRequest();
            List<ServiceManagerStatus> statusesGetManyRes = await Client.GetAsync(statusesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(statusesGetManyRes.Count, Is.GreaterThan(0));

            ServiceManagerTasksPOSTRequest taskCreateReq = new ServiceManagerTasksPOSTRequest()
            {
                JobID = jobID,
                Description = "Service manager task " + RandomString(8),
                Priority = new ServiceManagerPriority()
                {
                    PriorityID = prioritiesGetManyRes.First().PriorityID
                },
                Status = new ServiceManagerStatus()
                {
                    StatusID = statusesGetManyRes.First().StatusID
                }
            };

            ServiceManagerTask taskCreateRes = await Client.PostAsync(taskCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(taskCreateRes.TaskID, Is.Not.Null);

            return taskCreateRes;
        }

        protected async Task<ServiceManagerActivity> CreateActivityAsync()
        {
            ServiceManagerActivitiesPOSTRequest activityCreateReq = new ServiceManagerActivitiesPOSTRequest()
            {
                Name = "Activity " + RandomString(8),
                Description = "Service manager activity " + RandomString(8),
                IsEnabled = true,
                IsDefault = false
            };

            ServiceManagerActivity activityCreateRes = await Client.PostAsync(activityCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(activityCreateRes.ActivityID, Is.Not.Null);

            return activityCreateRes;
        }

        protected async Task<ServiceManagerActivity> GetAnyActivityAsync()
        {
            ServiceManagerActivitiesGETManyRequest activitiesGetManyReq = new ServiceManagerActivitiesGETManyRequest();
            List<ServiceManagerActivity> activitiesGetManyRes = await Client.GetAsync(activitiesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(activitiesGetManyRes.Count, Is.GreaterThan(0));

            return activitiesGetManyRes.First();
        }

        protected async Task EnsureStockOnHandAsync(string partNo, decimal quantity)
        {
            // Create a creditor for purchase order and goods received note operations.
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Service Manager Stock Seed Creditor"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);

            // Create a purchase order line for the requested inventory part.
            PurchaseOrderPOSTRequest purchaseOrderCreateReq = new PurchaseOrderPOSTRequest()
            {
                CreditorAccountNo = creditorCreateRes.AccountNo,
                Reference = "PO-" + RandomString(8),
                OrderDate = DateTime.Today,
                Lines = new List<PurchaseOrderLine>()
                {
                    new PurchaseOrderLine()
                    {
                        PartNo = partNo,
                        Quantity = quantity
                    }
                }
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.PurchaseOrders.PurchaseOrder purchaseOrderCreateRes = await Client.PostAsync(purchaseOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseOrderCreateRes.PurchaseOrderID, Is.Not.Null);

            // Activate the purchase order so stock can be received.
            PurchaseOrderACTIVATERequest purchaseOrderActivateReq = new PurchaseOrderACTIVATERequest()
            {
                PurchaseOrderID = purchaseOrderCreateRes.PurchaseOrderID
            };

            _ = await Client.PostAsync(purchaseOrderActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a goods received note from the purchase order line.
            GoodsReceivedNoteCREATEFromPOLinesRequest grnCreateReq = new GoodsReceivedNoteCREATEFromPOLinesRequest()
            {
                ReceivedDate = DateTime.Today,
                ReceivedPOLineQuantities = new List<ReceivedPOLineQuantity>()
                {
                    new ReceivedPOLineQuantity()
                    {
                        OrderLineID = purchaseOrderCreateRes.Lines[0].PurchaseOrderLineID,
                        Quantity = quantity
                    }
                }
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes.GoodsReceivedNote grnCreateRes = await Client.PostAsync(grnCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(grnCreateRes.GRNID, Is.Not.Null);

            // Activate the goods received note to commit stock on hand.
            GoodsReceivedNoteACTIVATERequest grnActivateReq = new GoodsReceivedNoteACTIVATERequest()
            {
                GRNID = grnCreateRes.GRNID
            };

            _ = await Client.PostAsync(grnActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
        }
    }
}

