using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesOrderPaymentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.PaymentType;

namespace JiwaAPITests.SalesOrders
{
    public class PaymentTypes : JiwaAPITest
    {
        #region "{PaymentTypes}"
        [Test]
        public async Task SalesOrder_PaymentTypes_CRUD()
        {
            // Create a sales order payment type
            SalesOrderPaymentTypesPOSTRequest paymentTypeCreateReq = new SalesOrderPaymentTypesPOSTRequest()
            {
                Name = "Sales Order Payment Type " + RandomString(8),
                Code = RandomString(4),
                IsEnabled = true,
                IsDefault = false,
                IsCreditCard = false,
                IsPOS = false
            };
            SalesOrderPaymentTypeDto paymentTypeCreateRes = await Client.PostAsync(paymentTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(paymentTypeCreateRes.PaymentTypeID, Is.Not.Null);
            Assert.That(paymentTypeCreateRes.Name, Is.EqualTo(paymentTypeCreateReq.Name));

            // Read all sales order payment types
            SalesOrderPaymentTypesGETManyRequest paymentTypesGetManyReq = new SalesOrderPaymentTypesGETManyRequest();
            List<SalesOrderPaymentTypeDto> paymentTypesGetManyRes = await Client.GetAsync(paymentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(paymentTypesGetManyRes.Any(x => x.PaymentTypeID == paymentTypeCreateRes.PaymentTypeID), Is.True);

            // Read the created sales order payment type
            SalesOrderPaymentTypesGETRequest paymentTypeGetReq = new SalesOrderPaymentTypesGETRequest()
            {
                PaymentTypeID = paymentTypeCreateRes.PaymentTypeID
            };
            SalesOrderPaymentTypeDto paymentTypeGetRes = await Client.GetAsync(paymentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(paymentTypeGetRes.PaymentTypeID, Is.EqualTo(paymentTypeCreateRes.PaymentTypeID));
            Assert.That(paymentTypeGetRes.Name, Is.EqualTo(paymentTypeCreateReq.Name));

            // Update the sales order payment type
            SalesOrderPaymentTypesPATCHRequest paymentTypePatchReq = new SalesOrderPaymentTypesPATCHRequest()
            {
                PaymentTypeID = paymentTypeCreateRes.PaymentTypeID,
                Name = "Updated Sales Order Payment Type " + RandomString(6),
                Code = RandomString(4),
                IsEnabled = false,
                IsDefault = false,
                IsCreditCard = false,
                IsPOS = false
            };
            SalesOrderPaymentTypeDto paymentTypePatchRes = await Client.PatchAsync(paymentTypePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(paymentTypePatchRes.PaymentTypeID, Is.EqualTo(paymentTypeCreateRes.PaymentTypeID));
            Assert.That(paymentTypePatchRes.Name, Is.EqualTo(paymentTypePatchReq.Name));

            // Read the updated sales order payment type
            paymentTypeGetRes = await Client.GetAsync(paymentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(paymentTypeGetRes.Name, Is.EqualTo(paymentTypePatchReq.Name));
            Assert.That(paymentTypeGetRes.IsEnabled, Is.EqualTo(paymentTypePatchReq.IsEnabled));

            // Delete the sales order payment type
            SalesOrderPaymentTypesDELETERequest paymentTypeDeleteReq = new SalesOrderPaymentTypesDELETERequest()
            {
                PaymentTypeID = paymentTypeCreateRes.PaymentTypeID
            };
            await Client.DeleteAsync(paymentTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the sales order payment type was deleted
            WebServiceException paymentTypeDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(paymentTypeGetReq);
            });
            Assert.That(paymentTypeDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all payment types and ensure the deleted type is no longer returned
            paymentTypesGetManyRes = await Client.GetAsync(paymentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(paymentTypesGetManyRes.Any(x => x.PaymentTypeID == paymentTypeCreateRes.PaymentTypeID), Is.False);
        }
        #endregion

        #region "{Cache}"
        [Test]
        public async Task SalesOrder_PaymentTypes_CacheDelete()
        {
            // Attempt to clear the sales order payment type cache
            SalesOrderPaymentTypesCACHEDELETERequest cacheDeleteReq = new SalesOrderPaymentTypesCACHEDELETERequest();
            WebServiceException cacheEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                await Client.DeleteAsync(cacheDeleteReq);
            });
            Assert.That(cacheEx.ErrorMessage, Does.Contain("Invalid ClientKey"));
        }
        #endregion
    }
}

