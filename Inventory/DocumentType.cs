using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;

namespace JiwaAPITests.Inventory
{
    public class DocumentType : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task InventoryDocumentType_CRUD()
        {
            // Create a document type
            InventoryDocumentTypePOSTRequest createReq = new InventoryDocumentTypePOSTRequest()
            {
                Description = "DocType " + RandomString(5),
                DefaultType = false
            };
            InventoryDocumentTypeDto createRes = await Client.PostAsync(createReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Read all document types
            List<InventoryDocumentTypeDto> getManyRes = await Client.GetAsync(new InventoryDocumentTypesGETManyRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(getManyRes.Any(x => x.DocumentTypeID == createRes.DocumentTypeID), Is.True);

            // Read and update the created document type
            InventoryDocumentTypeGETRequest getReq = new InventoryDocumentTypeGETRequest() { DocumentTypeID = createRes.DocumentTypeID };
            InventoryDocumentTypeDto getRes = await Client.GetAsync(getReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            InventoryDocumentTypePATCHRequest patchReq = new InventoryDocumentTypePATCHRequest()
            {
                DocumentTypeID = createRes.DocumentTypeID,
                Description = "Updated " + RandomString(5)
            };
            InventoryDocumentTypeDto patchRes = await Client.PatchAsync(patchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(patchRes.DocumentTypeID, Is.EqualTo(patchReq.DocumentTypeID));
            Assert.That(patchRes.DocumentTypeID, Is.EqualTo(createRes.DocumentTypeID));

            // Delete the document type
            await Client.DeleteAsync(new InventoryDocumentTypeDELETERequest() { DocumentTypeID = createRes.DocumentTypeID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}


