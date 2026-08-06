using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using JiwaFinancials.Jiwa.JiwaServiceModel.Inventory;
using JiwaFinancials.Jiwa.JiwaServiceModel.Notes;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Mail;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ServiceStack.Diagnostics.Events;

namespace JiwaAPITests.Debtors
{
    public class Account : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Debtor_CRUD()
        {
            // Create a debtor
            DebtorPOSTRequest accountCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor accountCreateRes = await Client.PostAsync(accountCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(accountCreateRes.AccountNo, Is.EqualTo(accountCreateReq.AccountNo));
            Assert.That(accountCreateRes.DebtorID, !Is.Null);

            // Read the created debtor using the DebtorID
            DebtorGETRequest accountGetReq = new DebtorGETRequest() { DebtorID = accountCreateRes.DebtorID };
            Debtor accountGetRes = await Client.GetAsync(accountGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(accountGetRes.AccountNo, Is.EqualTo(accountCreateReq.AccountNo));
            Assert.That(accountGetRes.Name, Is.EqualTo(accountCreateReq.Name));

            // Update the debtor
            DebtorPATCHRequest accountPatchReq = new DebtorPATCHRequest()
            {
                DebtorID = accountCreateRes.DebtorID,
                Name = "Updated Debtor Test",
                EmailAddress = "d@e.f"
            };
            Debtor accountPatchRes = await Client.PatchAsync(accountPatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(accountPatchRes.DebtorID, Is.EqualTo(accountPatchReq.DebtorID));
            Assert.That(accountPatchRes.Name, Is.EqualTo(accountPatchReq.Name));
            Assert.That(accountPatchRes.EmailAddress, Is.EqualTo(accountPatchReq.EmailAddress));

            // Remove the created debtor
            DebtorDELETERequest accountDeleteReq = new DebtorDELETERequest() { DebtorID = accountCreateRes.DebtorID };
            await Client.DeleteAsync(accountDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // ensure the deleted debtor is not there anymore            
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                Debtor getDeletedRes = await Client.GetAsync(accountGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Try to GET non-existent debtor to make sure we get a 404
            accountGetReq.DebtorID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                Debtor accountGetRes = await Client.GetAsync(accountGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "Contact Names"
        [Test]
        public async Task Debtor_ContactNames_CRUD()
        {
            // Create an account we can operate on
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.AccountNo, Is.EqualTo(debtorCreateReq.AccountNo));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

            // Add an contact name to the account
            DebtorContactNamePOSTRequest debtorContactNamePOSTReq = new DebtorContactNamePOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                Title = "Mr.",
                FirstName = "John",
                Surname = "Citizen"
            };
            DebtorContactName debtorContactNamePOSTRes = await Client.PostAsync(debtorContactNamePOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorContactNamePOSTRes.ContactNameID, !Is.Null);
            Assert.That(debtorContactNamePOSTRes.Title, Is.EqualTo(debtorContactNamePOSTReq.Title));
            Assert.That(debtorContactNamePOSTRes.FirstName, Is.EqualTo(debtorContactNamePOSTReq.FirstName));
            Assert.That(debtorContactNamePOSTRes.Surname, Is.EqualTo(debtorContactNamePOSTReq.Surname));

            // Check to see if the debtor contact name is present via a GET 
            DebtorContactNameGETRequest debtorContactNameGETReq = new DebtorContactNameGETRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                ContactNameID = debtorContactNamePOSTRes.ContactNameID
            };

            DebtorContactName debtorContactNameGETRes = await Client.GetAsync(debtorContactNameGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorContactNameGETRes.ContactNameID, Is.EqualTo(debtorContactNamePOSTRes.ContactNameID));
            Assert.That(debtorContactNameGETRes.Title, Is.EqualTo(debtorContactNamePOSTRes.Title));
            Assert.That(debtorContactNameGETRes.FirstName, Is.EqualTo(debtorContactNamePOSTRes.FirstName));
            Assert.That(debtorContactNameGETRes.Surname, Is.EqualTo(debtorContactNamePOSTRes.Surname));

            // Try also the GET Many - should return the single account we added
            DebtorContactNamesGETManyRequest debtorContactNameGETManyReq = new DebtorContactNamesGETManyRequest()
            {
                DebtorID = debtorCreateRes.DebtorID
            };

            List<DebtorContactName> debtorContactNameGETManyRes = await Client.GetAsync(debtorContactNameGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorContactNameGETManyRes.Count, Is.GreaterThan(0));

            // Try patching the account
            DebtorContactNamePATCHRequest debtorContactNamePATCHReq = new DebtorContactNamePATCHRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                ContactNameID = debtorContactNamePOSTRes.ContactNameID,
                EmailAddress = "g@h.i"
            };
            DebtorContactName debtorContactNamePATCHRes = await Client.PatchAsync(debtorContactNamePATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorContactNamePATCHRes.ContactNameID, Is.EqualTo(debtorContactNamePATCHReq.ContactNameID));
            Assert.That(debtorContactNamePATCHRes.EmailAddress, Is.EqualTo(debtorContactNamePATCHReq.EmailAddress));

            // Get the patched account and ensure it matches what we patched
            debtorContactNameGETRes = await Client.GetAsync(debtorContactNameGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorContactNameGETRes.EmailAddress, Is.EqualTo(debtorContactNamePATCHReq.EmailAddress));

            // Remove the debtor contact name we added
            DebtorContactNameDELETERequest debtorContactNameDELETEReq = new DebtorContactNameDELETERequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                ContactNameID = debtorContactNamePOSTRes.ContactNameID
            };
            await Client.DeleteAsync(debtorContactNameDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the debtor contact name is no longer present in the list of debtor contact names for the account
            debtorContactNameGETManyRes = await Client.GetAsync(debtorContactNameGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorContactNameGETManyRes.Count, Is.EqualTo(0));

            // Ensure explicitly requesting the debtor contact name 404's
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                debtorContactNameGETRes = await Client.GetAsync(debtorContactNameGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "Backorders"
        [Test]
        public async Task Debtor_Backorders_GETMany()
        {
            // Create a debtor we can operate on
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Backorders Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

            // Read the backorders for the new debtor
            DebtorBackordersGETRequest debtorBackordersGETReq = new DebtorBackordersGETRequest()
            {
                DebtorID = debtorCreateRes.DebtorID
            };

            List<DebtorBackOrder> debtorBackordersGETRes = await Client.GetAsync(debtorBackordersGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorBackordersGETRes, !Is.Null);
            Assert.That(debtorBackordersGETRes.Count, Is.EqualTo(0));

            // Create an item
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Item Test",
                DefaultPrice = 125.67M
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.PartNo, Is.EqualTo(itemCreateReq.PartNo));
            Assert.That(itemCreateRes.InventoryID, !Is.Null);

            // Create a sales order for debtor and add item (it will go on backorder)
            SalesOrderPOSTRequest salesOrderCreateReq = new SalesOrderPOSTRequest()
            {
                DebtorAccountNo = debtorCreateRes.AccountNo,
                InvoiceInitDate = DateTime.Today.Date,
                Lines = new List<JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrderLine()
                    {
                        InventoryID = itemCreateRes.InventoryID,
                        QuantityOrdered = 1
                    }
                }
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder salesOrderCreateRes = await Client.PostAsync(salesOrderCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(salesOrderCreateRes.InvoiceID, !Is.Null);

            // Process the sales order
            SalesOrderPROCESSRequest salesOrderProcessReq = new SalesOrderPROCESSRequest()
            {
                InvoiceID = salesOrderCreateRes.InvoiceID
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder salesOrderProcessRes = await Client.PostAsync(salesOrderProcessReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(salesOrderProcessRes.Status, Is.EqualTo(JiwaFinancials.Jiwa.JiwaServiceModel.SalesOrders.SalesOrder.SalesOrderStatuses.e_SalesOrderProcessed));

            // Read the backorders for the new debtor
            debtorBackordersGETReq = new DebtorBackordersGETRequest()
            {
                DebtorID = debtorCreateRes.DebtorID
            };

            debtorBackordersGETRes = await Client.GetAsync(debtorBackordersGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorBackordersGETRes, !Is.Null);
            Assert.That(debtorBackordersGETRes.Count, Is.EqualTo(1));
        }
        #endregion

        #region "Contact Name Tag Memberships"
        [Test]
        public async Task Debtor_ContactNames_TagMembership_CRUD()
        {
            // Create an account we can operate on
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Contact Name Tag Membership Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

            // Add a contact name to the account
            DebtorContactNamePOSTRequest debtorContactNamePOSTReq = new DebtorContactNamePOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                Title = "Mr.",
                FirstName = "John",
                Surname = "Citizen"
            };

            DebtorContactName debtorContactNamePOSTRes = await Client.PostAsync(debtorContactNamePOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorContactNamePOSTRes.ContactNameID, !Is.Null);

            // Create a tag
            DebtorContactNameTagPOSTRequest tagCreateReq = new DebtorContactNameTagPOSTRequest()
            {
                Text = RandomString(5)
            };

            DebtorContactNameTag tagCreateRes = await Client.PostAsync(tagCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(tagCreateRes.Text, Is.EqualTo(tagCreateReq.Text));
            Assert.That(tagCreateRes.RecID, !Is.Null);

            // Add a tag membership to the contact name
            DebtorContactNameTagMembershipPOSTRequest tagMembershipPOSTReq = new DebtorContactNameTagMembershipPOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                ContactNameID = debtorContactNamePOSTRes.ContactNameID,
                TagID = tagCreateRes.RecID
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag tagMembershipPOSTRes = await Client.PostAsync(tagMembershipPOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(tagMembershipPOSTRes.RecID, Is.EqualTo(tagCreateRes.RecID));
            Assert.That(tagMembershipPOSTRes.Text, Is.EqualTo(tagCreateRes.Text));

            // Get the tag memberships for the contact name
            DebtorContactNameTagMembershipGETManyRequest tagMembershipGETManyReq = new DebtorContactNameTagMembershipGETManyRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                ContactNameID = debtorContactNamePOSTRes.ContactNameID
            };

            List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag> tagMembershipGETManyRes = await Client.GetAsync(tagMembershipGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagMembershipGETManyRes, !Is.Null);
            Assert.That(tagMembershipGETManyRes.Count, Is.EqualTo(1));
            Assert.That(tagMembershipGETManyRes[0].RecID, Is.EqualTo(tagCreateRes.RecID));

            // Replace all the tag memberships with an empty list
            DebtorContactNameTagMembershipPUTRequest tagMembershipPUTReq = new DebtorContactNameTagMembershipPUTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                ContactNameID = debtorContactNamePOSTRes.ContactNameID,
                Tags = new List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag>()
            };

            List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag> tagMembershipPUTRes = await Client.PutAsync(tagMembershipPUTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagMembershipPUTRes, !Is.Null);
            Assert.That(tagMembershipPUTRes.Count, Is.EqualTo(0));

            DebtorContactNameTag firstTagCreateRes = tagCreateRes.CreateCopy();

            // Create a second tag so we can add it with a PUT later
            tagCreateReq = new DebtorContactNameTagPOSTRequest()
            {
                Text = RandomString(5)
            };

            tagCreateRes = await Client.PostAsync(tagCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(tagCreateRes.Text, Is.EqualTo(tagCreateReq.Text));
            Assert.That(tagCreateRes.RecID, !Is.Null);

            // Replace all the tag memberships with the two tags we created
            tagMembershipPUTReq = new DebtorContactNameTagMembershipPUTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                ContactNameID = debtorContactNamePOSTRes.ContactNameID,
                Tags = new List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag()
                    {
                        RecID = firstTagCreateRes.RecID
                    },
                    new JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag()
                    {
                        RecID = tagCreateRes.RecID
                    }
                }
            };

            tagMembershipPUTRes = await Client.PutAsync(tagMembershipPUTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagMembershipPUTRes, !Is.Null);
            Assert.That(tagMembershipPUTRes.Count, Is.EqualTo(2));
            Assert.That(tagMembershipPUTRes[0].RecID, Is.EqualTo(firstTagCreateRes.RecID));
            Assert.That(tagMembershipPUTRes[1].RecID, Is.EqualTo(tagCreateRes.RecID));

            // Remove a tag membership
            DebtorContactNameTagMembershipDELETERequest tagMembershipDELETEReq = new DebtorContactNameTagMembershipDELETERequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                ContactNameID = debtorContactNamePOSTRes.ContactNameID,
                TagID = tagCreateRes.RecID
            };

            await Client.DeleteAsync(tagMembershipDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted tag membership is not there anymore
            tagMembershipGETManyRes = await Client.GetAsync(tagMembershipGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagMembershipGETManyRes, !Is.Null);
            Assert.That(tagMembershipGETManyRes.Count, Is.EqualTo(1));
            Assert.That(tagMembershipGETManyRes[0].RecID, Is.EqualTo(firstTagCreateRes.RecID));
        }
        #endregion

        #region "Contact Name Tags"
        [Test]
        public async Task Debtor_ContactNamesTag_CRUD()
        {
            // Create a tag
            DebtorContactNameTagPOSTRequest tagCreateReq = new DebtorContactNameTagPOSTRequest()
            {
                Text = RandomString(5)
            };

            DebtorContactNameTag tagCreateRes = await Client.PostAsync(tagCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(tagCreateRes.Text, Is.EqualTo(tagCreateReq.Text));
            Assert.That(tagCreateRes.RecID, !Is.Null);

            // Read all tags and ensure the created tag is returned
            DebtorContactNameTagGETManyRequest tagGETManyReq = new DebtorContactNameTagGETManyRequest();
            List<DebtorContactNameTag> tagGETManyRes = await Client.GetAsync(tagGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagGETManyRes.Any(x => x.RecID == tagCreateRes.RecID), Is.True);

            // Read the created tag using the RecID
            DebtorContactNameTagGETRequest tagGETReq = new DebtorContactNameTagGETRequest() { RecID = tagCreateRes.RecID };
            DebtorContactNameTag tagGETRes = await Client.GetAsync(tagGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagGETRes.Text, Is.EqualTo(tagCreateReq.Text));

            // Update the tag
            DebtorContactNameTagPATCHRequest tagPATCHReq = new DebtorContactNameTagPATCHRequest()
            {
                RecID = tagCreateRes.RecID,
                Text = "Updated Tag " + RandomString(6)
            };

            DebtorContactNameTag tagPATCHRes = await Client.PatchAsync(tagPATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagPATCHRes.RecID, Is.EqualTo(tagPATCHReq.RecID));
            Assert.That(tagPATCHRes.Text, Is.EqualTo(tagPATCHReq.Text));

            // Remove the created tag
            DebtorContactNameTagDELETERequest tagDELETEReq = new DebtorContactNameTagDELETERequest() { RecID = tagCreateRes.RecID };
            await Client.DeleteAsync(tagDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the deleted tag is not there anymore
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                DebtorContactNameTag getDeletedRes = await Client.GetAsync(tagGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            // Read all tags and ensure the deleted tag is no longer returned
            tagGETManyRes = await Client.GetAsync(tagGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagGETManyRes.Any(x => x.RecID == tagCreateRes.RecID), Is.False);

            // Try to GET non-existent tag to make sure we get a 404
            tagGETReq.RecID = Guid.NewGuid().ToString();
            ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                DebtorContactNameTag getRes = await Client.GetAsync(tagGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "Debtor Part Numbers"
        [Test]
        public async Task Debtor_DebtorPartNumbers_CRUD()
        {
            // Create an account we can operate on
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Part Number Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

            // Create an inventory item we can link to the debtor part number
            InventoryPOSTRequest itemCreateReq = new InventoryPOSTRequest()
            {
                PartNo = RandomString(5),
                Description = "Debtor Part Number Item Test",
                DefaultPrice = 125.67M
            };

            InventoryItem itemCreateRes = await Client.PostAsync(itemCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(itemCreateRes.InventoryID, !Is.Null);

            // Add a debtor part number to the account
            DebtorPartNumberPOSTRequest debtorPartNumberPOSTReq = new DebtorPartNumberPOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                InventoryID = itemCreateRes.InventoryID,
                DebtorPartNo = RandomString(8),
                DebtorBarcode = RandomString(10)
            };

            DebtorPartNumber debtorPartNumberPOSTRes = await Client.PostAsync(debtorPartNumberPOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorPartNumberPOSTRes.PartNumberID, !Is.Null);
            Assert.That(debtorPartNumberPOSTRes.InventoryID, Is.EqualTo(debtorPartNumberPOSTReq.InventoryID));
            Assert.That(debtorPartNumberPOSTRes.DebtorPartNo, Is.EqualTo(debtorPartNumberPOSTReq.DebtorPartNo));
            Assert.That(debtorPartNumberPOSTRes.DebtorBarcode, Is.EqualTo(debtorPartNumberPOSTReq.DebtorBarcode));

            // Get the debtor part number
            DebtorPartNumberGETRequest debtorPartNumberGETReq = new DebtorPartNumberGETRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                PartNumberID = debtorPartNumberPOSTRes.PartNumberID
            };

            DebtorPartNumber debtorPartNumberGETRes = await Client.GetAsync(debtorPartNumberGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorPartNumberGETRes.PartNumberID, Is.EqualTo(debtorPartNumberPOSTRes.PartNumberID));
            Assert.That(debtorPartNumberGETRes.InventoryID, Is.EqualTo(itemCreateRes.InventoryID));
            Assert.That(debtorPartNumberGETRes.PartNo, Is.EqualTo(itemCreateRes.PartNo));
            Assert.That(debtorPartNumberGETRes.DebtorPartNo, Is.EqualTo(debtorPartNumberPOSTReq.DebtorPartNo));
            Assert.That(debtorPartNumberGETRes.DebtorBarcode, Is.EqualTo(debtorPartNumberPOSTReq.DebtorBarcode));

            // Get all debtor part numbers for the account
            DebtorPartNumbersGETManyRequest debtorPartNumbersGETManyReq = new DebtorPartNumbersGETManyRequest()
            {
                DebtorID = debtorCreateRes.DebtorID
            };

            List<DebtorPartNumber> debtorPartNumbersGETManyRes = await Client.GetAsync(debtorPartNumbersGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorPartNumbersGETManyRes.Count, Is.EqualTo(1));
            Assert.That(debtorPartNumbersGETManyRes[0].PartNumberID, Is.EqualTo(debtorPartNumberPOSTRes.PartNumberID));

            // Update the debtor part number
            DebtorPartNumberPATCHRequest debtorPartNumberPATCHReq = new DebtorPartNumberPATCHRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                PartNumberID = debtorPartNumberPOSTRes.PartNumberID,
                DebtorPartNo = RandomString(8),
                DebtorBarcode = RandomString(10)
            };

            DebtorPartNumber debtorPartNumberPATCHRes = await Client.PatchAsync(debtorPartNumberPATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorPartNumberPATCHRes.PartNumberID, Is.EqualTo(debtorPartNumberPATCHReq.PartNumberID));
            Assert.That(debtorPartNumberPATCHRes.DebtorPartNo, Is.EqualTo(debtorPartNumberPATCHReq.DebtorPartNo));
            Assert.That(debtorPartNumberPATCHRes.DebtorBarcode, Is.EqualTo(debtorPartNumberPATCHReq.DebtorBarcode));

            // Get the patched debtor part number and ensure it matches what we patched
            debtorPartNumberGETRes = await Client.GetAsync(debtorPartNumberGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorPartNumberGETRes.DebtorPartNo, Is.EqualTo(debtorPartNumberPATCHReq.DebtorPartNo));
            Assert.That(debtorPartNumberGETRes.DebtorBarcode, Is.EqualTo(debtorPartNumberPATCHReq.DebtorBarcode));

            // Remove the debtor part number
            DebtorPartNumberDELETERequest debtorPartNumberDELETEReq = new DebtorPartNumberDELETERequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                PartNumberID = debtorPartNumberPOSTRes.PartNumberID
            };

            await Client.DeleteAsync(debtorPartNumberDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the debtor part number is no longer present in the list for the account
            debtorPartNumbersGETManyRes = await Client.GetAsync(debtorPartNumbersGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorPartNumbersGETManyRes.Count, Is.EqualTo(0));

            // Ensure explicitly requesting the debtor part number 404's
            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                debtorPartNumberGETRes = await Client.GetAsync(debtorPartNumberGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "Delivery Addresses"
        [Test]
        public async Task Debtor_DeliveryAddresses_CRUD()
        {
            // Create an account we can operate on
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.AccountNo, Is.EqualTo(debtorCreateReq.AccountNo));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

            // Add an delivery address to the account
            DebtorDeliveryAddressPOSTRequest debtorDeliveryAddressPOSTReq = new DebtorDeliveryAddressPOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressName = "Name",
                DeliveryAddressCode = "Code",
                Address1 = "Address1",
                Address2 = "Address2",
                Address3 = "Address3",
                Address4 = "Address4",
                Postcode = "Postcode",
                Country = "Country",
                Notes = "Notes",
                CourierDetails = "CourierDetails",
                EDIStoreLocationCode = "EDIStoreLocationCode",
                EmailAddress = "EmailAddress",
                Phone = "Phone",
            };
            DebtorDeliveryAddress debtorDeliveryAddressPOSTRes = await Client.PostAsync(debtorDeliveryAddressPOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorDeliveryAddressPOSTRes.DeliveryAddressID, !Is.Null);
            Assert.That(debtorDeliveryAddressPOSTRes.DeliveryAddressName, Is.EqualTo(debtorDeliveryAddressPOSTReq.DeliveryAddressName));
            Assert.That(debtorDeliveryAddressPOSTRes.DeliveryAddressCode, Is.EqualTo(debtorDeliveryAddressPOSTReq.DeliveryAddressCode));
            Assert.That(debtorDeliveryAddressPOSTRes.Address1, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address1));
            Assert.That(debtorDeliveryAddressPOSTRes.Address2, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address2));
            Assert.That(debtorDeliveryAddressPOSTRes.Address3, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address3));
            Assert.That(debtorDeliveryAddressPOSTRes.Address4, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address4));
            Assert.That(debtorDeliveryAddressPOSTRes.Postcode, Is.EqualTo(debtorDeliveryAddressPOSTReq.Postcode));
            Assert.That(debtorDeliveryAddressPOSTRes.Country, Is.EqualTo(debtorDeliveryAddressPOSTReq.Country));
            Assert.That(debtorDeliveryAddressPOSTRes.Notes, Is.EqualTo(debtorDeliveryAddressPOSTReq.Notes));
            Assert.That(debtorDeliveryAddressPOSTRes.CourierDetails, Is.EqualTo(debtorDeliveryAddressPOSTReq.CourierDetails));
            Assert.That(debtorDeliveryAddressPOSTRes.EDIStoreLocationCode, Is.EqualTo(debtorDeliveryAddressPOSTReq.EDIStoreLocationCode));
            Assert.That(debtorDeliveryAddressPOSTRes.EmailAddress, Is.EqualTo(debtorDeliveryAddressPOSTReq.EmailAddress));
            Assert.That(debtorDeliveryAddressPOSTRes.Phone, Is.EqualTo(debtorDeliveryAddressPOSTReq.Phone));

            // Check to see if the debtor delivery address is present via a GET 
            DebtorDeliveryAddressGETRequest debtorDeliveryAddressGETReq = new DebtorDeliveryAddressGETRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = debtorDeliveryAddressPOSTRes.DeliveryAddressID
            };

            DebtorDeliveryAddress debtorDeliveryAddressGETRes = await Client.GetAsync(debtorDeliveryAddressGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorDeliveryAddressGETRes.DeliveryAddressID, Is.EqualTo(debtorDeliveryAddressPOSTRes.DeliveryAddressID));
            Assert.That(debtorDeliveryAddressGETRes.DeliveryAddressName, Is.EqualTo(debtorDeliveryAddressPOSTReq.DeliveryAddressName));
            Assert.That(debtorDeliveryAddressGETRes.DeliveryAddressCode, Is.EqualTo(debtorDeliveryAddressPOSTReq.DeliveryAddressCode));
            Assert.That(debtorDeliveryAddressGETRes.Address1, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address1));
            Assert.That(debtorDeliveryAddressGETRes.Address2, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address2));
            Assert.That(debtorDeliveryAddressGETRes.Address3, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address3));
            Assert.That(debtorDeliveryAddressGETRes.Address4, Is.EqualTo(debtorDeliveryAddressPOSTReq.Address4));
            Assert.That(debtorDeliveryAddressGETRes.Postcode, Is.EqualTo(debtorDeliveryAddressPOSTReq.Postcode));
            Assert.That(debtorDeliveryAddressGETRes.Country, Is.EqualTo(debtorDeliveryAddressPOSTReq.Country));
            Assert.That(debtorDeliveryAddressGETRes.Notes, Is.EqualTo(debtorDeliveryAddressPOSTReq.Notes));
            Assert.That(debtorDeliveryAddressGETRes.CourierDetails, Is.EqualTo(debtorDeliveryAddressPOSTReq.CourierDetails));
            Assert.That(debtorDeliveryAddressGETRes.EDIStoreLocationCode, Is.EqualTo(debtorDeliveryAddressPOSTReq.EDIStoreLocationCode));
            Assert.That(debtorDeliveryAddressGETRes.EmailAddress, Is.EqualTo(debtorDeliveryAddressPOSTReq.EmailAddress));
            Assert.That(debtorDeliveryAddressGETRes.Phone, Is.EqualTo(debtorDeliveryAddressPOSTReq.Phone));

