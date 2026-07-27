using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.CRBatchTX;
using JiwaFinancials.Jiwa.JiwaServiceModel.Creditors;
using JiwaFinancials.Jiwa.JiwaServiceModel.CustomFields;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables.Or;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiwaAPITests.CreditorPurchases
{
    public class CreditorPurchase : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task CreditorPurchase_CRUD()
        {
            // Create a creditor to use for a creditor purchase
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Creditor Test"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);
            Assert.That(creditorCreateRes.AccountNo, Is.EqualTo(creditorCreateReq.AccountNo));

            // Create a creditor purchase and add a line for the creditor created above
            CreditorPurchasePOSTRequest purchaseCreateReq = new CreditorPurchasePOSTRequest()
            {
                Description = "Creditor Purchase Test",
                BatchDate = DateTime.Today,
                TransLines = new List<CRBatchTranLine>()
                {
                    new CRBatchTranLine()
                    {
                        RemitNo = RandomString(8),
                        CreditorAccountNo = creditorCreateReq.AccountNo,
                        HomeTransAmount = 123.45M,
                        SupplierTransAmount = 123.45M,
                        ReceiptDate = DateTime.Today,
                        DueDate = DateTime.Today.AddDays(30)
                    }
                }
            };

            CreditorBatchTrans purchaseCreateRes = await Client.PostAsync(purchaseCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseCreateRes.BatchID, Is.Not.Null);
            Assert.That(purchaseCreateRes.TransLines.Count, Is.EqualTo(1));
            Assert.That(purchaseCreateRes.TransLines[0].CreditorAccountNo, Is.EqualTo(purchaseCreateReq.TransLines[0].CreditorAccountNo));

            // Read the created creditor purchase using the CreditorPurchaseID
            CreditorPurchaseGETRequest purchaseGetReq = new CreditorPurchaseGETRequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID
            };

            CreditorBatchTrans purchaseGetRes = await Client.GetAsync(purchaseGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchaseGetRes.BatchID, Is.EqualTo(purchaseCreateRes.BatchID));
            Assert.That(purchaseGetRes.Description, Is.EqualTo(purchaseCreateReq.Description));

            // Update the creditor purchase
            CreditorPurchasePATCHRequest purchasePatchReq = new CreditorPurchasePATCHRequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID,
                Description = "Updated Creditor Purchase Test"
            };

            CreditorBatchTrans purchasePatchRes = await Client.PatchAsync(purchasePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchasePatchRes.Description, Is.EqualTo(purchasePatchReq.Description));

            // Remove the created creditor purchase
            CreditorPurchaseDELETERequest purchaseDeleteReq = new CreditorPurchaseDELETERequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID
            };

            await Client.DeleteAsync(purchaseDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            WebServiceException ex = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                CreditorBatchTrans deletedPurchaseRes = await Client.GetAsync(purchaseGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));

            //Create another creditor purchase and add a line for the creditor created above
            CreditorPurchasePOSTRequest purchaseForActivateReq = new CreditorPurchasePOSTRequest()
            {
                Description = "Creditor Purchase Activate Test",
                BatchDate = DateTime.Today,
                TransLines = new List<CRBatchTranLine>()
                {
                    new CRBatchTranLine()
                    {
                        RemitNo = RandomString(8),
                        CreditorAccountNo = creditorCreateReq.AccountNo,
                        HomeTransAmount = 456.78M,
                        SupplierTransAmount = 456.78M,
                        ReceiptDate = DateTime.Today,
                        DueDate = DateTime.Today.AddDays(7)
                    }
                }
            };

            CreditorBatchTrans purchaseForActivateRes = await Client.PostAsync(purchaseForActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseForActivateRes.BatchID, Is.Not.Null);

            //Activate the creditor purchase 
            CreditorPurchaseACTIVATERequest purchaseActivateReq = new CreditorPurchaseACTIVATERequest()
            {
                CreditorPurchaseID = purchaseForActivateRes.BatchID
            };

            CreditorBatchTrans purchaseActivateRes = await Client.PostAsync(purchaseActivateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

            if (purchaseActivateRes != null)
            {
                Assert.That(purchaseActivateRes.BatchStatus, Is.EqualTo(BatchStatusType.Activated));
            }
        }
        #endregion

        #region "Lines"
        [Test]
        public async Task CreditorPurchase_Lines_CRUD()
        {
            // Create a creditor to use for a creditor purchase
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Creditor Test"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);
            Assert.That(creditorCreateRes.AccountNo, Is.EqualTo(creditorCreateReq.AccountNo));

            // Create a creditor purchase and add a line for the creditor created above
            CreditorPurchasePOSTRequest purchaseCreateReq = new CreditorPurchasePOSTRequest()
            {
                Description = "Creditor Purchase Lines Test",
                BatchDate = DateTime.Today,
                TransLines = new List<CRBatchTranLine>()
                {
                    new CRBatchTranLine()
                    {
                        RemitNo = RandomString(8),
                        CreditorAccountNo = creditorCreateReq.AccountNo,
                        HomeTransAmount = 123.45M,
                        SupplierTransAmount = 123.45M,
                        ReceiptDate = DateTime.Today,
                        DueDate = DateTime.Today.AddDays(30)
                    }
                }
            };

            CreditorBatchTrans purchaseCreateRes = await Client.PostAsync(purchaseCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseCreateRes.BatchID, Is.Not.Null);
            Assert.That(purchaseCreateRes.TransLines.Count, Is.EqualTo(1));
            Assert.That(purchaseCreateRes.TransLines[0].CreditorAccountNo, Is.EqualTo(purchaseCreateReq.TransLines[0].CreditorAccountNo));

            // Read the created creditor purchase line using the CreditorPurchaseID and LineID
            CreditorPurchaseLineGETRequest purchaseLineGetReq = new CreditorPurchaseLineGETRequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID,
                LineID = purchaseCreateRes.TransLines[0].CRBatchTranLineID
            };

            CRBatchTranLine purchaseLineGetRes = await Client.GetAsync(purchaseLineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchaseLineGetRes.CRBatchTranLineID, Is.EqualTo(purchaseCreateRes.TransLines[0].CRBatchTranLineID));
            Assert.That(purchaseLineGetRes.CreditorAccountNo, Is.EqualTo(purchaseCreateReq.TransLines[0].CreditorAccountNo));

            // Read the creditor purchase lines using the CreditorPurchaseID
            CreditorPurchaseLinesGETManyRequest purchaseLinesGetManyReq = new CreditorPurchaseLinesGETManyRequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID
            };

            List<CRBatchTranLine> purchaseLinesGetManyRes = await Client.GetAsync(purchaseLinesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchaseLinesGetManyRes.Count, Is.EqualTo(1));
            Assert.That(purchaseLinesGetManyRes.Any(x => x.CRBatchTranLineID == purchaseCreateRes.TransLines[0].CRBatchTranLineID), Is.True);

            // Add a creditor purchase line
            CreditorPurchaseLinePOSTRequest purchaseLineCreateReq = new CreditorPurchaseLinePOSTRequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID,
                RemitNo = RandomString(8),
                CreditorAccountNo = creditorCreateReq.AccountNo,
                HomeTransAmount = 234.56M,
                SupplierTransAmount = 234.56M,
                ReceiptDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(14)
            };

            CRBatchTranLine purchaseLineCreateRes = await Client.PostAsync(purchaseLineCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseLineCreateRes.CRBatchTranLineID, Is.Not.Null);
            Assert.That(purchaseLineCreateRes.CreditorAccountNo, Is.EqualTo(purchaseLineCreateReq.CreditorAccountNo));
            Assert.That(purchaseLineCreateRes.HomeTransAmount, Is.EqualTo(purchaseLineCreateReq.HomeTransAmount));

            // Read the creditor purchase lines again to ensure the appended line is returned
            purchaseLinesGetManyRes = await Client.GetAsync(purchaseLinesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchaseLinesGetManyRes.Count, Is.EqualTo(2));
            Assert.That(purchaseLinesGetManyRes.Any(x => x.CRBatchTranLineID == purchaseLineCreateRes.CRBatchTranLineID), Is.True);

            // Update the appended creditor purchase line
            CreditorPurchaseLinePATCHRequest purchaseLinePatchReq = new CreditorPurchaseLinePATCHRequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID,
                LineID = purchaseLineCreateRes.CRBatchTranLineID,
                HomeTransAmount = 345.67M,
                SupplierTransAmount = 345.67M,
                DueDate = DateTime.Today.AddDays(21)
            };

            CRBatchTranLine purchaseLinePatchRes = await Client.PatchAsync(purchaseLinePatchReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchaseLinePatchRes.CRBatchTranLineID, Is.EqualTo(purchaseLineCreateRes.CRBatchTranLineID));
            Assert.That(purchaseLinePatchRes.HomeTransAmount, Is.EqualTo(purchaseLinePatchReq.HomeTransAmount));
            Assert.That(purchaseLinePatchRes.DueDate, Is.EqualTo(purchaseLinePatchReq.DueDate));

            // Get the patched creditor purchase line and ensure it matches what we patched
            purchaseLineGetReq = new CreditorPurchaseLineGETRequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID,
                LineID = purchaseLineCreateRes.CRBatchTranLineID
            };

            purchaseLineGetRes = await Client.GetAsync(purchaseLineGetReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchaseLineGetRes.HomeTransAmount, Is.EqualTo(purchaseLinePatchReq.HomeTransAmount));
            Assert.That(purchaseLineGetRes.SupplierTransAmount, Is.EqualTo(purchaseLinePatchReq.SupplierTransAmount));
            Assert.That(purchaseLineGetRes.DueDate, Is.EqualTo(purchaseLinePatchReq.DueDate));

            // Remove the appended creditor purchase line
            CreditorPurchaseLineDELETERequest purchaseLineDeleteReq = new CreditorPurchaseLineDELETERequest()
            {
                CreditorPurchaseID = purchaseCreateRes.BatchID,
                LineID = purchaseLineCreateRes.CRBatchTranLineID
            };

            await Client.DeleteAsync(purchaseLineDeleteReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.NoContent));

            // Read the creditor purchase lines again to ensure the deleted line is no longer returned
            purchaseLinesGetManyRes = await Client.GetAsync(purchaseLinesGetManyReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(purchaseLinesGetManyRes.Count, Is.EqualTo(1));
            Assert.That(purchaseLinesGetManyRes.Any(x => x.CRBatchTranLineID == purchaseLineCreateRes.CRBatchTranLineID), Is.False);

            // Try to GET the deleted creditor purchase line to make sure we get a 404
            WebServiceException lineDeleteEx = Assert.ThrowsAsync<WebServiceException>(async () =>
            {
                CRBatchTranLine deletedPurchaseLineRes = await Client.GetAsync(purchaseLineGetReq);
            });
            Assert.That(lineDeleteEx.StatusCode, Is.EqualTo(404));
        }
        #endregion

        #region "Queries"
        [Test]
        public async Task CR_BatchTransQuery()
        {
            CR_BatchTransQuery CR_BatchTransQueryRequest = new CR_BatchTransQuery();
            ServiceStack.QueryResponse<CR_BatchTrans> CR_BatchTransQueryResponse;

            //Read all creditor purchases
            CR_BatchTransQueryResponse = await Client.GetAsync(CR_BatchTransQueryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Let's assume we expect to get at least one creditor purchase back - demo data has many creditor purchases.
            Assert.That(CR_BatchTransQueryResponse.Results.Count > 0);

            // Try with an invalid APIKey on to make sure we get a 401
            // Need to use a new client for this, as existing session Id's cookied will bind us to the session from
            // previous requests
            using (ServiceStack.JsonApiClient tempClient = new ServiceStack.JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                var ex = Assert.Throws<ServiceStack.WebServiceException>(() => CR_BatchTransQueryResponse = tempClient.Get(CR_BatchTransQueryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }
        }

        [Test]
        public async Task CR_BatchTransORQuery()
        {
            // Create a creditor to use for a creditor purchase
            CreditorPOSTRequest creditorCreateReq = new CreditorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Creditor Test"
            };

            Creditor creditorCreateRes = await Client.PostAsync(creditorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(creditorCreateRes.CreditorID, Is.Not.Null);
            Assert.That(creditorCreateRes.AccountNo, Is.EqualTo(creditorCreateReq.AccountNo));

            // Create a creditor purchase to query using OR routing
            CreditorPurchasePOSTRequest purchaseCreateReq = new CreditorPurchasePOSTRequest()
            {
                Description = $"Creditor Purchase OR Query Test {RandomString(5)}",
                BatchDate = DateTime.Today,
                TransLines = new List<CRBatchTranLine>()
                {
                    new CRBatchTranLine()
                    {
                        RemitNo = RandomString(8),
                        CreditorAccountNo = creditorCreateReq.AccountNo,
                        HomeTransAmount = 123.45M,
                        SupplierTransAmount = 123.45M,
                        ReceiptDate = DateTime.Today,
                        DueDate = DateTime.Today.AddDays(30)
                    }
                }
            };

            CreditorBatchTrans purchaseCreateRes = await Client.PostAsync(purchaseCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(purchaseCreateRes.BatchID, Is.Not.Null);

            CR_BatchTransORQuery CR_BatchTransORQueryRequest = new CR_BatchTransORQuery()
            {
                ReceiptID = Guid.NewGuid().ToString(),
                Description = purchaseCreateReq.Description
            };
            ServiceStack.QueryResponse<CR_BatchTransOR> CR_BatchTransORQueryResponse;

            //Read creditor purchases using OR query routing
            CR_BatchTransORQueryResponse = await Client.GetAsync(CR_BatchTransORQueryRequest);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            // Ensure the OR query returned the creditor purchase matching one of the supplied conditions.
            Assert.That(CR_BatchTransORQueryResponse.Results.Count > 0);
            Assert.That(CR_BatchTransORQueryResponse.Results.Any(x => x.Description == purchaseCreateReq.Description), Is.True);

            // Try with an invalid APIKey on to make sure we get a 401
            // Need to use a new client for this, as existing session Id's cookied will bind us to the session from
            // previous requests
            using (ServiceStack.JsonApiClient tempClient = new ServiceStack.JsonApiClient(Configuration.Hostname))
            {
                tempClient.BearerToken = "InvalidAPIKey";
                var ex = Assert.Throws<ServiceStack.WebServiceException>(() => CR_BatchTransORQueryResponse = tempClient.Get(CR_BatchTransORQueryRequest));
                Assert.That(ex.StatusCode, Is.EqualTo(401));
            }
        }
        #endregion

    }
}

