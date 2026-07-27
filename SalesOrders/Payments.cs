using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesOrderDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder;
using SalesOrderPaymentDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderPayment;
using SalesOrderPaymentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.PaymentType;

namespace JiwaAPITests.SalesOrders
{
    public class Payments : JiwaAPITest
    {
        private async Task<SalesOrderDto> CreateSalesOrderWithLineAsync()
        {
            // Create an inventory item
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Payments Test Item",
                DefaultPrice = 11.00M
            };
            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a debtor
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Payments Test Debtor"
            };
            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Create a sales order with one line
            SalesOrderPOSTRequest soCreateReq = new SalesOrderPOSTRequest()
            {
                DebtorAccountNo = debtorCreateReq.AccountNo,
                InvoiceInitDate = DateTime.Today,
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        QuantityOrdered = 1
                    }
                }
            };
            SalesOrderDto soCreateRes = await Client.PostAsync(soCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(soCreateRes.InvoiceID, Is.Not.Null);

            return soCreateRes;
        }

        #region "{Payments}"
        [Test]
        public async Task SalesOrder_Payments_CRUD()
        {
            // Read payment types and use one for the payment creation request
            SalesOrderPaymentTypesGETManyRequest paymentTypesGetManyReq = new SalesOrderPaymentTypesGETManyRequest();
            List<SalesOrderPaymentTypeDto> paymentTypesGetManyRes = await Client.GetAsync(paymentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(paymentTypesGetManyRes.Count, Is.GreaterThan(0));

            // Create a sales order to append a payment to
            SalesOrderDto salesOrderCreateRes = await CreateSalesOrderWithLineAsync();
            string invoiceID = salesOrderCreateRes.InvoiceID;
            string invoiceHistoryID = salesOrderCreateRes.Histories[0].InvoiceHistoryID;

            // Append a payment to the sales order history
            SalesOrderPaymentsPOSTRequest paymentCreateReq = new SalesOrderPaymentsPOSTRequest()
            {
                InvoiceID = invoiceID,
                InvoiceHistoryID = invoiceHistoryID,
                PaymentType = paymentTypesGetManyRes[0],
                AmountPaid = 10.00M,
                PaymentDate = DateTime.Today
            };
            SalesOrderPaymentDto paymentCreateRes = await Client.PostAsync(paymentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(paymentCreateRes.PaymentID, Is.Not.Null);
            Assert.That(paymentCreateRes.AmountPaid, Is.EqualTo(paymentCreateReq.AmountPaid));

            // Read all sales order payments and ensure the created payment is returned
            SalesOrderPaymentsGETManyRequest paymentsGetManyReq = new SalesOrderPaymentsGETManyRequest()
            {
                InvoiceID = invoiceID
            };
            List<SalesOrderPaymentDto> paymentsGetManyRes = await Client.GetAsync(paymentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(paymentsGetManyRes.Any(x => x.PaymentID == paymentCreateRes.PaymentID), Is.True);

            // Read the created sales order payment
            SalesOrderPaymentsGETRequest paymentGetReq = new SalesOrderPaymentsGETRequest()
            {
                InvoiceID = invoiceID,
                PaymentID = paymentCreateRes.PaymentID
            };
            SalesOrderPaymentDto paymentGetRes = await Client.GetAsync(paymentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(paymentGetRes.PaymentID, Is.EqualTo(paymentCreateRes.PaymentID));
            Assert.That(paymentGetRes.AmountPaid, Is.EqualTo(paymentCreateReq.AmountPaid));

            // Update the sales order payment
            SalesOrderPaymentsPATCHRequest paymentPatchReq = new SalesOrderPaymentsPATCHRequest()
            {
                InvoiceID = invoiceID,
                PaymentID = paymentCreateRes.PaymentID,
                AmountPaid = 15.00M,
                PaymentDate = DateTime.Today.AddDays(1),
                PaymentRef = "Updated Payment " + RandomString(6)
            };
            SalesOrderPaymentDto paymentPatchRes = await Client.PatchAsync(paymentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(paymentPatchRes.PaymentID, Is.EqualTo(paymentCreateRes.PaymentID));
            Assert.That(paymentPatchRes.AmountPaid, Is.EqualTo(paymentPatchReq.AmountPaid));

            // Read the updated sales order payment
            paymentGetRes = await Client.GetAsync(paymentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(paymentGetRes.AmountPaid, Is.EqualTo(paymentPatchReq.AmountPaid));
            Assert.That(paymentGetRes.PaymentRef, Is.EqualTo(paymentPatchReq.PaymentRef));

            // Delete the sales order payment
            SalesOrderPaymentsDELETERequest paymentDeleteReq = new SalesOrderPaymentsDELETERequest()
            {
                InvoiceID = invoiceID,
                PaymentID = paymentCreateRes.PaymentID
            };
            await Client.DeleteAsync(paymentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the sales order payment was deleted
            WebServiceException paymentDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(paymentGetReq);
            });
            Assert.That(paymentDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all payments and ensure the deleted payment is no longer returned
            paymentsGetManyRes = await Client.GetAsync(paymentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(paymentsGetManyRes.Any(x => x.PaymentID == paymentCreateRes.PaymentID), Is.False);
        }
        #endregion
    }
}