            // Try also the GET Many - should return the single delivery address we added
            DebtorDeliveryAddressesGETManyRequest debtorDeliveryAddressGETManyReq = new DebtorDeliveryAddressesGETManyRequest()
            {
                DebtorID = debtorCreateRes.DebtorID
            };

            List<DebtorDeliveryAddress> debtorDeliveryAddressGETManyRes = await Client.GetAsync(debtorDeliveryAddressGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorDeliveryAddressGETManyRes.Count, Is.GreaterThan(0));

            // Try patching the delivery address
            DebtorDeliveryAddressPATCHRequest debtorDeliveryAddressPATCHReq = new DebtorDeliveryAddressPATCHRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = debtorDeliveryAddressPOSTRes.DeliveryAddressID,
                DeliveryAddressName = "Name Updated",
                DeliveryAddressCode = "Code Updated",
                Address1 = "Address1 Updated",
                Address2 = "Address2 Updated",
                Address3 = "Address3 Updated",
                Address4 = "Address4 Updated",
                Postcode = "Postcode2",
                Country = "Country Updated",
                Notes = "Notes Updated",
                CourierDetails = "CourierDetails Updated",
                EDIStoreLocationCode = "EDIStoreLocationCode Updated",
                EmailAddress = "EmailAddress Updated",
                Phone = "Phone Updated",
            };
            DebtorDeliveryAddress debtorDeliveryAddressPATCHRes = await Client.PatchAsync(debtorDeliveryAddressPATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorDeliveryAddressPATCHRes.DeliveryAddressID, Is.EqualTo(debtorDeliveryAddressPATCHReq.DeliveryAddressID));
            Assert.That(debtorDeliveryAddressPATCHRes.EmailAddress, Is.EqualTo(debtorDeliveryAddressPATCHReq.EmailAddress));

