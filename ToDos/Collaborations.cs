using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CollaborationDto = JiwaFinancials.Jiwa.JiwaServiceModel.ToDos.Collaboration;
using ToDoDto = JiwaFinancials.Jiwa.JiwaServiceModel.ToDos.ToDo;

namespace JiwaAPITests.ToDos
{
    public class Collaborations : JiwaAPITest
    {
        private async Task<ToDoDto> CreateToDoAsync()
        {
            ToDoPOSTRequest toDoCreateReq = new ToDoPOSTRequest()
            {
                Subject = "To Do " + RandomString(8),
                Body = "To Do for collaboration " + RandomString(10),
                ReminderEnabled = false
            };

            ToDoDto toDoCreateRes = await Client.PostAsync(toDoCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(toDoCreateRes.ToDoID, Is.Not.Null.And.Not.Empty);

            return toDoCreateRes;
        }

        private async Task<string> GetStaffIdForCollaborationAsync(ToDoDto toDo)
        {
            if (!string.IsNullOrWhiteSpace(toDo.AssignedTo?.StaffID))
            {
                return toDo.AssignedTo.StaffID;
            }

            if (!string.IsNullOrWhiteSpace(toDo.AssignedBy?.StaffID))
            {
                return toDo.AssignedBy.StaffID;
            }

            HR_StaffQuery staffQueryReq = new HR_StaffQuery()
            {
                Take = 1,
                OrderBy = "StaffID"
            };

            QueryResponse<HR_Staff> staffQueryRes = await Client.GetAsync(staffQueryReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(staffQueryRes.Results, Is.Not.Null);

            HR_Staff staff = staffQueryRes.Results.FirstOrDefault();
            Assert.That(staff, Is.Not.Null);
            Assert.That(staff.StaffID, Is.Not.Null.And.Not.Empty);

            return staff.StaffID;
        }

        #region "{Collaborations}"
        [Test]
        public async Task ToDo_Collaborations_CRUD()
        {
            // Create a to do record.
            ToDoDto toDoCreateRes = await CreateToDoAsync();
            string staffId = await GetStaffIdForCollaborationAsync(toDoCreateRes);

            // Add a collaboration to the to do.
            ToDoCollaborationPOSTRequest collaborationCreateReq = new ToDoCollaborationPOSTRequest()
            {
                ToDoID = toDoCreateRes.ToDoID,
                Staff = staffId,
                NotificationEnabled = true,
                NotificationText = "Notify collaborator " + RandomString(8)
            };

            CollaborationDto collaborationCreateRes = await Client.PostAsync(collaborationCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(collaborationCreateRes.CollID, Is.Not.Null.And.Not.Empty);
            Assert.That(collaborationCreateRes.Staff, Is.EqualTo(collaborationCreateReq.Staff));

            // Read collaborations for the to do and verify the created collaboration is returned.
            ToDoCollaborationGETManyRequest collaborationGetManyReq = new ToDoCollaborationGETManyRequest()
            {
                ToDoID = toDoCreateRes.ToDoID
            };

            List<CollaborationDto> collaborationGetManyRes = await Client.GetAsync(collaborationGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(collaborationGetManyRes.Any(x => x.CollID == collaborationCreateRes.CollID), Is.True);

            // Read the created collaboration.
            ToDoCollaborationGETRequest collaborationGetReq = new ToDoCollaborationGETRequest()
            {
                ToDoID = toDoCreateRes.ToDoID,
                CollID = collaborationCreateRes.CollID
            };

            CollaborationDto collaborationGetRes = await Client.GetAsync(collaborationGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(collaborationGetRes.CollID, Is.EqualTo(collaborationCreateRes.CollID));
            Assert.That(collaborationGetRes.Staff, Is.EqualTo(collaborationCreateReq.Staff));

            // Update the collaboration for the to do.
            ToDoCollaborationPATCHRequest collaborationPatchReq = new ToDoCollaborationPATCHRequest()
            {
                ToDoID = toDoCreateRes.ToDoID,
                CollID = collaborationCreateRes.CollID,
                NotificationEnabled = false,
                NotificationText = "Updated collaboration " + RandomString(8)
            };

            CollaborationDto collaborationPatchRes = await Client.PatchAsync(collaborationPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(collaborationPatchRes.CollID, Is.EqualTo(collaborationPatchReq.CollID));
            Assert.That(collaborationPatchRes.CollID, Is.EqualTo(collaborationCreateRes.CollID));
            Assert.That(collaborationPatchRes.NotificationText, Is.EqualTo(collaborationPatchReq.NotificationText));

            // Verify the collaboration was updated.
            collaborationGetRes = await Client.GetAsync(collaborationGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(collaborationGetRes.NotificationText, Is.EqualTo(collaborationPatchReq.NotificationText));
            Assert.That(collaborationGetRes.NotificationEnabled, Is.EqualTo(collaborationPatchReq.NotificationEnabled));

            // Delete the collaboration from the to do.
            ToDoCollaborationDELETERequest collaborationDeleteReq = new ToDoCollaborationDELETERequest()
            {
                ToDoID = toDoCreateRes.ToDoID,
                CollID = collaborationCreateRes.CollID
            };

            await Client.DeleteAsync(collaborationDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the collaboration was deleted.
            WebServiceException collaborationDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(collaborationGetReq);
            });
            Assert.That(collaborationDeleteEx.StatusCode, Is.EqualTo(404));

            // Read collaborations and verify the deleted collaboration is no longer returned.
            collaborationGetManyRes = await Client.GetAsync(collaborationGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(collaborationGetManyRes.Any(x => x.CollID == collaborationCreateRes.CollID), Is.False);

            // Delete the to do record.
            await Client.DeleteAsync(new ToDoDELETERequest() { ToDoID = toDoCreateRes.ToDoID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

