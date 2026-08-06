using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors.Classification;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace JiwaAPITests.Creditors
{
    public class Classification : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task CreditorClassification_CRUD()
        {
            // Create a creditor classification
            CreditorClassificationPOSTRequest classificationCreateReq = new CreditorClassificationPOSTRequest()
            {
                Description = RandomString(5)
            };

            CreditorClassification classificationCreateRes = await Client.PostAsync(classificationCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(classificationCreateRes.Description, Is.EqualTo(classificationCreateReq.Description));
            Assert.That(classificationCreateRes.ClassificationID, !Is.Null);

            // Read the created item using the ClassificationID
            CreditorClassificationGETRequest classificationGetReq = new CreditorClassificationGETRequest() { ClassificationID = classificationCreateRes.ClassificationID };
            CreditorClassification classificationGetRes = await Client.GetAsync(classificationGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(classificationGetRes.Description, Is.EqualTo(classificationCreateReq.Description));

            // Update the classification
            CreditorClassificationPATCHRequest classificationPatchReq = new CreditorClassificationPATCHRequest()
            {
                ClassificationID = classificationCreateRes.ClassificationID,
                Description = RandomString(5)
            };
            CreditorClassification classificationPatchRes = await Client.PatchAsync(classificationPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(classificationPatchRes.ClassificationID, Is.EqualTo(classificationPatchReq.ClassificationID));
            Assert.That(classificationPatchRes.Description, Is.EqualTo(classificationPatchReq.Description));

            // Remove the created classification
            CreditorClassificationDELETERequest classificationDELETEReq = new CreditorClassificationDELETERequest() { ClassificationID = classificationCreateRes.ClassificationID };
            await Client.DeleteAsync(classificationDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // ensure the deleted item is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                CreditorClassification getDeletedRes = await Client.GetAsync(classificationGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}