            // Get the patched account and ensure it matches what we patched
            debtorDeliveryAddressGETRes = await Client.GetAsync(debtorDeliveryAddressGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorDeliveryAddressGETRes.EmailAddress, Is.EqualTo(debtorDeliveryAddressPATCHReq.EmailAddress));

            // Add a second delivery address to the account
            DebtorDeliveryAddressPOSTRequest newDebtorDeliveryAddressPOSTReq = new DebtorDeliveryAddressPOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressName = "New Name",
                DeliveryAddressCode = "New Code",
                Address1 = "New Address1",
                Address2 = "New Address2",
                Address3 = "New Address3",
                Address4 = "New Address4",
                Postcode = "Postcode",
                Country = "New Country",
                Notes = "New Notes",
                CourierDetails = "New CourierDetails",
                EDIStoreLocationCode = "New EDIStoreLocationCode",
                EmailAddress = "New EmailAddress",
                Phone = "New Phone",
            };
            DebtorDeliveryAddress newDebtorDeliveryAddressPOSTRes = await Client.PostAsync(newDebtorDeliveryAddressPOSTReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(newDebtorDeliveryAddressPOSTRes.DeliveryAddressID, !Is.Null);
            Assert.That(newDebtorDeliveryAddressPOSTRes.DeliveryAddressName, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.DeliveryAddressName));
            Assert.That(newDebtorDeliveryAddressPOSTRes.DeliveryAddressCode, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.DeliveryAddressCode));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Address1, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Address1));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Address2, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Address2));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Address3, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Address3));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Address4, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Address4));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Postcode, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Postcode));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Country, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Country));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Notes, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Notes));
            Assert.That(newDebtorDeliveryAddressPOSTRes.CourierDetails, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.CourierDetails));
            Assert.That(newDebtorDeliveryAddressPOSTRes.EDIStoreLocationCode, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.EDIStoreLocationCode));
            Assert.That(newDebtorDeliveryAddressPOSTRes.EmailAddress, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.EmailAddress));
            Assert.That(newDebtorDeliveryAddressPOSTRes.Phone, Is.EqualTo(newDebtorDeliveryAddressPOSTReq.Phone));

            // Make the new delivery address the default delivery address for the account
            debtorDeliveryAddressPATCHReq = new DebtorDeliveryAddressPATCHRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = newDebtorDeliveryAddressPOSTRes.DeliveryAddressID,
                IsDefault = true
            };
            debtorDeliveryAddressPATCHRes = await Client.PatchAsync(debtorDeliveryAddressPATCHReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorDeliveryAddressPATCHRes.DeliveryAddressID, Is.EqualTo(debtorDeliveryAddressPATCHReq.DeliveryAddressID));
            Assert.That(debtorDeliveryAddressPATCHRes.IsDefault, Is.EqualTo(debtorDeliveryAddressPATCHReq.IsDefault));

            // Get the patched account and ensure it matches what we patched
            debtorDeliveryAddressGETReq = new DebtorDeliveryAddressGETRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = newDebtorDeliveryAddressPOSTRes.DeliveryAddressID
            };
            debtorDeliveryAddressGETRes = await Client.GetAsync(debtorDeliveryAddressGETReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorDeliveryAddressGETRes.IsDefault, Is.EqualTo(debtorDeliveryAddressPATCHReq.IsDefault));

            // Remove the original debtor delivery address we added
            DebtorDeliveryAddressDELETERequest debtorDeliveryAddressDELETEReq = new DebtorDeliveryAddressDELETERequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = debtorDeliveryAddressPOSTRes.DeliveryAddressID
            };
            await Client.DeleteAsync(debtorDeliveryAddressDELETEReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Ensure the debtor delivery address is no longer present in the list of debtor delivery addresss for the account
            debtorDeliveryAddressGETManyRes = await Client.GetAsync(debtorDeliveryAddressGETManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(debtorDeliveryAddressGETManyRes.Count, Is.EqualTo(1));

            // Ensure explicitly requesting the debtor delivery address 404's
            debtorDeliveryAddressGETReq = new DebtorDeliveryAddressGETRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                DeliveryAddressID = debtorDeliveryAddressPOSTRes.DeliveryAddressID
            };

            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                debtorDeliveryAddressGETRes = await Client.GetAsync(debtorDeliveryAddressGETReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "Tag Memberships"
        [Test]
        public async Task Debtor_TagMembership_CRUD()
        {
            // Create a debtor we can operate on
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Tag Membership Test"                
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.AccountNo, Is.EqualTo(debtorCreateReq.AccountNo));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

            // Create a tag
            DebtorTagPOSTRequest tagCreateReq = new DebtorTagPOSTRequest()
            {
                Text = RandomString(5)
            };

            DebtorTag tagCreateRes = await Client.PostAsync(tagCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(tagCreateRes.Text, Is.EqualTo(tagCreateReq.Text));
            Assert.That(tagCreateRes.RecID, !Is.Null);

            // Add a tag membership to the debtor
            DebtorTagMembershipPOSTRequest tagMembershipPOSTRequest = new DebtorTagMembershipPOSTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                TagID = tagCreateRes.RecID
            };

            JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag tagMembershipCreateRes = await Client.PostAsync(tagMembershipPOSTRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(tagMembershipCreateRes.Text, Is.EqualTo(tagCreateReq.Text));
            Assert.That(tagMembershipCreateRes.RecID, Is.EqualTo(tagCreateRes.RecID));

            // Get the tag memberships for the debtor
            DebtorTagMembershipGETManyRequest tagMembershipGetManyReq = new DebtorTagMembershipGETManyRequest()
            {
                DebtorID = debtorCreateRes.DebtorID
            };

            List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag> tagMembershipGetManyRes = await Client.GetAsync(tagMembershipGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagMembershipGetManyRes, !Is.Null);
            Assert.That(tagMembershipGetManyRes.Count, Is.EqualTo(1));
            Assert.That(tagMembershipGetManyRes[0].RecID, Is.EqualTo(tagCreateRes.RecID));

            // Replace all the tag memberships with an empty list
            DebtorTagMembershipPUTRequest tagMembershipPutReq = new DebtorTagMembershipPUTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                Tags = new List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag>()
            };

            List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag> tagMembershipPutRes = await Client.PutAsync(tagMembershipPutReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagMembershipPutRes, !Is.Null);
            Assert.That(tagMembershipPutRes.Count, Is.EqualTo(0));

            DebtorTag firstTagCreateRes = tagCreateRes.CreateCopy();

            // Create a second tag so we can add it with a PUT later
            tagCreateReq = new DebtorTagPOSTRequest()
            {
                Text = RandomString(5)
            };

            tagCreateRes = await Client.PostAsync(tagCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(tagCreateRes.Text, Is.EqualTo(tagCreateReq.Text));
            Assert.That(tagCreateRes.RecID, !Is.Null);

            // Replace all the tag memberships with the two tags we created
            tagMembershipPutReq = new DebtorTagMembershipPUTRequest()
            {
                DebtorID = debtorCreateRes.DebtorID,
                Tags = new List<JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag>()
                {
                    new JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag()
                    {
                        RecID = firstTagCreateRes.RecID
                    },
                    new JiwaFinancials.Jiwa.JiwaServiceModel.Tags.Tag()
                    {
                        RecID = tagCreateRes.RecID
                    }
                }
            };

            tagMembershipPutRes = await Client.PutAsync(tagMembershipPutReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagMembershipPutRes, !Is.Null);
            Assert.That(tagMembershipPutRes.Count, Is.EqualTo(2));
            Assert.That(tagMembershipPutRes[0].RecID, Is.EqualTo(firstTagCreateRes.RecID));
            Assert.That(tagMembershipPutRes[1].RecID, Is.EqualTo(tagCreateRes.RecID));

            // Remove a tag membership
            DebtorTagMembershipDELETERequest tagMembershipDeleteReq = new DebtorTagMembershipDELETERequest() { DebtorID = debtorCreateRes.DebtorID, TagID = tagCreateRes.RecID };
            await Client.DeleteAsync(tagMembershipDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // ensure the deleted tag membership is not there anymore            
            // Get the tag memberships for the debtor
            tagMembershipGetManyReq = new DebtorTagMembershipGETManyRequest()
            {
                DebtorID = debtorCreateRes.DebtorID
            };

            tagMembershipGetManyRes = await Client.GetAsync(tagMembershipGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(tagMembershipGetManyRes, !Is.Null);
            Assert.That(tagMembershipGetManyRes.Count, Is.EqualTo(1));
            Assert.That(tagMembershipGetManyRes[0].RecID, Is.EqualTo(firstTagCreateRes.RecID));

            // Remove the debtor account we created
            DebtorDELETERequest accountDeleteReq = new DebtorDELETERequest() { DebtorID = debtorCreateRes.DebtorID };
            await Client.DeleteAsync(accountDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));
        }
        #endregion

        #region "Queries"
        [Test]
        public async Task DB_MainQuery()
        {
            DB_MainQuery DB_MainQueryRequest = new DB_MainQuery();
            ServiceStack.QueryResponse<DB_Main> DB_MainQueryResponse;

            //Read all debtor accounts            
            DB_MainQueryResponse = await Client.GetAsync(DB_MainQueryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Let's assume we expect to get at least one debtor account back - demo data has many debtor accounts.
            Assert.That(DB_MainQueryResponse.Results.Count > 0);

            // Try with an invalid APIKey on to make sure we get a 401
            // Need to use a new client for this, as existing session Id's cookied will bind us to the session from
            // previous requests
            using (ServiceStack.JsonApiClient tempClient = new ServiceStack.JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                var ex = Assert.Throws<ServiceStack.WebServiceException>(() => DB_MainQueryResponse = tempClient.Get(DB_MainQueryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }
        }

        [Test]
        public async Task v_Jiwa_Debtor_ListQuery()
        {
            // get first 10 parts
            v_Jiwa_Debtor_ListQuery  v_Jiwa_Inventory_Item_ListQueryRequest = new v_Jiwa_Debtor_ListQuery()
            {
                Take = 10,
                OrderBy = "AccountNo"
            };
            ServiceStack.QueryResponse<v_Jiwa_Debtor_List> v_Jiwa_Debtor_ListQueryResponse;

            v_Jiwa_Debtor_ListQueryResponse = await Client.GetAsync(v_Jiwa_Inventory_Item_ListQueryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Ensure we got only the 10 we asked for
            Assert.That(v_Jiwa_Debtor_ListQueryResponse.Results.Count == 10);

            // Try with an invalid APIKey on to make sure we get a 401
            // Need to use a new client for this, as existing session Id's cookied will bind us to the session from
            // previous requests
            using (ServiceStack.JsonApiClient tempClient = new ServiceStack.JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                var ex = Assert.Throws<ServiceStack.WebServiceException>(() => v_Jiwa_Debtor_ListQueryResponse = tempClient.Get(v_Jiwa_Inventory_Item_ListQueryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }
        }

        #endregion
    }
}


