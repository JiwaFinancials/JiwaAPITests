using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Languages;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.Languages
{
    public class Language : JiwaAPITest
    {
        #region "Core"
        [Test]
        public async Task Languages_CRUD()
        {
            // Create a language.
            LanguagePOSTRequest languageCreateReq = new LanguagePOSTRequest()
            {
                Description = "Language " + RandomString(6),
                LanguageCode = RandomString(2),
                IsDefault = false
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Languages.Language languageCreateRes = await Client.PostAsync(languageCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(languageCreateRes.LanguageID, Is.Not.Null);
            Assert.That(languageCreateRes.Description, Is.EqualTo(languageCreateReq.Description));

            // Read all languages and confirm the created language is present.
            LanguagesGETManyRequest languagesGetManyReq = new LanguagesGETManyRequest();
            List<JiwaFinancials.Jiwa.JiwaServiceModel.Languages.Language> languagesGetManyRes = await Client.GetAsync(languagesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(languagesGetManyRes.Any(x => x.LanguageID == languageCreateRes.LanguageID), Is.True);

            // Read the created language.
            LanguageGETRequest languageGetReq = new LanguageGETRequest()
            {
                LanguageID = languageCreateRes.LanguageID
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Languages.Language languageGetRes = await Client.GetAsync(languageGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(languageGetRes.LanguageID, Is.EqualTo(languageCreateRes.LanguageID));
            Assert.That(languageGetRes.Description, Is.EqualTo(languageCreateReq.Description));

            // Update the created language.
            LanguagePATCHRequest languagePatchReq = new LanguagePATCHRequest()
            {
                LanguageID = languageCreateRes.LanguageID,
                Description = "Updated Language " + RandomString(6),
                LanguageCode = RandomString(2),
                IsDefault = false
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Languages.Language languagePatchRes = await Client.PatchAsync(languagePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(languagePatchRes.LanguageID, Is.EqualTo(languagePatchReq.LanguageID));
            Assert.That(languagePatchRes.LanguageID, Is.EqualTo(languageCreateRes.LanguageID));
            Assert.That(languagePatchRes.Description, Is.EqualTo(languagePatchReq.Description));

            // Verify the language was updated.
            languageGetRes = await Client.GetAsync(languageGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(languageGetRes.Description, Is.EqualTo(languagePatchReq.Description));
            Assert.That(languageGetRes.LanguageCode, Is.EqualTo(languagePatchReq.LanguageCode));

            // Delete the language.
            LanguageDELETERequest languageDeleteReq = new LanguageDELETERequest()
            {
                LanguageID = languageCreateRes.LanguageID
            };

            await Client.DeleteAsync(languageDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the language was deleted.
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                JiwaFinancials.Jiwa.JiwaServiceModel.Languages.Language deletedLanguageGetRes = await Client.GetAsync(languageGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}



