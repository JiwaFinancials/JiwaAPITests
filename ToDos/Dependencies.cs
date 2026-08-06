using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DependencyDto = JiwaFinancials.Jiwa.JiwaServiceModel.ToDos.Dependency;
using ToDoDto = JiwaFinancials.Jiwa.JiwaServiceModel.ToDos.ToDo;

namespace JiwaAPITests.ToDos
{
    public class Dependencies : JiwaAPITest
    {
        private async Task<ToDoDto> CreateToDoAsync(string subjectSuffix)
        {
            ToDoPOSTRequest toDoCreateReq = new ToDoPOSTRequest()
            {
                Subject = "To Do " + subjectSuffix + " " + RandomString(8),
                Body = "To Do dependency test " + RandomString(10),
                ReminderEnabled = false
            };

            ToDoDto toDoCreateRes = await Client.PostAsync(toDoCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(toDoCreateRes.ToDoID, Is.Not.Null.And.Not.Empty);

            return toDoCreateRes;
        }

        #region "{Dependencies}"
        [Test]
        public async Task ToDo_Dependencies_CRUD()
        {
            // Create a to do record that will hold dependencies.
            ToDoDto toDoCreateRes = await CreateToDoAsync("Primary");

            // Create a second to do record to use as the dependency target.
            ToDoDto dependencyToDoCreateRes = await CreateToDoAsync("Dependency");

            // Create a third to do record to use as the dependency PATCH target.
            ToDoDto dependencyPatchToDoCreateRes = await CreateToDoAsync("Dependency2");

            // Add a dependency to the to do.
            ToDoDependencyPOSTRequest dependencyCreateReq = new ToDoDependencyPOSTRequest()
            {
                ToDoID = toDoCreateRes.ToDoID,
                DependencyRecID = dependencyToDoCreateRes.ToDoID,
                ItemNo = 1
            };

            DependencyDto dependencyCreateRes = await Client.PostAsync(dependencyCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(dependencyCreateRes.DepID, Is.Not.Null.And.Not.Empty);
            Assert.That(dependencyCreateRes.DependencyRecID, Is.EqualTo(dependencyCreateReq.DependencyRecID));

            // Read dependencies for the to do and verify the created dependency is returned.
            ToDoDependencyGETManyRequest dependencyGetManyReq = new ToDoDependencyGETManyRequest()
            {
                ToDoID = toDoCreateRes.ToDoID
            };

            List<DependencyDto> dependencyGetManyRes = await Client.GetAsync(dependencyGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(dependencyGetManyRes.Any(x => x.DepID == dependencyCreateRes.DepID), Is.True);

            // Read the created dependency.
            ToDoDependencyGETRequest dependencyGetReq = new ToDoDependencyGETRequest()
            {
                ToDoID = toDoCreateRes.ToDoID,
                DepID = dependencyCreateRes.DepID
            };

            DependencyDto dependencyGetRes = await Client.GetAsync(dependencyGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(dependencyGetRes.DepID, Is.EqualTo(dependencyCreateRes.DepID));
            Assert.That(dependencyGetRes.DependencyRecID, Is.EqualTo(dependencyCreateReq.DependencyRecID));

            // Update the dependency for the to do.
            ToDoDependencyPATCHRequest dependencyPatchReq = new ToDoDependencyPATCHRequest()
            {
                ToDoID = toDoCreateRes.ToDoID,
                DepID = dependencyCreateRes.DepID,
                DependencyRecID = dependencyPatchToDoCreateRes.ToDoID 
            };

            DependencyDto dependencyPatchRes = await Client.PatchAsync(dependencyPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(dependencyPatchRes.DepID, Is.EqualTo(dependencyPatchReq.DepID));
            Assert.That(dependencyPatchRes.DepID, Is.EqualTo(dependencyCreateRes.DepID));
            Assert.That(dependencyPatchRes.DependencyRecID, Is.EqualTo(dependencyPatchReq.DependencyRecID));

            // Verify the dependency was updated.
            dependencyGetRes = await Client.GetAsync(dependencyGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(dependencyGetRes.DependencyRecID, Is.EqualTo(dependencyPatchReq.DependencyRecID));

            // Delete the dependency from the to do.
            ToDoDependencyDELETERequest dependencyDeleteReq = new ToDoDependencyDELETERequest()
            {
                ToDoID = toDoCreateRes.ToDoID,
                DepID = dependencyCreateRes.DepID
            };

            await Client.DeleteAsync(dependencyDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the dependency was deleted.
            WebServiceException dependencyDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(dependencyGetReq);
            });
            Assert.That(dependencyDeleteEx.StatusCode, Is.EqualTo(404));

            // Read dependencies and verify the deleted dependency is no longer returned.
            dependencyGetManyRes = await Client.GetAsync(dependencyGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(dependencyGetManyRes.Any(x => x.DepID == dependencyCreateRes.DepID), Is.False);

            // Delete the created to do records.
            await Client.DeleteAsync(new ToDoDELETERequest() { ToDoID = toDoCreateRes.ToDoID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            await Client.DeleteAsync(new ToDoDELETERequest() { ToDoID = dependencyToDoCreateRes.ToDoID });
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}

