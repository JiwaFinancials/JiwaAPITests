using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Documents;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Debtors
{
    public class DocumentTypes : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task DebtorDocumentType_CRUD()
        {
            // Create a debtor document type
            DebtorDocumentTypePOSTRequest typeCreateReq = new DebtorDocumentTypePOSTRequest()
            {
                Description = RandomString(10)
            };

            DocumentType typeCreateRes = await Client.PostAsync(typeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(typeCreateRes.DocumentTypeID, !Is.Null);
            Assert.That(typeCreateRes.Description, Is.EqualTo(typeCreateReq.Description));

            // Read the created document type using the DocumentTypeID
            DebtorDocumentTypeGETRequest typeGetReq = new DebtorDocumentTypeGETRequest() 
            { 
                DocumentTypeID = typeCreateRes.DocumentTypeID 
            };
            DocumentType typeGetRes = await Client.GetAsync(typeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(typeGetRes.Description, Is.EqualTo(typeCreateReq.Description));

            // Update the document type
            DebtorDocumentTypePATCHRequest typePatchReq = new DebtorDocumentTypePATCHRequest()
            {
                DocumentTypeID = typeCreateRes.DocumentTypeID,
                Description = RandomString(10)
            };
            DocumentType typePatchRes = await Client.PatchAsync(typePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(typePatchRes.Description, Is.EqualTo(typePatchReq.Description));

            // Delete the document type
            DebtorDocumentTypeDELETERequest typeDeleteReq = new DebtorDocumentTypeDELETERequest() 
            { 
                DocumentTypeID = typeCreateRes.DocumentTypeID 
            };
            await Client.DeleteAsync(typeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted document type is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                DocumentType getDeletedRes = await Client.GetAsync(typeGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Try to GET non-existent document type to make sure we get a 404
            typeGetReq.DocumentTypeID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                DocumentType typeGetRes = await Client.GetAsync(typeGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task DebtorDocumentTypes_GetMany()
        {
            // Create a few document types
            List<DocumentType> createdTypes = new List<DocumentType>();
            for (int i = 0; i < 2; i++)
            {
                DebtorDocumentTypePOSTRequest typeCreateReq = new DebtorDocumentTypePOSTRequest()
                {
                    Description = RandomString(10)
                };

                DocumentType typeCreateRes = await Client.PostAsync(typeCreateReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
                createdTypes.Add(typeCreateRes);
            }

            // Get the list of document types
            DebtorDocumentTypesGETManyRequest typesGetManyReq = new DebtorDocumentTypesGETManyRequest();
            List<DocumentType> typesGetManyRes = await Client.GetAsync(typesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(typesGetManyRes.Count, Is.GreaterThanOrEqualTo(2));

            // Clean up - delete the document types
            foreach (var type in createdTypes)
            {
                DebtorDocumentTypeDELETERequest typeDeleteReq = new DebtorDocumentTypeDELETERequest() 
                { 
                    DocumentTypeID = type.DocumentTypeID 
                };
                await Client.DeleteAsync(typeDeleteReq);
                Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
            }
        }
        #endregion
    }
}

