using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryNoteTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Notes.NoteType;

namespace JiwaAPITests.Inventory
{
    public class NoteType : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task InventoryNoteType_CRUD()
        {
            // Create a note type
            InventoryNoteTypePOSTRequest createReq = new InventoryNoteTypePOSTRequest()
            {
                Description = "NoteType " + RandomString(5),
                DefaultType = false
            };
            InventoryNoteTypeDto createRes = await Client.PostAsync(createReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            // Read all note types
            List<InventoryNoteTypeDto> getManyRes = await Client.GetAsync(new InventoryNoteTypesGETManyRequest());
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(getManyRes.Any(x => x.NoteTypeID == createRes.NoteTypeID), Is.True);

            // Read and update the created note type
            InventoryNoteTypeGETRequest getReq = new InventoryNoteTypeGETRequest() { NoteTypeID = createRes.NoteTypeID };
            InventoryNoteTypeDto getRes = await Client.GetAsync(getReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            InventoryNoteTypePATCHRequest patchReq = new InventoryNoteTypePATCHRequest()
            {
                NoteTypeID = createRes.NoteTypeID,
                Description = "Updated " + RandomString(5)
            };
            InventoryNoteTypeDto patchRes = await Client.PatchAsync(patchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(patchRes.NoteTypeID, Is.EqualTo(createRes.NoteTypeID));

            // Delete the note type
            await Client.DeleteAsync(new InventoryNoteTypeDELETERequest() { NoteTypeID = createRes.NoteTypeID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

