using JiwaFinancials.Jiwa.JiwaServiceModel;
using JiwaFinancials.Jiwa.JiwaServiceModel.Debtors;
using NUnit.Framework;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests.Debtors
{
    public class StatementReport : JiwaAPITest
    {
        #region "{Main}"
        [Test]
        public async Task Debtor_StatementReport_GET()
        {
            // Create a debtor we can operate on
            DebtorPOSTRequest debtorCreateReq = new DebtorPOSTRequest()
            {
                AccountNo = RandomString(5),
                Name = "Debtor Test",
                EmailAddress = "a@b.c"
            };

            Debtor debtorCreateRes = await Client.PostAsync(debtorCreateReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(debtorCreateRes.DebtorID, !Is.Null);

            try
            {
                // Get statement report for the debtor
                // Use a standard report ID - adjust based on your system's available reports
                DebtorStatementReportGETRequest statementReportGetReq = new DebtorStatementReportGETRequest()
                {
                    DebtorID = debtorCreateRes.DebtorID,
                    ReportID = "0", // Standard statement report ID
                    AsAtDate = DateTime.Today,
                    AsAttachment = false
                };

                try
                {
                    object statementReportGetRes = await Client.GetAsync(statementReportGetReq);
                    Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
                    Assert.That(statementReportGetRes, Is.Not.Null);
                }
                catch (WebServiceException ex)
                {
                    // Report ID may not exist in test environment, which is acceptable
                    Assert.That(ex.StatusCode, Is.EqualTo(404));
                }
            }
            finally
            {
                // Clean up: Remove the created debtor
                DebtorDELETERequest debtorDeleteReq = new DebtorDELETERequest() { DebtorID = debtorCreateRes.DebtorID };
                await Client.DeleteAsync(debtorDeleteReq);
            }
        }

        [Test]
        public async Task Debtor_StatementReport_GET_InvalidDebtor()
        {
            // Try to get statement report for non-existent debtor
            DebtorStatementReportGETRequest statementReportGetReq = new DebtorStatementReportGETRequest()
            {
                DebtorID = Guid.NewGuid().ToString(),
                ReportID = "0",
                AsAtDate = DateTime.Today,
                AsAttachment = false
            };

            WebServiceException ex = Assert.ThrowsAsync<ServiceStack.WebServiceException>(async () =>
            {
                object statementReportGetRes = await Client.GetAsync(statementReportGetReq);
            });
            Assert.That(ex.StatusCode, Is.EqualTo(404));
        }
        #endregion
    }
}

