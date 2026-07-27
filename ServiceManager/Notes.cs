using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Notes;
using JiwaFinancials.Jiwa.JiwaServiceModel.ServiceManager;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.ServiceManager
{
    public class Notes : ServiceManagerTestBase
    {
        #region "{Main}"
        [Test]
        public async Task ServiceManager_Jobs_Tasks_Notes_CRUD()
        {
            // Create a service manager job.
            Job jobCreateRes = await CreateJobAsync();

            // Create a service manager task.
            ServiceManagerTask taskCreateRes = await CreateTaskAsync(jobCreateRes.JobID);

            // Read all task note types and use one for note creation.
            ServiceManagerTaskNoteTypesGETManyRequest noteTypesGetManyReq = new ServiceManagerTaskNoteTypesGETManyRequest();
            List<NoteType> noteTypesGetManyRes = await Client.GetAsync(noteTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteTypesGetManyRes.Count, Is.GreaterThan(0));

            // Read all notes for the service manager task.
            ServiceManagerTaskNotesGETManyRequest notesGetManyReq = new ServiceManagerTaskNotesGETManyRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID
            };

            List<Note> notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes, Is.Not.Null);

            // Append a note to the service manager task.
            ServiceManagerTaskNotePOSTRequest noteCreateReq = new ServiceManagerTaskNotePOSTRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                NoteText = "Task note " + RandomString(8),
                NoteType = new NoteType()
                {
                    NoteTypeID = noteTypesGetManyRes.First().NoteTypeID
                }
            };

            Note noteCreateRes = await Client.PostAsync(noteCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(noteCreateRes.NoteID, Is.Not.Null);
            Assert.That(noteCreateRes.NoteText, Is.EqualTo(noteCreateReq.NoteText));

            // Read all notes again and ensure the created note is returned.
            notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.True);

            // Read the created note.
            ServiceManagerTaskNoteGETRequest noteGetReq = new ServiceManagerTaskNoteGETRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                NoteID = noteCreateRes.NoteID
            };

            Note noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));

            // Update the created note.
            ServiceManagerTaskNotePATCHRequest notePatchReq = new ServiceManagerTaskNotePATCHRequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                NoteID = noteCreateRes.NoteID,
                NoteText = "Updated task note " + RandomString(8)
            };

            Note notePatchRes = await Client.PatchAsync(notePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notePatchRes.NoteID, Is.EqualTo(noteCreateRes.NoteID));
            Assert.That(notePatchRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Verify the note was updated.
            noteGetRes = await Client.GetAsync(noteGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(noteGetRes.NoteText, Is.EqualTo(notePatchReq.NoteText));

            // Delete the created note.
            ServiceManagerTaskNoteDELETERequest noteDeleteReq = new ServiceManagerTaskNoteDELETERequest()
            {
                JobID = jobCreateRes.JobID,
                TaskID = taskCreateRes.TaskID,
                NoteID = noteCreateRes.NoteID
            };

            await Client.DeleteAsync(noteDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the note was deleted.
            WebServiceException noteDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(noteGetReq);
            });
            Assert.That(noteDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all notes again and ensure the deleted note is no longer returned.
            notesGetManyRes = await Client.GetAsync(notesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(notesGetManyRes.Any(x => x.NoteID == noteCreateRes.NoteID), Is.False);

            // Clean up the created service manager task.
            await Client.DeleteAsync(new ServiceManagerTasksDELETERequest() { JobID = jobCreateRes.JobID, TaskID = taskCreateRes.TaskID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Clean up the created service manager job.
            await Client.DeleteAsync(new ServiceManagerJobDELETERequest() { JobID = jobCreateRes.JobID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

