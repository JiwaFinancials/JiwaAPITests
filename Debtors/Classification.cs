using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors.Classification;

namespace JiwaAPITests.Debtors
{
    public class Classification : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task DebtorClassification_CRUD()
        {
            // Create a debtor classification
            DebtorClassificationPOSTRequest classificationCreateReq = new DebtorClassificationPOSTRequest()
            {
                Description = RandomString(5)
            };

            DebtorClassification classificationCreateRes = await Client.PostAsync(classificationCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(classificationCreateRes.Description, Is.EqualTo(classificationCreateReq.Description));
            Assert.That(classificationCreateRes.ClassificationID, !Is.Null);

            // Read the created item using the ClassificationID
            DebtorClassificationGETRequest classificationGetReq = new DebtorClassificationGETRequest() { ClassificationID = classificationCreateRes.ClassificationID };
            DebtorClassification classificationGetRes = await Client.GetAsync(classificationGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(classificationGetRes.Description, Is.EqualTo(classificationCreateReq.Description));

            // Update the classification
            DebtorClassificationPATCHRequest classificationPatchReq = new DebtorClassificationPATCHRequest()
            {
                ClassificationID = classificationCreateRes.ClassificationID,
                Description = RandomString(5)
            };
            DebtorClassification classificationPatchRes = await Client.PatchAsync(classificationPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(classificationPatchRes.Description, Is.EqualTo(classificationPatchReq.Description));

            // Remove the created classification
            DebtorClassificationDELETERequest classificationDELETEReq = new DebtorClassificationDELETERequest() { ClassificationID = classificationCreateRes.ClassificationID };
            await Client.DeleteAsync(classificationDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // ensure the deleted item is not there anymore            
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                DebtorClassification getDeletedRes = await Client.GetAsync(classificationGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));            
        }
        #endregion
    }
}
