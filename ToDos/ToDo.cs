using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Threading.Tasks;
using ToDoDto = JiwaFinancials.Jiwa.JiwaServiceModel.ToDos.ToDo;

namespace JiwaAPITests.ToDos
{
    public class ToDo : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task ToDo_CRUD()
        {
            // Create a to do record.
            ToDoPOSTRequest toDoCreateReq = new ToDoPOSTRequest()
            {
                Subject = "To Do " + RandomString(8),
                Body = "Created to do " + RandomString(12),
                ReminderEnabled = false
            };

            ToDoDto toDoCreateRes = await Client.PostAsync(toDoCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(toDoCreateRes.ToDoID, Is.Not.Null.And.Not.Empty);
            Assert.That(toDoCreateRes.Subject, Is.EqualTo(toDoCreateReq.Subject));

            // Read the created to do record.
            ToDoGETRequest toDoGetReq = new ToDoGETRequest()
            {
                ToDoID = toDoCreateRes.ToDoID
            };

            ToDoDto toDoGetRes = await Client.GetAsync(toDoGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(toDoGetRes.ToDoID, Is.EqualTo(toDoCreateRes.ToDoID));
            Assert.That(toDoGetRes.Subject, Is.EqualTo(toDoCreateReq.Subject));

            // Update the to do record.
            ToDoPATCHRequest toDoPatchReq = new ToDoPATCHRequest()
            {
                ToDoID = toDoCreateRes.ToDoID,
                Subject = "Updated To Do " + RandomString(8),
                Body = "Updated to do " + RandomString(10)
            };

            ToDoDto toDoPatchRes = await Client.PatchAsync(toDoPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(toDoPatchRes.ToDoID, Is.EqualTo(toDoCreateRes.ToDoID));
            Assert.That(toDoPatchRes.Subject, Is.EqualTo(toDoPatchReq.Subject));

            // Verify the to do record was updated.
            toDoGetRes = await Client.GetAsync(toDoGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(toDoGetRes.Subject, Is.EqualTo(toDoPatchReq.Subject));
            Assert.That(toDoGetRes.Body, Is.EqualTo(toDoPatchReq.Body));

            // Delete the to do record.
            ToDoDELETERequest toDoDeleteReq = new ToDoDELETERequest()
            {
                ToDoID = toDoCreateRes.ToDoID
            };

            await Client.DeleteAsync(toDoDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the to do record was deleted.
            WebServiceException toDoDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(toDoGetReq);
            });
            Assert.That(toDoDeleteEx.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}
