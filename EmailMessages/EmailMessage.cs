using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Documents;
using JiwaFinancials.Jiwa.JiwaServiceModel.Email;
using NUnit.Framework;
using ServiceStack;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiwaAPITests.EmailMessages
{
    public class EmailMessage : JiwaAPITest
    {
        #region "EmailMessages_Core"
        [Test]
        public async Task EmailMessages_CRUD()
        {
            // Create an email message
            EmailMessagePOSTRequest createReq = new EmailMessagePOSTRequest()
            {
                Reference = $"EMAIL-{RandomString(8)}",
                EmailFrom = "sender@example.com",
                EmailTo = "recipient@example.com",
                EmailSubject = $"Subject-{RandomString(8)}",
                EmailBody = "Initial body",
                BodyIsHTML = false,
                Status = JiwaFinancials.Jiwa.JiwaServiceModel.Email.EmailMessage.EmailStatuses.Entered
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Email.EmailMessage createRes = await Client.PostAsync(createReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(createRes.EmailMessageID, Is.Not.Null);
            Assert.That(createRes.EmailSubject, Is.EqualTo(createReq.EmailSubject));

            // Read the created email message
            EmailMessageGETRequest getReq = new EmailMessageGETRequest()
            {
                EmailMessageID = createRes.EmailMessageID
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Email.EmailMessage getRes = await Client.GetAsync(getReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(getRes.EmailMessageID, Is.EqualTo(createRes.EmailMessageID));

            // Update the email message
            EmailMessagePATCHRequest patchReq = new EmailMessagePATCHRequest()
            {
                EmailMessageID = createRes.EmailMessageID,
                EmailFrom = createRes.EmailFrom,
                EmailTo = createRes.EmailTo,
                EmailSubject = $"Updated-{RandomString(8)}",
                EmailBody = "Updated body",
                BodyIsHTML = createRes.BodyIsHTML,
                Status = JiwaFinancials.Jiwa.JiwaServiceModel.Email.EmailMessage.EmailStatuses.ReadyToSend,
                Reference = createRes.Reference
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Email.EmailMessage patchRes = await Client.PatchAsync(patchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(patchRes.EmailMessageID, Is.EqualTo(patchReq.EmailMessageID));
            Assert.That(patchRes.EmailSubject, Is.EqualTo(patchReq.EmailSubject));
            Assert.That(patchRes.Status, Is.EqualTo(patchReq.Status));

            // Verify the email message was updated
            JiwaFinancials.Jiwa.JiwaServiceModel.Email.EmailMessage verifyUpdatedRes = await Client.GetAsync(getReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(verifyUpdatedRes.EmailSubject, Is.EqualTo(patchReq.EmailSubject));
            Assert.That(verifyUpdatedRes.Status, Is.EqualTo(patchReq.Status));

            // Delete the email message
            EmailMessageDELETERequest deleteReq = new EmailMessageDELETERequest()
            {
                EmailMessageID = createRes.EmailMessageID
            };

            await Client.DeleteAsync<object>(deleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the email message was deleted
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(getReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "EmailMessages_Attachments"
        [Test]
        public async Task EmailMessages_Attachments_CRUD()
        {
            // Create an email message to append an attachment to
            EmailMessagePOSTRequest emailCreateReq = new EmailMessagePOSTRequest()
            {
                Reference = $"EMAIL-{RandomString(8)}",
                EmailFrom = "sender@example.com",
                EmailTo = "recipient@example.com",
                EmailSubject = $"Attachment Subject-{RandomString(8)}",
                EmailBody = "Attachment body",
                BodyIsHTML = false,
                Status = JiwaFinancials.Jiwa.JiwaServiceModel.Email.EmailMessage.EmailStatuses.Entered
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Email.EmailMessage emailCreateRes = await Client.PostAsync(emailCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(emailCreateRes.EmailMessageID, Is.Not.Null);

            // Create an attachment type to use for attachment creation
            EmailMessageAttachmentTypePOSTRequest attachmentTypeCreateReq = new EmailMessageAttachmentTypePOSTRequest()
            {
                Description = "Email Attachment Type " + RandomString(8),
                DefaultType = false
            };

            DocumentType attachmentTypeCreateRes = await Client.PostAsync(attachmentTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(attachmentTypeCreateRes.DocumentTypeID, Is.Not.Null);

            // Append an attachment to the email message
            EmailMessageAttachmentPOSTRequest attachmentCreateReq = new EmailMessageAttachmentPOSTRequest()
            {
                EmailMessageID = emailCreateRes.EmailMessageID,
                Description = "Email Attachment " + RandomString(8),
                PhysicalFileName = "EmailAttachment.txt",
                FileBinary = Encoding.UTF8.GetBytes("Email message attachment content"),
                DocumentType = new DocumentType() { DocumentTypeID = attachmentTypeCreateRes.DocumentTypeID }
            };

            Document attachmentCreateRes = await Client.PostAsync(attachmentCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(attachmentCreateRes.DocumentID, Is.Not.Null);
            Assert.That(attachmentCreateRes.Description, Is.EqualTo(attachmentCreateReq.Description));

            // Read all attachments for the email message and ensure the created attachment is returned
            EmailMessageAttachmentsGETManyRequest attachmentsGetManyReq = new EmailMessageAttachmentsGETManyRequest()
            {
                EmailMessageID = emailCreateRes.EmailMessageID
            };

            List<Document> attachmentsGetManyRes = await Client.GetAsync(attachmentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(attachmentsGetManyRes.Any(x => x.DocumentID == attachmentCreateRes.DocumentID), Is.True);

            // Read the created attachment using the AttachmentID
            EmailMessageAttachmentGETRequest attachmentGetReq = new EmailMessageAttachmentGETRequest()
            {
                EmailMessageID = emailCreateRes.EmailMessageID,
                AttachmentID = attachmentCreateRes.DocumentID
            };

            Document attachmentGetRes = await Client.GetAsync(attachmentGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(attachmentGetRes.DocumentID, Is.EqualTo(attachmentCreateRes.DocumentID));
            Assert.That(attachmentGetRes.Description, Is.EqualTo(attachmentCreateReq.Description));

            // Update the created attachment
            EmailMessageAttachmentPATCHRequest attachmentPatchReq = new EmailMessageAttachmentPATCHRequest()
            {
                EmailMessageID = emailCreateRes.EmailMessageID,
                AttachmentID = attachmentCreateRes.DocumentID,
                Description = "Updated Email Attachment " + RandomString(6)
            };

            Document attachmentPatchRes = await Client.PatchAsync(attachmentPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(attachmentPatchRes.DocumentID, Is.EqualTo(attachmentPatchReq.AttachmentID));
            Assert.That(attachmentPatchRes.DocumentID, Is.EqualTo(attachmentCreateRes.DocumentID));
            Assert.That(attachmentPatchRes.Description, Is.EqualTo(attachmentPatchReq.Description));

            // Delete the created attachment
            EmailMessageAttachmentDELETERequest attachmentDeleteReq = new EmailMessageAttachmentDELETERequest()
            {
                EmailMessageID = emailCreateRes.EmailMessageID,
                AttachmentID = attachmentCreateRes.DocumentID
            };

            await Client.DeleteAsync<object>(attachmentDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the attachment was deleted
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(attachmentGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Verify the deleted attachment is not returned in the attachment list
            attachmentsGetManyRes = await Client.GetAsync(attachmentsGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(attachmentsGetManyRes.Any(x => x.DocumentID == attachmentCreateRes.DocumentID), Is.False);

            // Delete the created email message
            EmailMessageDELETERequest emailDeleteReq = new EmailMessageDELETERequest()
            {
                EmailMessageID = emailCreateRes.EmailMessageID
            };

            await Client.DeleteAsync<object>(emailDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Delete the created attachment type
            EmailMessageAttachmentTypeDELETERequest attachmentTypeDeleteReq = new EmailMessageAttachmentTypeDELETERequest()
            {
                AttachmentTypeID = attachmentTypeCreateRes.DocumentTypeID
            };

            await Client.DeleteAsync<object>(attachmentTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion

        #region "EmailMessages_AttachmentTypes"
        [Test]
        public async Task EmailMessages_AttachmentTypes_CRUD()
        {
            // Create an attachment type
            EmailMessageAttachmentTypePOSTRequest attachmentTypeCreateReq = new EmailMessageAttachmentTypePOSTRequest()
            {
                Description = "Email Attachment Type " + RandomString(8),
                DefaultType = false
            };

            DocumentType attachmentTypeCreateRes = await Client.PostAsync(attachmentTypeCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(attachmentTypeCreateRes.DocumentTypeID, Is.Not.Null);
            Assert.That(attachmentTypeCreateRes.Description, Is.EqualTo(attachmentTypeCreateReq.Description));

            // Read all attachment types and ensure the created type is returned
            EmailMessageAttachmentTypesGETManyRequest attachmentTypesGetManyReq = new EmailMessageAttachmentTypesGETManyRequest();
            List<DocumentType> attachmentTypesGetManyRes = await Client.GetAsync(attachmentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(attachmentTypesGetManyRes.Any(x => x.DocumentTypeID == attachmentTypeCreateRes.DocumentTypeID), Is.True);

            // Read the created attachment type using the AttachmentTypeID
            EmailMessageAttachmentTypeGETRequest attachmentTypeGetReq = new EmailMessageAttachmentTypeGETRequest()
            {
                AttachmentTypeID = attachmentTypeCreateRes.DocumentTypeID
            };

            DocumentType attachmentTypeGetRes = await Client.GetAsync(attachmentTypeGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(attachmentTypeGetRes.DocumentTypeID, Is.EqualTo(attachmentTypeCreateRes.DocumentTypeID));
            Assert.That(attachmentTypeGetRes.Description, Is.EqualTo(attachmentTypeCreateReq.Description));

            // Update the created attachment type
            EmailMessageAttachmentTypePATCHRequest attachmentTypePatchReq = new EmailMessageAttachmentTypePATCHRequest()
            {
                AttachmentTypeID = attachmentTypeCreateRes.DocumentTypeID,
                Description = "Updated Email Attachment Type " + RandomString(6),
                DefaultType = false
            };

            DocumentType attachmentTypePatchRes = await Client.PatchAsync(attachmentTypePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(attachmentTypePatchRes.DocumentTypeID, Is.EqualTo(attachmentTypePatchReq.AttachmentTypeID));
            Assert.That(attachmentTypePatchRes.DocumentTypeID, Is.EqualTo(attachmentTypeCreateRes.DocumentTypeID));
            Assert.That(attachmentTypePatchRes.Description, Is.EqualTo(attachmentTypePatchReq.Description));

            // Delete the created attachment type
            EmailMessageAttachmentTypeDELETERequest attachmentTypeDeleteReq = new EmailMessageAttachmentTypeDELETERequest()
            {
                AttachmentTypeID = attachmentTypeCreateRes.DocumentTypeID
            };

            await Client.DeleteAsync<object>(attachmentTypeDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Verify the attachment type was deleted
            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                _ = await Client.GetAsync(attachmentTypeGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Verify the deleted attachment type is not returned in the attachment type list
            attachmentTypesGetManyRes = await Client.GetAsync(attachmentTypesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(attachmentTypesGetManyRes.Any(x => x.DocumentTypeID == attachmentTypeCreateRes.DocumentTypeID), Is.False);
        }
        #endregion
    }
}


