using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseOrderNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;

namespace JiwaAPITests.PurchaseOrders
{
    public class NoteType : PurchaseOrderTestBase
    {
        #region "PurchaseOrders_NoteTypes"
        [Test]
        public async Task PurchaseOrders_NoteTypes_CRUD()
        {
            // Create a purchase order note type.
            PurchaseOrderNoteTypePOSTRequest noteTypeCreateReq = new PurchaseOrderNoteTypePOSTRequest()
            {
                Description = RandomString(12),
                DefaultType = false
            };

            PurchaseOrderNoteTypeDto noteTypeCreateRes = await Client.PostAsync(noteTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteTypeCreateRes.NoteTypeID, Is.Not.Null);
            Assert.That(noteTypeCreateRes.Description, Is.EqualTo(noteTypeCreateReq.Description));

            // Read all purchase order note types.
            PurchaseOrderNoteTypesGETManyRequest noteTypesGetManyReq = new PurchaseOrderNoteTypesGETManyRequest();
            List<PurchaseOrderNoteTypeDto> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Any(x => x.NoteTypeID == noteTypeCreateRes.NoteTypeID), Is.True);

            // Read the created purchase order note type.
            PurchaseOrderNoteTypeGETRequest noteTypeGetReq = new PurchaseOrderNoteTypeGETRequest()
            {
                NoteTypeID = noteTypeCreateRes.NoteTypeID
            };

            PurchaseOrderNoteTypeDto noteTypeGetRes = await Client.GetAsync(noteTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypeGetRes.NoteTypeID, Is.EqualTo(noteTypeCreateRes.NoteTypeID));
            Assert.That(noteTypeGetRes.Description, Is.EqualTo(noteTypeCreateReq.Description));

            // Update the purchase order note type.
            PurchaseOrderNoteTypePATCHRequest noteTypePatchReq = new PurchaseOrderNoteTypePATCHRequest()
            {
                NoteTypeID = noteTypeCreateRes.NoteTypeID,
                Description = "Updated " + RandomString(10)
            };

            PurchaseOrderNoteTypeDto noteTypePatchRes = await Client.PatchAsync(noteTypePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypePatchRes.NoteTypeID, Is.EqualTo(noteTypePatchReq.NoteTypeID));
            Assert.That(noteTypePatchRes.NoteTypeID, Is.EqualTo(noteTypeCreateRes.NoteTypeID));
            Assert.That(noteTypePatchRes.Description, Is.EqualTo(noteTypePatchReq.Description));

            // Read the updated purchase order note type.
            noteTypeGetRes = await Client.GetAsync(noteTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypeGetRes.Description, Is.EqualTo(noteTypePatchReq.Description));

            // Delete the purchase order note type.
            PurchaseOrderNoteTypeDELETERequest noteTypeDeleteReq = new PurchaseOrderNoteTypeDELETERequest()
            {
                NoteTypeID = noteTypeCreateRes.NoteTypeID
            };

            await Client.DeleteAsync(noteTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Confirm the purchase order note type was deleted.
            WebServiceException noteTypeDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(noteTypeGetReq);
            });
            Assert.That(noteTypeDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all note types and ensure the deleted type is no longer returned.
            noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Any(x => x.NoteTypeID == noteTypeCreateRes.NoteTypeID), Is.False);
        }
        #endregion
    }
}


