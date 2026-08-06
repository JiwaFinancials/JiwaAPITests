using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.PrepaidLabour;
using NUnit.Framework;
using ServiceStack;
using System.Threading.Tasks;

namespace JiwaAPITests.PrepaidLabour
{
    public class PrepaidLabourPack : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task PrepaidLabourPack_CRUD()
        {
            // Create a debtor for the prepaid labour pack.
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = $"Prepaid Labour Debtor {RandomString(5)}"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, Is.Not.Null);

            // Create a prepaid labour pack.
            PrepaidLabourPackPOSTRequest prepaidLabourPackCreateReq = new PrepaidLabourPackPOSTRequest()
            {
                PackNo = $"PL-{RandomString(6)}",
                Name = $"Prepaid Labour Pack {RandomString(5)}",
                Description = "Prepaid labour pack CRUD test",
                DebtorID = debtorCreateRes.DebtorID,
                AccountNo = debtorCreateRes.AccountNo,
                TotalHours = 20M,
                ReorderLevel = 5M,
                Rate = 120M,
                Ratio = 1M,
                SpecialUse = false
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.PrepaidLabour.PrepaidLabourPack prepaidLabourPackCreateRes = await Client.PostAsync(prepaidLabourPackCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(prepaidLabourPackCreateRes.PackID, Is.Not.Null);
            Assert.That(prepaidLabourPackCreateRes.Name, Is.EqualTo(prepaidLabourPackCreateReq.Name));

            // Read the created prepaid labour pack.
            PrepaidLabourPackGETRequest prepaidLabourPackGetReq = new PrepaidLabourPackGETRequest()
            {
                PackID = prepaidLabourPackCreateRes.PackID
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.PrepaidLabour.PrepaidLabourPack prepaidLabourPackGetRes = await Client.GetAsync(prepaidLabourPackGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(prepaidLabourPackGetRes.PackID, Is.EqualTo(prepaidLabourPackCreateRes.PackID));
            Assert.That(prepaidLabourPackGetRes.Name, Is.EqualTo(prepaidLabourPackCreateReq.Name));

            // Update the created prepaid labour pack.
            PrepaidLabourPackPATCHRequest prepaidLabourPackPatchReq = new PrepaidLabourPackPATCHRequest()
            {
                PackID = prepaidLabourPackCreateRes.PackID,
                DebtorID = debtorCreateRes.DebtorID,
                AccountNo = debtorCreateRes.AccountNo,
                Name = $"Updated Prepaid Labour Pack {RandomString(5)}",
                Description = "Prepaid labour pack CRUD updated",
                TotalHours = 30M,
                ReorderLevel = 10M,
                Rate = 135M,
                Ratio = 0.75M,
                SpecialUse = true
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.PrepaidLabour.PrepaidLabourPack prepaidLabourPackPatchRes = await Client.PatchAsync(prepaidLabourPackPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(prepaidLabourPackPatchRes.PackID, Is.EqualTo(prepaidLabourPackPatchReq.PackID));
            Assert.That(prepaidLabourPackPatchRes.PackID, Is.EqualTo(prepaidLabourPackCreateRes.PackID));
            Assert.That(prepaidLabourPackPatchRes.Name, Is.EqualTo(prepaidLabourPackPatchReq.Name));

            // Verify the prepaid labour pack was updated.
            prepaidLabourPackGetRes = await Client.GetAsync(prepaidLabourPackGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(prepaidLabourPackGetRes.Name, Is.EqualTo(prepaidLabourPackPatchReq.Name));
            Assert.That(prepaidLabourPackGetRes.Description, Is.EqualTo(prepaidLabourPackPatchReq.Description));
            Assert.That(prepaidLabourPackGetRes.SpecialUse, Is.EqualTo(prepaidLabourPackPatchReq.SpecialUse));

            // Delete the prepaid labour pack.
            PrepaidLabourPackDELETERequest prepaidLabourPackDeleteReq = new PrepaidLabourPackDELETERequest()
            {
                PackID = prepaidLabourPackCreateRes.PackID
            };

            await Client.DeleteAsync(prepaidLabourPackDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the prepaid labour pack was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.PrepaidLabour.PrepaidLabourPack deletedPrepaidLabourPackGetRes = await Client.GetAsync(prepaidLabourPackGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}


