using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.GoodsReceivedNotes;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoodsReceivedNoteDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;

namespace JiwaAPITests.GoodsReceivedNotes
{
    public class DocumentTypes : GoodsReceivedNotesTestBase
    {
        #region "GoodsReceivedNotes_DocumentTypes"
        [Test]
        public async Task GoodsReceivedNotes_DocumentTypes_CRUD()
        {
            // Create a goods received note document type.
            GoodsReceivedNoteDocumentTypePOSTRequest documentTypeCreateReq = new GoodsReceivedNoteDocumentTypePOSTRequest()
            {
                Description = RandomString(12),
                DefaultType = false
            };

            GoodsReceivedNoteDocumentTypeDto documentTypeCreateRes = await Client.PostAsync(documentTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentTypeCreateRes.DocumentTypeID, Is.Not.Null);
            Assert.That(documentTypeCreateRes.Description, Is.EqualTo(documentTypeCreateReq.Description));

            // Read all goods received note document types.
            GoodsReceivedNoteDocumentTypesGETManyRequest documentTypesGetManyReq = new GoodsReceivedNoteDocumentTypesGETManyRequest();
            List<GoodsReceivedNoteDocumentTypeDto> documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Any(x => x.DocumentTypeID == documentTypeCreateRes.DocumentTypeID), Is.True);

            // Read the created goods received note document type.
            GoodsReceivedNoteDocumentTypeGETRequest documentTypeGetReq = new GoodsReceivedNoteDocumentTypeGETRequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            GoodsReceivedNoteDocumentTypeDto documentTypeGetRes = await Client.GetAsync(documentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypeGetRes.DocumentTypeID, Is.EqualTo(documentTypeCreateRes.DocumentTypeID));

            // Update the goods received note document type.
            GoodsReceivedNoteDocumentTypePATCHRequest documentTypePatchReq = new GoodsReceivedNoteDocumentTypePATCHRequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID,
                Description = "Updated " + RandomString(10)
            };

            GoodsReceivedNoteDocumentTypeDto documentTypePatchRes = await Client.PatchAsync(documentTypePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypePatchRes.DocumentTypeID, Is.EqualTo(documentTypePatchReq.DocumentTypeID));
            Assert.That(documentTypePatchRes.DocumentTypeID, Is.EqualTo(documentTypeCreateRes.DocumentTypeID));
            Assert.That(documentTypePatchRes.Description, Is.EqualTo(documentTypePatchReq.Description));

            // Delete the goods received note document type.
            GoodsReceivedNoteDocumentTypeDELETERequest documentTypeDeleteReq = new GoodsReceivedNoteDocumentTypeDELETERequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            await Client.DeleteAsync(documentTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the goods received note document type was deleted.
            WebServiceException documentTypeDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(documentTypeGetReq);
            });
            Assert.That(documentTypeDeleteEx.StatusCode, Is.EqualTo(404));

            documentTypesGetManyRes = await Client.GetAsync(documentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentTypesGetManyRes.Any(x => x.DocumentTypeID == documentTypeCreateRes.DocumentTypeID), Is.False);
        }
        #endregion
    }
}


