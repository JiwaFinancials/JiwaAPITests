using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Currencies;
using ServiceStack;
using System.Threading.Tasks;

namespace JiwaAPITests.Currencies
{
    public class Currency : JiwaAPITest
    {
        #region "Core"
        [Test]
        public async Task Currencies_CRUD()
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
            Assert.That(currencyCreateRes.Name, Is.EqualTo(currencyCreateReq.Name));

            CurrencyGETRequest currencyGetReq = new CurrencyGETRequest()
            {
                CurrencyID = currencyCreateRes.CurrencyID
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Currencies.Currency currencyGetRes = await Client.GetAsync(currencyGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(currencyGetRes.CurrencyID, Is.EqualTo(currencyCreateRes.CurrencyID));
            Assert.That(currencyGetRes.Name, Is.EqualTo(currencyCreateReq.Name));

            CurrencyPATCHRequest currencyPatchReq = new CurrencyPATCHRequest()
            {
                CurrencyID = currencyCreateRes.CurrencyID,
                Name = "Updated Currency " + RandomString(5),
                ShortName = RandomString(3),
                Symbol = "U$",
                DecimalPlaces = 3,
                IsEnabled = false,
                IsLocal = false
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Currencies.Currency currencyPatchRes = await Client.PatchAsync(currencyPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(currencyPatchRes.CurrencyID, Is.EqualTo(currencyPatchReq.CurrencyID));
            Assert.That(currencyPatchRes.CurrencyID, Is.EqualTo(currencyCreateRes.CurrencyID));
            Assert.That(currencyPatchRes.Name, Is.EqualTo(currencyPatchReq.Name));

            currencyGetRes = await Client.GetAsync(currencyGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(currencyGetRes.Name, Is.EqualTo(currencyPatchReq.Name));
            Assert.That(currencyGetRes.ShortName, Is.EqualTo(currencyPatchReq.ShortName));

            CurrencyDELETERequest currencyDeleteReq = new CurrencyDELETERequest()
            {
                CurrencyID = currencyCreateRes.CurrencyID
            };

            await Client.DeleteAsync(currencyDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.Currencies.Currency deletedCurrencyGetRes = await Client.GetAsync(currencyGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}


