using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.DebtorSystemTemplates;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Debtors
{
    public class DebtorSystems : JiwaAPITest
    {
        #region "DebtorSystems"
        [Test]
        public async Task DebtorSystem_CRUD()
        {
            // Create a debtor to associate with the system
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

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

            // Create a debtor system
            DebtorSystemPOSTRequest systemCreateReq = new DebtorSystemPOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                Description = RandomString(10),
                DebtorSystemTemplateID = createRes.DebtorSystemTemplateID
            };

            DebtorSystem systemCreateRes = await Client.PostAsync(systemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(systemCreateRes.SystemID, !Is.Null);
            Assert.That(systemCreateRes.Description, Is.EqualTo(systemCreateReq.Description));

            // Read the created debtor system using the SystemID
            DebtorSystemGETRequest systemGetReq = new DebtorSystemGETRequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID,
                SystemID = systemCreateRes.SystemID 
            };
            DebtorSystem systemGetRes = await Client.GetAsync(systemGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(systemGetRes.Description, Is.EqualTo(systemCreateReq.Description));

            // Update the debtor system
            DebtorSystemPATCHRequest systemPatchReq = new DebtorSystemPATCHRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                SystemID = systemCreateRes.SystemID,
                Description = RandomString(10)
            };
            DebtorSystem systemPatchRes = await Client.PatchAsync(systemPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(systemPatchRes.SystemID, Is.EqualTo(systemPatchReq.SystemID));
            Assert.That(systemPatchRes.Description, Is.EqualTo(systemPatchReq.Description));

            // Delete the debtor system
            DebtorSystemDELETERequest systemDeleteReq = new DebtorSystemDELETERequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID,
                SystemID = systemCreateRes.SystemID 
            };
            await Client.DeleteAsync(systemDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted system is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                DebtorSystem getDeletedRes = await Client.GetAsync(systemGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Clean up the test debtor
            DebtorDELETERequest debtorDeleteReq = new DebtorDELETERequest() { DebtorID = debtorCreateRes.DebtorID };
            await Client.DeleteAsync(debtorDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }

        [Test]
        public async Task DebtorSystems_GetMany()
        {
            // Create a debtor to associate with systems
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

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

            // Create a few debtor systems
            List<DebtorSystem> createdSystems = new List<DebtorSystem>();
            for (int i = 0; i < 2; i++)
            {
                DebtorSystemPOSTRequest systemCreateReq = new DebtorSystemPOSTRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    Description = RandomString(10),
                    DebtorSystemTemplateID = createRes.DebtorSystemTemplateID
                };

                DebtorSystem systemCreateRes = await Client.PostAsync(systemCreateReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                createdSystems.Add(systemCreateRes);
            }

            // Get the list of debtor systems
            DebtorSystemsGETManyRequest systemsGetManyReq = new DebtorSystemsGETManyRequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID 
            };
            List<DebtorSystem> systemsGetManyRes = await Client.GetAsync(systemsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(systemsGetManyRes.Count, Is.GreaterThanOrEqualTo(2));

            // Clean up - delete the systems
            foreach (var system in createdSystems)
            {
                DebtorSystemDELETERequest systemDeleteReq = new DebtorSystemDELETERequest() 
                { 
                    DebtorID = debtorCreateRes.DebtorID,
                    SystemID = system.SystemID 
                };
                await Client.DeleteAsync(systemDeleteReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
            }

            // Clean up - delete the debtor
            DebtorDELETERequest debtorDeleteReq = new DebtorDELETERequest() { DebtorID = debtorCreateRes.DebtorID };
            await Client.DeleteAsync(debtorDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion

        #region "DebtorSystemFields"
        [Test]
        public async Task DebtorSystemFields_CRUD()
        {
            // Create a debtor to associate with the system
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

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

            // Create a debtor system
            DebtorSystemPOSTRequest systemCreateReq = new DebtorSystemPOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                Description = RandomString(10),
                DebtorSystemTemplateID = createRes.DebtorSystemTemplateID 
            };

            DebtorSystem systemCreateRes = await Client.PostAsync(systemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Retrieve the debtor system to get its fields
            DebtorSystemGETRequest systemGetReq = new DebtorSystemGETRequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID,
                SystemID = systemCreateRes.SystemID 
            };
            DebtorSystem systemGetRes = await Client.GetAsync(systemGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // If the system has fields, test field operations
            if (systemGetRes.Fields != null && systemGetRes.Fields.Count > 0)
            {
                DebtorSystemField firstField = systemGetRes.Fields.First();

                // Get the debtor system field
                DebtorSystemFieldGETRequest fieldGetReq = new DebtorSystemFieldGETRequest() 
                { 
                    DebtorID = debtorCreateRes.DebtorID,
                    SystemID = systemCreateRes.SystemID,
                    SystemFieldID = firstField.SystemFieldID 
                };
                DebtorSystemField fieldGetRes = await Client.GetAsync(fieldGetReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

                // Update the debtor system field
                DebtorSystemFieldPATCHRequest fieldPatchReq = new DebtorSystemFieldPATCHRequest() 
                { 
                    DebtorID = debtorCreateRes.DebtorID,
                    SystemID = systemCreateRes.SystemID,
                    SystemFieldID = firstField.SystemFieldID,
                    Contents = RandomString(20)
                };
                DebtorSystemField fieldPatchRes = await Client.PatchAsync(fieldPatchReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                Assert.That(fieldPatchRes.SystemFieldID, Is.EqualTo(fieldPatchReq.SystemFieldID));
                Assert.That(fieldPatchRes.Contents, Is.EqualTo(fieldPatchReq.Contents));
            }

            // Get the list of debtor system fields
            DebtorSystemFieldsGETManyRequest fieldsGetManyReq = new DebtorSystemFieldsGETManyRequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID,
                SystemID = systemCreateRes.SystemID 
            };
            List<DebtorSystemField> fieldsGetManyRes = await Client.GetAsync(fieldsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Clean up - delete the system (which will cascade to fields)
            DebtorSystemDELETERequest systemDeleteReq = new DebtorSystemDELETERequest() 
            { 
                DebtorID = debtorCreateRes.DebtorID,
                SystemID = systemCreateRes.SystemID 
            };
            await Client.DeleteAsync(systemDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Clean up - delete the debtor
            DebtorDELETERequest debtorDeleteReq = new DebtorDELETERequest() { DebtorID = debtorCreateRes.DebtorID };
            await Client.DeleteAsync(debtorDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}


