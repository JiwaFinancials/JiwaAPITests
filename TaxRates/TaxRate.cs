using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tax;
using NUnit.Framework;
using ServiceStack;
using System.Threading.Tasks;
using TaxRateDto = JiwaFinancials.Jiwa.JiwaServiceModel.Tax.TaxRate;

namespace JiwaAPITests.TaxRates
{
    public class TaxRate : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task TaxRates_CRUD()
        {
            // Create a tax rate.
            TaxRatePOSTRequest taxRateCreateReq = new TaxRatePOSTRequest()
            {
                Description = "Tax Rate " + RandomString(6),
                GSTTaxGroup = TaxRateTypes.GSTOut,
                Rate = 0.10M,
                IsEnabled = true
            };

            TaxRateDto taxRateCreateRes = await Client.PostAsync(taxRateCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(taxRateCreateRes.TaxID, Is.Not.Null.And.Not.Empty);
            Assert.That(taxRateCreateRes.Description, Is.EqualTo(taxRateCreateReq.Description));

            // Read the created tax rate.
            TaxRateGETRequest taxRateGetReq = new TaxRateGETRequest()
            {
                TaxID = taxRateCreateRes.TaxID
            };

            TaxRateDto taxRateGetRes = await Client.GetAsync(taxRateGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(taxRateGetRes.TaxID, Is.EqualTo(taxRateCreateRes.TaxID));
            Assert.That(taxRateGetRes.Description, Is.EqualTo(taxRateCreateReq.Description));

            // Update the created tax rate.
            TaxRatePATCHRequest taxRatePatchReq = new TaxRatePATCHRequest()
            {
                TaxID = taxRateCreateRes.TaxID,
                Description = "Updated Tax Rate " + RandomString(6),
                GSTTaxGroup = taxRateGetRes.GSTTaxGroup,
                Rate = taxRateGetRes.Rate,
                IsEnabled = taxRateGetRes.IsEnabled
            };

            TaxRateDto taxRatePatchRes = await Client.PatchAsync(taxRatePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(taxRatePatchRes.TaxID, Is.EqualTo(taxRateCreateRes.TaxID));
            Assert.That(taxRatePatchRes.Description, Is.EqualTo(taxRatePatchReq.Description));

            // Verify the tax rate was updated.
            taxRateGetRes = await Client.GetAsync(taxRateGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(taxRateGetRes.Description, Is.EqualTo(taxRatePatchReq.Description));

            // Delete the tax rate.
            await Client.DeleteAsync(new TaxRateDELETERequest() { TaxID = taxRateCreateRes.TaxID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the tax rate was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                TaxRateDto deletedTaxRate = await Client.GetAsync(taxRateGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}
