using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Currencies;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.Currencies
{
    public class Rates : JiwaAPITest
    {
        private async Task<JiwaFinancials.Jiwa.JiwaServiceModel.Currencies.Currency> CreateCurrency()
        {
            CurrencyPOSTRequest currencyCreateReq = new CurrencyPOSTRequest()
            {
                Name = "Currency " + RandomString(6),
                ShortName = RandomString(3),
                Symbol = "$",
                DecimalPlaces = 2,
                IsEnabled = true,
                IsLocal = false
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Currencies.Currency currencyCreateRes = await Client.PostAsync(currencyCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(currencyCreateRes.CurrencyID, Is.Not.Null);

            return currencyCreateRes;
        }

        #region "Rates"
        [Test]
        public async Task Currencies_Rates_CRUD()
        {
            JiwaFinancials.Jiwa.JiwaServiceModel.Currencies.Currency currencyCreateRes = await CreateCurrency();

            CurrencyRatesGETManyRequest ratesGetManyReq = new CurrencyRatesGETManyRequest()
            {
                CurrencyID = currencyCreateRes.CurrencyID
            };

            List<CurrencyRate> ratesGetManyRes = await Client.GetAsync(ratesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            CurrencyRatePOSTRequest rateCreateReq = new CurrencyRatePOSTRequest()
            {
                CurrencyID = currencyCreateRes.CurrencyID,
                EffectiveDate = DateTime.Today,
                TransactionRate = 1.25M
            };

            CurrencyRate rateCreateRes = await Client.PostAsync(rateCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(rateCreateRes.RateID, Is.Not.Null);
            Assert.That(rateCreateRes.TransactionRate, Is.EqualTo(rateCreateReq.TransactionRate));

            CurrencyRateGETRequest rateGetReq = new CurrencyRateGETRequest()
            {
                CurrencyID = currencyCreateRes.CurrencyID,
                RateID = rateCreateRes.RateID
            };

            CurrencyRate rateGetRes = await Client.GetAsync(rateGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(rateGetRes.RateID, Is.EqualTo(rateCreateRes.RateID));
            Assert.That(rateGetRes.TransactionRate, Is.EqualTo(rateCreateReq.TransactionRate));

            ratesGetManyRes = await Client.GetAsync(ratesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(ratesGetManyRes.Any(x => x.RateID == rateCreateRes.RateID), Is.True);

            CurrencyRatePATCHRequest ratePatchReq = new CurrencyRatePATCHRequest()
            {
                CurrencyID = currencyCreateRes.CurrencyID,
                RateID = rateCreateRes.RateID,
                EffectiveDate = DateTime.Today.AddDays(1),
                TransactionRate = 1.35M
            };

            CurrencyRate ratePatchRes = await Client.PatchAsync(ratePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(ratePatchRes.RateID, Is.EqualTo(rateCreateRes.RateID));
            Assert.That(ratePatchRes.TransactionRate, Is.EqualTo(ratePatchReq.TransactionRate));

            rateGetRes = await Client.GetAsync(rateGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(rateGetRes.TransactionRate, Is.EqualTo(ratePatchReq.TransactionRate));

            CurrencyRateDELETERequest rateDeleteReq = new CurrencyRateDELETERequest()
            {
                CurrencyID = currencyCreateRes.CurrencyID,
                RateID = rateCreateRes.RateID
            };

            await Client.DeleteAsync(rateDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                CurrencyRate deletedRateGetRes = await Client.GetAsync(rateGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            ratesGetManyRes = await Client.GetAsync(ratesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(ratesGetManyRes.Any(x => x.RateID == rateCreateRes.RateID), Is.False);

            CurrencyDELETERequest currencyDeleteReq = new CurrencyDELETERequest()
            {
                CurrencyID = currencyCreateRes.CurrencyID
            };

            await Client.DeleteAsync(currencyDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

