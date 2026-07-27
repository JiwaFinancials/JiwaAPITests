using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.UserSettings
{
    public class UserSetting : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task UserSettings_POST_GET()
        {
            string section = "API Tests";
            string idKey = "UserSetting-" + RandomString(8);
            string createdContents = "Created " + RandomString(6);
            string updatedContents = "Updated " + RandomString(6);

            // Create a user setting for the current user.
            await Client.PostAsync(new UserSettingPOSTRequest()
            {
                Section = section,
                IDKey = idKey,
                Contents = createdContents
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Read the created user setting by section and ID key.
            QueryResponse<SY_UserProfile> userSettingGetRes = await Client.GetAsync(new UserSettingsGETRequest()
            {
                Section = section,
                IDKey = idKey
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(userSettingGetRes.Results, Is.Not.Null);
            Assert.That(userSettingGetRes.Results.Any(x => x.Section == section && x.IDKey == idKey && x.Contents == createdContents), Is.True);

            // Update the existing user setting for the current user.
            await Client.PostAsync(new UserSettingPOSTRequest()
            {
                Section = section,
                IDKey = idKey,
                Contents = updatedContents
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Verify the user setting was updated.
            userSettingGetRes = await Client.GetAsync(new UserSettingsGETRequest()
            {
                Section = section,
                IDKey = idKey
            });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(userSettingGetRes.Results.Any(x => x.Section == section && x.IDKey == idKey && x.Contents == updatedContents), Is.True);

            // Read all user settings and verify the updated setting is present.
            const int pageSize = 100;
            int skip = 0;
            List<SY_UserProfile> allUserSettings = new List<SY_UserProfile>();

            while (true)
            {
                QueryResponse<SY_UserProfile> userSettingsGetManyRes = await Client.GetAsync(new UserSettingsGETManyRequest()
                {
                    Skip = skip,
                    Take = pageSize
                });
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(userSettingsGetManyRes.Results, Is.Not.Null);

                allUserSettings.AddRange(userSettingsGetManyRes.Results);

                if (userSettingsGetManyRes.Results.Count < pageSize)
                {
                    break;
                }

                skip += pageSize;
            }

            Assert.That(allUserSettings.Any(x => x.Section == section && x.IDKey == idKey && x.Contents == updatedContents), Is.True);
        }
        #endregion
    }
}
