using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.DebtorSystemTemplates;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;

namespace JiwaAPITests.Debtors
{
    public class DebtorSystemTemplates : JiwaAPITest
    {
        #region "DebtorSystemTemplates_Core"
        [Test]
        public async Task DebtorSystemTemplates_CRUD()
        {
            // Create a debtor system template
            DebtorSystemTemplatePOSTRequest createReq = new DebtorSystemTemplatePOSTRequest()
            {
                Name = $"Template-{RandomString(8)}",
                Code = RandomString(8),
                IsEnabled = true
            };

            DebtorSystemTemplate createRes = await Client.PostAsync(createReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(createRes.DebtorSystemTemplateID, Is.Not.Null);
            Assert.That(createRes.Name, Is.EqualTo(createReq.Name));

            // Read the created debtor system template
            DebtorSystemTemplateGETRequest getReq = new DebtorSystemTemplateGETRequest()
            {
                DebtorSystemTemplateID = createRes.DebtorSystemTemplateID
            };

            DebtorSystemTemplate getRes = await Client.GetAsync(getReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(getRes.DebtorSystemTemplateID, Is.EqualTo(createRes.DebtorSystemTemplateID));

            // Update the debtor system template
            DebtorSystemTemplatePATCHRequest patchReq = new DebtorSystemTemplatePATCHRequest()
            {
                DebtorSystemTemplateID = createRes.DebtorSystemTemplateID,
                Name = $"Updated-{RandomString(8)}",
                Code = RandomString(8),
                IsEnabled = false
            };

            DebtorSystemTemplate patchRes = await Client.PatchAsync(patchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(patchRes.DebtorSystemTemplateID, Is.EqualTo(patchReq.DebtorSystemTemplateID));
            Assert.That(patchRes.Name, Is.EqualTo(patchReq.Name));
            Assert.That(patchRes.IsEnabled, Is.EqualTo(patchReq.IsEnabled));

            // Delete the debtor system template
            DebtorSystemTemplateDELETERequest deleteReq = new DebtorSystemTemplateDELETERequest()
            {
                DebtorSystemTemplateID = createRes.DebtorSystemTemplateID
            };

            await Client.DeleteAsync(deleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the debtor system template was deleted
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(getReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "DebtorSystemTemplates_TemplateFields"
        [Test]
        public async Task DebtorSystemTemplates_TemplateFields_CRUD()
        {
            // Create a debtor system template
            DebtorSystemTemplatePOSTRequest templateCreateReq = new DebtorSystemTemplatePOSTRequest()
            {
                Name = $"Template-{RandomString(8)}",
                Code = RandomString(8),
                IsEnabled = true
            };

            DebtorSystemTemplate templateCreateRes = await Client.PostAsync(templateCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(templateCreateRes.DebtorSystemTemplateID, Is.Not.Null);

            try
            {
                // Create a debtor system template field
                DebtorSystemTemplateFieldPOSTRequest fieldCreateReq = new DebtorSystemTemplateFieldPOSTRequest()
                {
                    DebtorSystemTemplateID = templateCreateRes.DebtorSystemTemplateID,
                    Name = $"Field-{RandomString(6)}",
                    FieldType = DebtorSystemTemplateField.DebtorSystemTemplateFieldType.Text,
                    DefaultValue = "Default",
                    ItemNo = 1
                };

                DebtorSystemTemplateField fieldCreateRes = await Client.PostAsync(fieldCreateReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                Assert.That(fieldCreateRes.DebtorSystemTemplateFieldID, Is.Not.Null);
                Assert.That(fieldCreateRes.Name, Is.EqualTo(fieldCreateReq.Name));

                // Read the debtor system template fields list
                DebtorSystemTemplateFieldsGETManyRequest fieldsGetManyReq = new DebtorSystemTemplateFieldsGETManyRequest()
                {
                    DebtorSystemTemplateID = templateCreateRes.DebtorSystemTemplateID
                };

                List<DebtorSystemTemplateField> fieldsGetManyRes = await Client.GetAsync(fieldsGetManyReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(fieldsGetManyRes.Any(x => x.DebtorSystemTemplateFieldID == fieldCreateRes.DebtorSystemTemplateFieldID), Is.True);

                // Read the created field
                DebtorSystemTemplateFieldGETRequest fieldGetReq = new DebtorSystemTemplateFieldGETRequest()
                {
                    DebtorSystemTemplateID = templateCreateRes.DebtorSystemTemplateID,
                    DebtorSystemTemplateFieldID = fieldCreateRes.DebtorSystemTemplateFieldID
                };

                DebtorSystemTemplateField fieldGetRes = await Client.GetAsync(fieldGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(fieldGetRes.DebtorSystemTemplateFieldID, Is.EqualTo(fieldCreateRes.DebtorSystemTemplateFieldID));

                // Update the field
                DebtorSystemTemplateFieldPATCHRequest fieldPatchReq = new DebtorSystemTemplateFieldPATCHRequest()
                {
                    DebtorSystemTemplateID = templateCreateRes.DebtorSystemTemplateID,
                    DebtorSystemTemplateFieldID = fieldCreateRes.DebtorSystemTemplateFieldID,
                    Name = $"Updated-{RandomString(6)}",
                    FieldType = DebtorSystemTemplateField.DebtorSystemTemplateFieldType.Combo,
                    ComboText = "A\nB",
                    DefaultValue = "A",
                    ItemNo = 2
                };

                DebtorSystemTemplateField fieldPatchRes = await Client.PatchAsync(fieldPatchReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(fieldPatchRes.DebtorSystemTemplateFieldID, Is.EqualTo(fieldPatchReq.DebtorSystemTemplateFieldID));
                Assert.That(fieldPatchRes.Name, Is.EqualTo(fieldPatchReq.Name));

                // Delete the field
                DebtorSystemTemplateFieldDELETERequest fieldDeleteReq = new DebtorSystemTemplateFieldDELETERequest()
                {
                    DebtorSystemTemplateID = templateCreateRes.DebtorSystemTemplateID,
                    DebtorSystemTemplateFieldID = fieldCreateRes.DebtorSystemTemplateFieldID
                };

                await Client.DeleteAsync(fieldDeleteReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

                // Verify the field was deleted
                WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
                {
                    _ = await Client.GetAsync(fieldGetReq);
                });
                Assert.That(ex.StatusCode, Is.EqualTo(404));
            }
            finally
            {
                DebtorSystemTemplateDELETERequest templateDeleteReq = new DebtorSystemTemplateDELETERequest()
                {
                    DebtorSystemTemplateID = templateCreateRes.DebtorSystemTemplateID
                };

                await Client.DeleteAsync(templateDeleteReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
            }
        }
        #endregion

        #region "DebtorSystemTemplates_TemplateReferences"
        [Test]
        public async Task DebtorSystemTemplates_TemplateReferences_CRUD()
        {
            // Create a debtor system template
            DebtorSystemTemplatePOSTRequest templateCreateReq = new DebtorSystemTemplatePOSTRequest()
            {
                Name = $"Template-{RandomString(8)}",
                Code = RandomString(8),
                IsEnabled = true
            };

            DebtorSystemTemplate templateCreateRes = await Client.PostAsync(templateCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(templateCreateRes.DebtorSystemTemplateID, Is.Not.Null);

            try
            {
                const string validAssemblyName = "Azure.AI.OpenAI.dll";
                const string validAssemblyFullName = "Azure.AI.OpenAI, Version=2.100.24.60605, Culture=neutral, PublicKeyToken=null";

                // Create a debtor system template reference
                DebtorSystemTemplateReferencePOSTRequest referenceCreateReq = new DebtorSystemTemplateReferencePOSTRequest()
                {
                    DebtorSystemTemplateID = templateCreateRes.DebtorSystemTemplateID,
                    AssemblyName = validAssemblyName,
                    AssemblyFullName = validAssemblyFullName,
                    ItemNo = 1
                };

                DebtorSystemTemplateReference referenceCreateRes = await Client.PostAsync(referenceCreateReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                Assert.That(referenceCreateRes.DebtorSystemTemplateReferenceID, Is.Not.Null);
                Assert.That(referenceCreateRes.AssemblyName, Is.EqualTo(referenceCreateReq.AssemblyName));

                // Read the debtor system template references list
                DebtorSystemTemplateReferencesGETManyRequest referencesGetManyReq = new DebtorSystemTemplateReferencesGETManyRequest()
                {
                    DebtorSystemTemplateID = templateCreateRes.DebtorSystemTemplateID
                };

                List<DebtorSystemTemplateReference> referencesGetManyRes = await Client.GetAsync(referencesGetManyReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(referencesGetManyRes.Any(x => x.DebtorSystemTemplateReferenceID == referenceCreateRes.DebtorSystemTemplateReferenceID), Is.True);

                // Read the created reference
                DebtorSystemTemplateReferenceGETRequest referenceGetReq = new DebtorSystemTemplateReferenceGETRequest()
                {
                    DebtorSystemTemplateID = templateCreateRes.DebtorSystemTemplateID,
                    DebtorSystemTemplateReferenceID = referenceCreateRes.DebtorSystemTemplateReferenceID
                };

                DebtorSystemTemplateReference referenceGetRes = await Client.GetAsync(referenceGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(referenceGetRes.DebtorSystemTemplateReferenceID, Is.EqualTo(referenceCreateRes.DebtorSystemTemplateReferenceID));

                // Update the reference
                DebtorSystemTemplateReferencePATCHRequest referencePatchReq = new DebtorSystemTemplateReferencePATCHRequest()
                {
                    DebtorSystemTemplateID = templateCreateRes.DebtorSystemTemplateID,
                    DebtorSystemTemplateReferenceID = referenceCreateRes.DebtorSystemTemplateReferenceID,
                    AssemblyName = validAssemblyName,
                    AssemblyFullName = validAssemblyFullName,
                    ItemNo = 2
                };

                DebtorSystemTemplateReference referencePatchRes = await Client.PatchAsync(referencePatchReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(referencePatchRes.DebtorSystemTemplateReferenceID, Is.EqualTo(referencePatchReq.DebtorSystemTemplateReferenceID));
                Assert.That(referencePatchRes.AssemblyName, Is.EqualTo(referencePatchReq.AssemblyName));

                // Delete the reference
                DebtorSystemTemplateReferenceDELETERequest referenceDeleteReq = new DebtorSystemTemplateReferenceDELETERequest()
                {
                    DebtorSystemTemplateID = templateCreateRes.DebtorSystemTemplateID,
                    DebtorSystemTemplateReferenceID = referenceCreateRes.DebtorSystemTemplateReferenceID
                };

                await Client.DeleteAsync(referenceDeleteReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

                // Verify the reference was deleted
                WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
                {
                    _ = await Client.GetAsync(referenceGetReq);
                });
                Assert.That(ex.StatusCode, Is.EqualTo(404));
            }
            finally
            {
                DebtorSystemTemplateDELETERequest templateDeleteReq = new DebtorSystemTemplateDELETERequest()
                {
                    DebtorSystemTemplateID = templateCreateRes.DebtorSystemTemplateID
                };

                await Client.DeleteAsync(templateDeleteReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
            }
        }
        #endregion
    }
}


