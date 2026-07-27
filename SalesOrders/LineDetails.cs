using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers;
using JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder;

namespace JiwaAPITests.SalesOrders
{
    public class LineDetails : JiwaAPITest
    {
        private async Task<(SalesOrderDto salesOrder, string serialNo)> CreatePickableSalesOrderAsync()
        {
            // Create a serialised inventory item
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Line Details Test Item",
                DefaultPrice = 25.00M,
                UseSerialNo = true
            };
            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a debtor
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Line Details Test Debtor"
            };
            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Seed stock for the serialised item
            string serialNo = RandomString(8);
            StockTransferPOSTRequest transferCreateReq = new StockTransferPOSTRequest()
            {
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers.StockTransferLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.StockTransfers.StockTransferLine()
                    {
                        FromInventoryPartNo = "External",
                        ToInventoryPartNo = itemCreateReq.PartNo,
                        TransferQuantity = 1,
                        ToPartSerialNo = serialNo
                    }
                }
            };
            StockTransfer transferCreateRes = await Client.PostAsync(transferCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Activate the transfer so the serial number can be picked
            StockTransferACTIVATERequest transferActivateReq = new StockTransferACTIVATERequest()
            {
                TransferID = transferCreateRes.TransferID
            };
            await Client.PostAsync(transferActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a sales order with a serialised line
            SalesOrderPOSTRequest soCreateReq = new SalesOrderPOSTRequest()
            {
                DebtorAccountNo = debtorCreateReq.AccountNo,
                InvoiceInitDate = DateTime.Today,
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        QuantityOrdered = 1,
                        LineDetails = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail>()
                        {
                            new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLineDetail()
                            {
                                SerialNo = serialNo,
                                Quantity = 1
                            }
                        }
                    }
                }
            };
            SalesOrderDto soCreateRes = await Client.PostAsync(soCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(soCreateRes.InvoiceID, Is.Not.Null);

            return (soCreateRes, serialNo);
        }

        #region "{LineDetails}"
        [Test]
        public async Task SalesOrder_LineDetails_PUT()
        {
            // Create a sales order line we can set picking information for
            var (salesOrder, serialNo) = await CreatePickableSalesOrderAsync();
            string invoiceID = salesOrder.InvoiceID;
            string invoiceLineID = salesOrder.Lines[0].InvoiceLineID;

            // Set the picking information for the sales order line
            SalesOrderPickRequest pickReq = new SalesOrderPickRequest()
            {
                InvoiceID = invoiceID,
                InvoiceLineID = invoiceLineID,
                PickData = new List<PickData>()
                {
                    new PickData()
                    {
                        SerialNo = serialNo,
                        Quantity = 1
                    }
                }
            };

            await Client.PutAsync(pickReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Read the updated sales order and confirm the line remains readable
            SalesOrderGETRequest salesOrderGetReq = new SalesOrderGETRequest()
            {
                InvoiceID = invoiceID
            };
            SalesOrderDto salesOrderGetRes = await Client.GetAsync(salesOrderGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(salesOrderGetRes.Lines.Single(x => x.InvoiceLineID == invoiceLineID).LineDetails.Any(x => x.SerialNo == serialNo), Is.True);
        }
        #endregion
    }
}

