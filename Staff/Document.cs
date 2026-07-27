using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HRStaffDto = JiwaFinancials.Jiwa.JiwaServiceModel.Tables.HR_Staff;
using StaffTimesheetDto = JiwaFinancials.Jiwa.JiwaServiceModel.Staff.Timesheets.Timesheet;
using TimesheetDocumentDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.Document;
using TimesheetDocumentTypeDto = JiwaFinancials.Jiwa.JiwaServiceModel.Documents.DocumentType;

namespace JiwaAPITests.Staff
{
    public class Document : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task StaffTimesheetDocument_CRUD()
        {
            // Read the Admin staff member to use for timesheet creation.
            HR_StaffQuery adminStaffQueryReq = new HR_StaffQuery()
            {
                Username = "Admin",
                Take = 1
            };

            QueryResponse<HRStaffDto> adminStaffQueryRes = await Client.GetAsync(adminStaffQueryReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            HRStaffDto adminStaff = adminStaffQueryRes.Results.FirstOrDefault();
            Assert.That(adminStaff, Is.Not.Null);
            Assert.That(adminStaff.StaffID, Is.Not.Null.And.Not.Empty);

            // Create a staff timesheet.
            StaffTimesheetPOSTRequest timesheetCreateReq = new StaffTimesheetPOSTRequest()
            {
                StaffID = adminStaff.StaffID,
                StaffUserName = adminStaff.Username,
                TimeSheetDate = DateTimeOffset.UtcNow,
                Reference = "Timesheet Docs " + RandomString(6),
                IsActivated = false
            };

            StaffTimesheetDto timesheetCreateRes = await Client.PostAsync(timesheetCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(timesheetCreateRes.TimesheetID, Is.Not.Null.And.Not.Empty);

            // Create a timesheet document type to use for document creation.
            TimesheetDocumentTypePOSTRequest documentTypeCreateReq = new TimesheetDocumentTypePOSTRequest()
            {
                Description = "Timesheet DocType " + RandomString(8),
                DefaultType = false
            };

            TimesheetDocumentTypeDto documentTypeCreateRes = await Client.PostAsync(documentTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentTypeCreateRes.DocumentTypeID, Is.Not.Null.And.Not.Empty);

            // Append a document to the timesheet.
            TimesheetDocumentPOSTRequest documentCreateReq = new TimesheetDocumentPOSTRequest()
            {
                TimesheetID = timesheetCreateRes.TimesheetID,
                Description = "Timesheet document " + RandomString(6),
                PhysicalFileName = "TimesheetDocument.txt",
                FileBinary = Encoding.UTF8.GetBytes("Timesheet document content"),
                DocumentType = new TimesheetDocumentTypeDto()
                {
                    DocumentTypeID = documentTypeCreateRes.DocumentTypeID
                }
            };

            TimesheetDocumentDto documentCreateRes = await Client.PostAsync(documentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(documentCreateRes.DocumentID, Is.Not.Null.And.Not.Empty);
            Assert.That(documentCreateRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Read all documents for the timesheet.
            TimesheetDocumentsGETManyRequest documentsGetManyReq = new TimesheetDocumentsGETManyRequest()
            {
                TimesheetID = timesheetCreateRes.TimesheetID
            };

            List<TimesheetDocumentDto> documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.True);

            // Read the appended document.
            TimesheetDocumentGETRequest documentGetReq = new TimesheetDocumentGETRequest()
            {
                TimesheetID = timesheetCreateRes.TimesheetID,
                DocumentID = documentCreateRes.DocumentID
            };

            TimesheetDocumentDto documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentCreateReq.Description));

            // Update the timesheet document.
            TimesheetDocumentPATCHRequest documentPatchReq = new TimesheetDocumentPATCHRequest()
            {
                TimesheetID = timesheetCreateRes.TimesheetID,
                DocumentID = documentCreateRes.DocumentID,
                Description = "Updated document " + RandomString(6)
            };

            TimesheetDocumentDto documentPatchRes = await Client.PatchAsync(documentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentPatchRes.DocumentID, Is.EqualTo(documentCreateRes.DocumentID));
            Assert.That(documentPatchRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Verify the timesheet document was updated.
            documentGetRes = await Client.GetAsync(documentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentGetRes.Description, Is.EqualTo(documentPatchReq.Description));

            // Delete the timesheet document.
            TimesheetDocumentDELETERequest documentDeleteReq = new TimesheetDocumentDELETERequest()
            {
                TimesheetID = timesheetCreateRes.TimesheetID,
                DocumentID = documentCreateRes.DocumentID
            };

            await Client.DeleteAsync(documentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the timesheet document was deleted.
            WebServiceException documentDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(documentGetReq);
            });
            Assert.That(documentDeleteEx.StatusCode, Is.EqualTo(404));

            // Read all documents and ensure the deleted document is no longer returned.
            documentsGetManyRes = await Client.GetAsync(documentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(documentsGetManyRes.Any(x => x.DocumentID == documentCreateRes.DocumentID), Is.False);

            // Delete the created timesheet document type.
            TimesheetDocumentTypeDELETERequest documentTypeDeleteReq = new TimesheetDocumentTypeDELETERequest()
            {
                DocumentTypeID = documentTypeCreateRes.DocumentTypeID
            };

            await Client.DeleteAsync(documentTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Delete the created staff timesheet.
            StaffTimesheetDELETERequest timesheetDeleteReq = new StaffTimesheetDELETERequest()
            {
                TimesheetID = timesheetCreateRes.TimesheetID
            };

            await Client.DeleteAsync(timesheetDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion
    }
}
