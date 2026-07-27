using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables.Or;
using JiwaFinancials.Jiwa.JiwaServiceModel;
using NUnit.Framework;
using ServiceStack;
using System.Threading.Tasks;

namespace JiwaAPITests.AutoQueries
{
    [Route("/Queries/SY_WebhookMessage", "GET")]
    public class AutoQuerySY_WebhookMessageRouteQuery : IReturn<QueryResponse<SY_WebhookMessage>>, IGet { }
    [Route("/Queries/SY_WebhookMessageResponse", "GET")]
    public class AutoQuerySY_WebhookMessageResponseRouteQuery : IReturn<QueryResponse<SY_WebhookMessageResponse>>, IGet { }
    [Route("/Queries/SY_WebhookSubscription", "GET")]
    public class AutoQuerySY_WebhookSubscriptionRouteQuery : IReturn<QueryResponse<SY_WebhookSubscription>>, IGet { }
    [Route("/Queries/SY_WebhookSubscriptionRequestHeader", "GET")]
    public class AutoQuerySY_WebhookSubscriptionRequestHeaderRouteQuery : IReturn<QueryResponse<SY_WebhookSubscriptionRequestHeader>>, IGet { }
    [Route("/Queries/OR/SY_WebhookMessage", "GET")]
    public class AutoQuerySY_WebhookMessageORRouteQuery : IReturn<QueryResponse<SY_WebhookMessageOR>>, IGet { }
    [Route("/Queries/OR/SY_WebhookMessageResponse", "GET")]
    public class AutoQuerySY_WebhookMessageResponseORRouteQuery : IReturn<QueryResponse<SY_WebhookMessageResponseOR>>, IGet { }
    [Route("/Queries/OR/SY_WebhookSubscription", "GET")]
    public class AutoQuerySY_WebhookSubscriptionORRouteQuery : IReturn<QueryResponse<SY_WebhookSubscriptionOR>>, IGet { }
    [Route("/Queries/OR/SY_WebhookSubscriptionRequestHeader", "GET")]
    public class AutoQuerySY_WebhookSubscriptionRequestHeaderORRouteQuery : IReturn<QueryResponse<SY_WebhookSubscriptionRequestHeaderOR>>, IGet { }

    public partial class AutoQuery
    {
        private async Task AssertQueryGet<TResult>(IReturn<QueryResponse<TResult>> request)
        {
            QueryResponse<TResult> res = await Client.GetAsync(request);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }

        #region "/Queries/OR/DB_Categories"
        [Test]
        public async Task OR_DB_Categories_GET()
        {
            // Retrieve OR DB_Categories records
            await AssertQueryGet(new DB_CategoriesORQuery());
        }
        #endregion

        #region "/Queries/OR/DB_Classification"
        [Test]
        public async Task OR_DB_Classification_GET()
        {
            // Retrieve OR DB_Classification records
            await AssertQueryGet(new DB_ClassificationORQuery());
        }
        #endregion

        #region "/Queries/OR/DB_PricingGroups"
        [Test]
        public async Task OR_DB_PricingGroups_GET()
        {
            // Retrieve OR DB_PricingGroups records
            await AssertQueryGet(new DB_PricingGroupsORQuery());
        }
        #endregion

        #region "/Queries/OR/FR_Carriers"
        [Test]
        public async Task OR_FR_Carriers_GET()
        {
            // Retrieve OR FR_Carriers records
            await AssertQueryGet(new FR_CarriersORQuery());
        }
        #endregion

        #region "/Queries/OR/FX_Currency"
        [Test]
        public async Task OR_FX_Currency_GET()
        {
            // Retrieve OR FX_Currency records
            await AssertQueryGet(new FX_CurrencyORQuery());
        }
        #endregion

        #region "/Queries/OR/FX_CurrencyRates"
        [Test]
        public async Task OR_FX_CurrencyRates_GET()
        {
            // Retrieve OR FX_CurrencyRates records
            await AssertQueryGet(new FX_CurrencyRatesORQuery());
        }
        #endregion

        #region "/Queries/OR/GL_Ledger"
        [Test]
        public async Task OR_GL_Ledger_GET()
        {
            // Retrieve OR GL_Ledger records
            await AssertQueryGet(new GL_LedgerORQuery());
        }
        #endregion

        #region "/Queries/OR/GL_Category"
        [Test]
        public async Task OR_GL_Category_GET()
        {
            // Retrieve OR GL_Category records
            await AssertQueryGet(new GL_CategoryORQuery());
        }
        #endregion

        #region "/Queries/OR/GL_Sets"
        [Test]
        public async Task OR_GL_Sets_GET()
        {
            // Retrieve OR GL_Sets records
            await AssertQueryGet(new GL_SetsORQuery());
        }
        #endregion

        #region "/Queries/OR/HR_Departments"
        [Test]
        public async Task OR_HR_Departments_GET()
        {
            // Retrieve OR HR_Departments records
            await AssertQueryGet(new HR_DepartmentsORQuery());
        }
        #endregion

        #region "/Queries/OR/HR_Staff"
        [Test]
        public async Task OR_HR_Staff_GET()
        {
            // Retrieve OR HR_Staff records
            await AssertQueryGet(new HR_StaffORQuery());
        }
        #endregion

        #region "/Queries/OR/IN_AttributeGroupTemplate"
        [Test]
        public async Task OR_IN_AttributeGroupTemplate_GET()
        {
            // Retrieve OR IN_AttributeGroupTemplate records
            await AssertQueryGet(new IN_AttributeGroupTemplateORQuery());
        }
        #endregion

        #region "/Queries/OR/IN_BinLocationLookup"
        [Test]
        public async Task OR_IN_BinLocationLookup_GET()
        {
            // Retrieve OR IN_BinLocationLookup records
            await AssertQueryGet(new IN_BinLocationLookupORQuery());
        }
        #endregion

        #region "/Queries/OR/IN_Categories"
        [Test]
        public async Task OR_IN_Categories_GET()
        {
            // Retrieve OR IN_Categories records
            await AssertQueryGet(new IN_CategoriesORQuery());
        }
        #endregion

        #region "/Queries/OR/IN_Classification"
        [Test]
        public async Task OR_IN_Classification_GET()
        {
            // Retrieve OR IN_Classification records
            await AssertQueryGet(new IN_ClassificationORQuery());
        }
        #endregion

        #region "/Queries/OR/IN_Logical"
        [Test]
        public async Task OR_IN_Logical_GET()
        {
            // Retrieve OR IN_Logical records
            await AssertQueryGet(new IN_LogicalORQuery());
        }
        #endregion

        #region "/Queries/OR/IN_Region"
        [Test]
        public async Task OR_IN_Region_GET()
        {
            // Retrieve OR IN_Region records
            await AssertQueryGet(new IN_RegionQuery());
        }
        #endregion

        #region "/Queries/OR/IN_Main"
        [Test]
        public async Task OR_IN_Main_GET()
        {
            // Retrieve OR IN_Main records
            await AssertQueryGet(new IN_MainORQuery());
        }
        #endregion

        #region "/Queries/OR/IN_SOH"
        [Test]
        public async Task OR_IN_SOH_GET()
        {
            // Retrieve OR IN_SOH records
            await AssertQueryGet(new IN_SOHORQuery());
        }
        #endregion

        #region "/Queries/OR/IN_Physical"
        [Test]
        public async Task OR_IN_Physical_GET()
        {
            // Retrieve OR IN_Physical records
            await AssertQueryGet(new IN_PhysicalORQuery());
        }
        #endregion

        #region "/Queries/OR/IN_Transfer"
        [Test]
        public async Task OR_IN_Transfer_GET()
        {
            // Retrieve OR IN_Transfer records
            await AssertQueryGet(new IN_TransferORQuery());
        }
        #endregion

        #region "/Queries/OR/PI_Main"
        [Test]
        public async Task OR_PI_Main_GET()
        {
            // Retrieve OR PI_Main records
            await AssertQueryGet(new PI_MainORQuery());
        }
        #endregion

        #region "/Queries/OR/PO_Main"
        [Test]
        public async Task OR_PO_Main_GET()
        {
            // Retrieve OR PO_Main records
            await AssertQueryGet(new PO_MainORQuery());
        }
        #endregion

        #region "/Queries/OR/RE_Main"
        [Test]
        public async Task OR_RE_Main_GET()
        {
            // Retrieve OR RE_Main records
            await AssertQueryGet(new RE_MainQuery());
        }
        #endregion

        #region "/Queries/OR/SH_Main"
        [Test]
        public async Task OR_SH_Main_GET()
        {
            // Retrieve OR SH_Main records
            await AssertQueryGet(new SH_MainQuery());
        }
        #endregion

        #region "/Queries/OR/SH_BookInMain"
        [Test]
        public async Task OR_SH_BookInMain_GET()
        {
            // Retrieve OR SH_BookInMain records
            await AssertQueryGet(new SH_BookInMainORQuery());
        }
        #endregion

        #region "/Queries/OR/SO_Main"
        [Test]
        public async Task OR_SO_Main_GET()
        {
            // Retrieve OR SO_Main records
            await AssertQueryGet(new SO_MainORQuery());
        }
        #endregion

        #region "/Queries/OR/QO_Main"
        [Test]
        public async Task OR_QO_Main_GET()
        {
            // Retrieve OR QO_Main records
            await AssertQueryGet(new QO_MainORQuery());
        }
        #endregion

        #region "/Queries/OR/TX_Main"
        [Test]
        public async Task OR_TX_Main_GET()
        {
            // Retrieve OR TX_Main records
            await AssertQueryGet(new TX_MainORQuery());
        }
        #endregion

        #region "/Queries/OR/SY_Branch"
        [Test]
        public async Task OR_SY_Branch_GET()
        {
            // Retrieve OR SY_Branch records
            await AssertQueryGet(new SY_BranchORQuery());
        }
        #endregion

        #region "/Queries/OR/SY_Plugin"
        [Test]
        public async Task OR_SY_Plugin_GET()
        {
            // Retrieve OR SY_Plugin records
            await AssertQueryGet(new SY_PluginORQuery());
        }
        #endregion

        #region "/Queries/OR/SY_Report"
        [Test]
        public async Task OR_SY_Report_GET()
        {
            // Retrieve OR SY_Report records
            await AssertQueryGet(new SY_ReportORQuery());
        }
        #endregion

        #region "/Queries/OR/SY_ReportSection"
        [Test]
        public async Task OR_SY_ReportSection_GET()
        {
            // Retrieve OR SY_ReportSection records
            await AssertQueryGet(new SY_ReportSectionORQuery());
        }
        #endregion

        #region "/Queries/OR/IN_WarehouseSOH"
        [Test]
        public async Task OR_IN_WarehouseSOH_GET()
        {
            // Retrieve OR IN_WarehouseSOH records
            await AssertQueryGet(new IN_WarehouseSOHORQuery());
        }
        #endregion

        #region "/Queries/OR/SY_SysValues"
        [Test]
        public async Task OR_SY_SysValues_GET()
        {
            // Retrieve OR SY_SysValues records
            await AssertQueryGet(new SY_SysValuesORQuery());
        }
        #endregion

        #region "/Queries/OR/SY_WebhookSubscriber"
        [Test]
        public async Task OR_SY_WebhookSubscriber_GET()
        {
            // Retrieve OR SY_WebhookSubscriber records
            await AssertQueryGet(new SY_WebhookSubscriberORQuery());
        }
        #endregion

        #region "/Queries/OR/SY_WebhookSubscription"
        [Test]
        public async Task OR_SY_WebhookSubscription_GET()
        {
            // Retrieve OR SY_WebhookSubscription records
            await AssertQueryGet(new AutoQuerySY_WebhookSubscriptionORRouteQuery());
        }
        #endregion

        #region "/Queries/OR/SY_WebhookSubscriptionRequestHeader"
        [Test]
        public async Task OR_SY_WebhookSubscriptionRequestHeader_GET()
        {
            // Retrieve OR SY_WebhookSubscriptionRequestHeader records
            await AssertQueryGet(new AutoQuerySY_WebhookSubscriptionRequestHeaderORRouteQuery());
        }
        #endregion

        #region "/Queries/OR/SY_WebhookMessage"
        [Test]
        public async Task OR_SY_WebhookMessage_GET()
        {
            // Retrieve OR SY_WebhookMessage records
            await AssertQueryGet(new AutoQuerySY_WebhookMessageORRouteQuery());
        }
        #endregion

        #region "/Queries/OR/SY_WebhookMessageResponse"
        [Test]
        public async Task OR_SY_WebhookMessageResponse_GET()
        {
            // Retrieve OR SY_WebhookMessageResponse records
            await AssertQueryGet(new AutoQuerySY_WebhookMessageResponseORRouteQuery());
        }
        #endregion

        #region "/Queries/OR/InventoryItemList"
        [Test]
        public async Task OR_InventoryItemList_GET()
        {
            // Retrieve OR InventoryItemList records
            await AssertQueryGet(new v_Jiwa_Inventory_Item_ListORQuery());
        }
        #endregion

        #region "/Queries/OR/SalesOrderList"
        [Test]
        public async Task OR_SalesOrderList_GET()
        {
            // Retrieve OR SalesOrderList records
            await AssertQueryGet(new v_Jiwa_SalesOrder_ListORQuery());
        }
        #endregion

        #region "/Queries/OR/SalesQuoteList"
        [Test]
        public async Task OR_SalesQuoteList_GET()
        {
            // Retrieve OR SalesQuoteList records
            await AssertQueryGet(new v_Jiwa_SalesQuote_ListORQuery());
        }
        #endregion

        #region "/Queries/OR/DebtorList"
        [Test]
        public async Task OR_DebtorList_GET()
        {
            // Retrieve OR DebtorList records
            await AssertQueryGet(new v_Jiwa_Debtor_ListORQuery());
        }
        #endregion

        #region "/Queries/OR/WH_Transfer"
        [Test]
        public async Task OR_WH_Transfer_GET()
        {
            // Retrieve OR WH_Transfer records
            await AssertQueryGet(new WH_TransferORQuery());
        }
        #endregion

        #region "/Queries/OR/BM_WorkOrderSelection"
        [Test]
        public async Task OR_BM_WorkOrderSelection_GET()
        {
            // Retrieve OR BM_WorkOrderSelection records
            await AssertQueryGet(new v_BM_WorkOrderSelectionORQuery());
        }
        #endregion

        #region "/Queries/OR/BM_WorkCentreSelection"
        [Test]
        public async Task OR_BM_WorkCentreSelection_GET()
        {
            // Retrieve OR BM_WorkCentreSelection records
            await AssertQueryGet(new v_BM_WorkCentreSelectionORQuery());
        }
        #endregion

        #region "/Queries/OR/StaffTimesheets"
        [Test]
        public async Task OR_StaffTimesheets_GET()
        {
            // Retrieve OR StaffTimesheets records
            await AssertQueryGet(new v_StaffTimesheetsORQuery());
        }
        #endregion

        #region "/Queries/OR/INSOHWithBinLocations"
        [Test]
        public async Task OR_INSOHWithBinLocations_GET()
        {
            // Retrieve OR INSOHWithBinLocations records
            await AssertQueryGet(new v_IN_SOHWithBinLocationsORQuery());
        }
        #endregion

        #region "/Queries/OR/InventoryBarCodeList"
        [Test]
        public async Task OR_InventoryBarCodeList_GET()
        {
            // Retrieve OR InventoryBarCodeList records
            await AssertQueryGet(new v_InventoryBarCodeListORQuery());
        }
        #endregion

        #region "/Queries/OR/PurchaseOrderSelection"
        [Test]
        public async Task OR_PurchaseOrderSelection_GET()
        {
            // Retrieve OR PurchaseOrderSelection records
            await AssertQueryGet(new v_PurchaseOrderSelectionORQuery());
        }
        #endregion

        #region "/Queries/OR/WorkOrderStatusesSelection"
        [Test]
        public async Task OR_WorkOrderStatusesSelection_GET()
        {
            // Retrieve OR WorkOrderStatusesSelection records
            await AssertQueryGet(new v_WorkOrderStatusesORQuery());
        }
        #endregion

        #region "/Queries/OR/WarehouseSelection"
        [Test]
        public async Task OR_WarehouseSelection_GET()
        {
            // Retrieve OR WarehouseSelection records
            await AssertQueryGet(new v_WarehouseSelectionORQuery());
        }
        #endregion

        #region "/Queries/OR/BinLocationSelection"
        [Test]
        public async Task OR_BinLocationSelection_GET()
        {
            // Retrieve OR BinLocationSelection records
            await AssertQueryGet(new v_BinLocationLookupORQuery());
        }
        #endregion

        #region "/Queries/OR/BMProductsForOutputs"
        [Test]
        public async Task OR_BMProductsForOutputs_GET()
        {
            // Retrieve OR BMProductsForOutputs records
            await AssertQueryGet(new V_BMProductsForOutputORQuery());
        }
        #endregion

        #region "/Queries/OR/StaffUserGroups"
        [Test]
        public async Task OR_StaffUserGroups_GET()
        {
            // Retrieve OR StaffUserGroups records
            await AssertQueryGet(new v_StaffUserGroupsORQuery());
        }
        #endregion

        #region "/Queries/OR/TimeSheetSelection"
        [Test]
        public async Task OR_TimeSheetSelection_GET()
        {
            // Retrieve OR TimeSheetSelection records
            await AssertQueryGet(new v_TimeSheetSelectionORQuery());
        }
        #endregion

        #region "/Queries/OR/TimeSheetWithWorkOrdersSelection"
        [Test]
        public async Task OR_TimeSheetWithWorkOrdersSelection_GET()
        {
            // Retrieve OR TimeSheetWithWorkOrdersSelection records
            await AssertQueryGet(new v_TimeSheetWithWorkOrdersSelectionQuery());
        }
        #endregion

        #region "/Queries/OR/WorkOrderToDoSelection"
        [Test]
        public async Task OR_WorkOrderToDoSelection_GET()
        {
            // Retrieve OR WorkOrderToDoSelection records
            await AssertQueryGet(new v_BM_WorkOrderToDoSelectionORQuery());
        }
        #endregion

        #region "/Queries/OR/GR_ReceivalDocuments"
        [Test]
        public async Task OR_GR_ReceivalDocuments_GET()
        {
            // Retrieve OR GR_ReceivalDocuments records
            await AssertQueryGet(new v_GR_ReceivalDocumentsORQuery());
        }
        #endregion

        #region "/Queries/OR/ServiceManagerSelectionQuery"
        [Test]
        public async Task OR_ServiceManagerSelectionQuery_GET()
        {
            // Retrieve OR ServiceManagerSelectionQuery records
            await AssertQueryGet(new vServiceManagerSelectionORQuery());
        }
        #endregion

        #region "/Queries/OR/ServiceManagerStatuses"
        [Test]
        public async Task OR_ServiceManagerStatuses_GET()
        {
            // Retrieve OR ServiceManagerStatuses records
            await AssertQueryGet(new SM_StatusesORQuery());
        }
        #endregion

        #region "/Queries/OR/ServiceManagerActivities"
        [Test]
        public async Task OR_ServiceManagerActivities_GET()
        {
            // Retrieve OR ServiceManagerActivities records
            await AssertQueryGet(new SM_ActivitiesORQuery());
        }
        #endregion

        #region "/Queries/OR/TimeSheetCombined"
        [Test]
        public async Task OR_TimeSheetCombined_GET()
        {
            // Retrieve OR TimeSheetCombined records
            await AssertQueryGet(new v_TimeSheetCombinedSelectionORQuery());
        }
        #endregion

        #region "/Queries/OR/TimeSheetSelectionWithFlags"
        [Test]
        public async Task OR_TimeSheetSelectionWithFlags_GET()
        {
            // Retrieve OR TimeSheetSelectionWithFlags records
            await AssertQueryGet(new vTimeSheetSelectionWithFlagsORQuery());
        }
        #endregion

        #region "/Queries/OR/JobCostingSelection"
        [Test]
        public async Task OR_JobCostingSelection_GET()
        {
            // Retrieve OR JobCostingSelection records
            await AssertQueryGet(new vJobCostingSelectionORQuery());
        }
        #endregion

        #region "/Queries/OR/WorkOrderSelection"
        [Test]
        public async Task OR_WorkOrderSelection_GET()
        {
            // Retrieve OR WorkOrderSelection records
            await AssertQueryGet(new vWorkOrderSelectionORQuery());
        }
        #endregion

        #region "/Queries/OR/WorkOrderStageSelection"
        [Test]
        public async Task OR_WorkOrderStageSelection_GET()
        {
            // Retrieve OR WorkOrderStageSelection records
            await AssertQueryGet(new vWorkOrderStageSelectionORQuery());
        }
        #endregion

        #region "/Queries/OR/DebtorTransactionList"
        [Test]
        public async Task OR_DebtorTransactionList_GET()
        {
            // Retrieve OR DebtorTransactionList records
            await AssertQueryGet(new v_Jiwa_Debtor_Transactions_ListORQuery());
        }
        #endregion

        #region "/Queries/OR/BackOrderList"
        [Test]
        public async Task OR_BackOrderList_GET()
        {
            // Retrieve OR BackOrderList records
            await AssertQueryGet(new v_Jiwa_Debtor_BackOrders_ListORQuery());
        }
        #endregion

        #region "/Queries/OR/v_Jiwa_SalesInformation"
        [Test]
        public async Task OR_v_Jiwa_SalesInformation_GET()
        {
            // Retrieve OR v_Jiwa_SalesInformation records
            await AssertQueryGet(new v_Jiwa_SalesInformationORQuery());
        }
        #endregion

        #region "/Queries/OR/v_Jiwa_SalesOrders"
        [Test]
        public async Task OR_v_Jiwa_SalesOrders_GET()
        {
            // Retrieve OR v_Jiwa_SalesOrders records
            await AssertQueryGet(new v_Jiwa_SalesOrdersORQuery());
        }
        #endregion

        #region "/Queries/OR/v_Jiwa_PurchaseInformation"
        [Test]
        public async Task OR_v_Jiwa_PurchaseInformation_GET()
        {
            // Retrieve OR v_Jiwa_PurchaseInformation records
            await AssertQueryGet(new v_Jiwa_PurchaseInformationORQuery());
        }
        #endregion

        #region "/Queries/OR/v_Jiwa_PurchaseOrders"
        [Test]
        public async Task OR_v_Jiwa_PurchaseOrders_GET()
        {
            // Retrieve OR v_Jiwa_PurchaseOrders records
            await AssertQueryGet(new v_Jiwa_PurchaseOrdersORQuery());
        }
        #endregion

        #region "/Queries/OR/SY_Forms"
        [Test]
        public async Task OR_SY_Forms_GET()
        {
            // Retrieve OR SY_Forms records
            await AssertQueryGet(new SY_FormsORQuery());
        }
        #endregion

        #region "/Queries/OR/InventoryItemListImmutableWarehouse"
        [Test]
        public async Task OR_InventoryItemListImmutableWarehouse_GET()
        {
            // Retrieve OR InventoryItemListImmutableWarehouse records
            await AssertQueryGet(new v_Jiwa_Inventory_Item_List_OR_ImmutableWarehouseQuery());
        }
        #endregion

        #region "/Queries/PI_Main"
        [Test]
        public async Task PI_Main_GET()
        {
            // Retrieve PI_Main records
            await AssertQueryGet(new PI_MainQuery());
        }
        #endregion

        #region "/Queries/PluginExceptions"
        [Test]
        public async Task PluginExceptions_GET()
        {
            // Retrieve PluginExceptions records
            await AssertQueryGet(new PluginExceptionQuery());
        }
        #endregion

        #region "/Queries/PO_Main"
        [Test]
        public async Task PO_Main_GET()
        {
            // Retrieve PO_Main records
            await AssertQueryGet(new PO_MainQuery());
        }
        #endregion

        #region "/Queries/PurchaseOrderSelection"
        [Test]
        public async Task PurchaseOrderSelection_GET()
        {
            // Retrieve PurchaseOrderSelection records
            await AssertQueryGet(new v_PurchaseOrderSelectionQuery());
        }
        #endregion

        #region "/Queries/QO_Main"
        [Test]
        public async Task QO_Main_GET()
        {
            // Retrieve QO_Main records
            await AssertQueryGet(new QO_MainQuery());
        }
        #endregion

        #region "/Queries/RE_Main"
        [Test]
        public async Task RE_Main_GET()
        {
            // Retrieve RE_Main records
            await AssertQueryGet(new RE_MainQuery());
        }
        #endregion

        #region "/Queries/SalesOrderList"
        [Test]
        public async Task SalesOrderList_GET()
        {
            // Retrieve SalesOrderList records
            await AssertQueryGet(new v_Jiwa_SalesOrder_ListQuery());
        }
        #endregion

        #region "/Queries/SalesQuoteList"
        [Test]
        public async Task SalesQuoteList_GET()
        {
            // Retrieve SalesQuoteList records
            await AssertQueryGet(new v_Jiwa_SalesQuote_ListQuery());
        }
        #endregion

        #region "/Queries/ServiceManagerActivities"
        [Test]
        public async Task ServiceManagerActivities_GET()
        {
            // Retrieve ServiceManagerActivities records
            await AssertQueryGet(new SM_ActivitiesQuery());
        }
        #endregion

        #region "/Queries/ServiceManagerSelectionQuery"
        [Test]
        public async Task ServiceManagerSelectionQuery_GET()
        {
            // Retrieve ServiceManagerSelectionQuery records
            await AssertQueryGet(new vServiceManagerSelectionQuery());
        }
        #endregion

        #region "/Queries/ServiceManagerStatuses"
        [Test]
        public async Task ServiceManagerStatuses_GET()
        {
            // Retrieve ServiceManagerStatuses records
            await AssertQueryGet(new SM_StatusesQuery());
        }
        #endregion

        #region "/Queries/SH_BookInMain"
        [Test]
        public async Task SH_BookInMain_GET()
        {
            // Retrieve SH_BookInMain records
            await AssertQueryGet(new SH_BookInMainQuery());
        }
        #endregion

        #region "/Queries/SH_Main"
        [Test]
        public async Task SH_Main_GET()
        {
            // Retrieve SH_Main records
            await AssertQueryGet(new SH_MainQuery());
        }
        #endregion

        #region "/Queries/SO_Main"
        [Test]
        public async Task SO_Main_GET()
        {
            // Retrieve SO_Main records
            await AssertQueryGet(new SO_MainQuery());
        }
        #endregion

        #region "/Queries/StaffTimesheets"
        [Test]
        public async Task StaffTimesheets_GET()
        {
            // Retrieve StaffTimesheets records
            await AssertQueryGet(new v_StaffTimesheetsQuery());
        }
        #endregion

        #region "/Queries/StaffUserGroups"
        [Test]
        public async Task StaffUserGroups_GET()
        {
            // Retrieve StaffUserGroups records
            await AssertQueryGet(new v_StaffUserGroupsQuery());
        }
        #endregion

        #region "/Queries/StartupLog"
        [Test]
        public async Task StartupLog_GET()
        {
            // Retrieve StartupLog records
            await AssertQueryGet(new StartupLogEntryQuery());
        }
        #endregion

        #region "/Queries/SY_Branch"
        [Test]
        public async Task SY_Branch_GET()
        {
            // Retrieve SY_Branch records
            await AssertQueryGet(new SY_BranchQuery());
        }
        #endregion

        #region "/Queries/SY_Forms"
        [Test]
        public async Task SY_Forms_GET()
        {
            // Retrieve SY_Forms records
            await AssertQueryGet(new SY_FormsQuery());
        }
        #endregion

        #region "/Queries/SY_Plugin"
        [Test]
        public async Task SY_Plugin_GET()
        {
            // Retrieve SY_Plugin records
            await AssertQueryGet(new SY_PluginQuery());
        }
        #endregion

        #region "/Queries/SY_Report"
        [Test]
        public async Task SY_Report_GET()
        {
            // Retrieve SY_Report records
            await AssertQueryGet(new SY_ReportQuery());
        }
        #endregion

        #region "/Queries/SY_ReportSection"
        [Test]
        public async Task SY_ReportSection_GET()
        {
            // Retrieve SY_ReportSection records
            await AssertQueryGet(new SY_ReportSectionQuery());
        }
        #endregion

        #region "/Queries/SY_SysValues"
        [Test]
        public async Task SY_SysValues_GET()
        {
            // Retrieve SY_SysValues records
            await AssertQueryGet(new SY_SysValuesQuery());
        }
        #endregion

        #region "/Queries/SY_WebhookMessage"
        [Test]
        public async Task SY_WebhookMessage_GET()
        {
            // Retrieve SY_WebhookMessage records
            await AssertQueryGet(new AutoQuerySY_WebhookMessageRouteQuery());
        }
        #endregion

        #region "/Queries/SY_WebhookMessageResponse"
        [Test]
        public async Task SY_WebhookMessageResponse_GET()
        {
            // Retrieve SY_WebhookMessageResponse records
            await AssertQueryGet(new AutoQuerySY_WebhookMessageResponseRouteQuery());
        }
        #endregion

        #region "/Queries/SY_WebhookSubscriber"
        [Test]
        public async Task SY_WebhookSubscriber_GET()
        {
            // Retrieve SY_WebhookSubscriber records
            await AssertQueryGet(new SY_WebhookSubscriberQuery());
        }
        #endregion

        #region "/Queries/SY_WebhookSubscription"
        [Test]
        public async Task SY_WebhookSubscription_GET()
        {
            // Retrieve SY_WebhookSubscription records
            await AssertQueryGet(new AutoQuerySY_WebhookSubscriptionRouteQuery());
        }
        #endregion

        #region "/Queries/SY_WebhookSubscriptionRequestHeader"
        [Test]
        public async Task SY_WebhookSubscriptionRequestHeader_GET()
        {
            // Retrieve SY_WebhookSubscriptionRequestHeader records
            await AssertQueryGet(new AutoQuerySY_WebhookSubscriptionRequestHeaderRouteQuery());
        }
        #endregion

        #region "/Queries/TimeSheetCombined"
        [Test]
        public async Task TimeSheetCombined_GET()
        {
            // Retrieve TimeSheetCombined records
            await AssertQueryGet(new v_TimeSheetCombinedSelectionQuery());
        }
        #endregion

        #region "/Queries/TimeSheetSelection"
        [Test]
        public async Task TimeSheetSelection_GET()
        {
            // Retrieve TimeSheetSelection records
            await AssertQueryGet(new v_TimeSheetSelectionQuery());
        }
        #endregion

        #region "/Queries/TimeSheetSelectionWithFlags"
        [Test]
        public async Task TimeSheetSelectionWithFlags_GET()
        {
            // Retrieve TimeSheetSelectionWithFlags records
            await AssertQueryGet(new vTimeSheetSelectionWithFlagsQuery());
        }
        #endregion

        #region "/Queries/TimeSheetWithWorkOrdersSelection"
        [Test]
        public async Task TimeSheetWithWorkOrdersSelection_GET()
        {
            // Retrieve TimeSheetWithWorkOrdersSelection records
            await AssertQueryGet(new v_TimeSheetWithWorkOrdersSelectionQuery());
        }
        #endregion

        #region "/Queries/TX_Main"
        [Test]
        public async Task TX_Main_GET()
        {
            // Retrieve TX_Main records
            await AssertQueryGet(new TX_MainQuery());
        }
        #endregion

        #region "/Queries/v_Jiwa_CreditorSummary"
        [Test]
        public async Task v_Jiwa_CreditorSummary_GET()
        {
            // Retrieve v_Jiwa_CreditorSummary records
            await AssertQueryGet(new v_Jiwa_CreditorSummaryQuery());
        }
        #endregion

        #region "/Queries/v_Jiwa_PurchaseInformation"
        [Test]
        public async Task v_Jiwa_PurchaseInformation_GET()
        {
            // Retrieve v_Jiwa_PurchaseInformation records
            await AssertQueryGet(new v_Jiwa_PurchaseInformationQuery());
        }
        #endregion

        #region "/Queries/v_Jiwa_PurchaseOrders"
        [Test]
        public async Task v_Jiwa_PurchaseOrders_GET()
        {
            // Retrieve v_Jiwa_PurchaseOrders records
            await AssertQueryGet(new v_Jiwa_PurchaseOrdersQuery());
        }
        #endregion

        #region "/Queries/v_Jiwa_SalesInformation"
        [Test]
        public async Task v_Jiwa_SalesInformation_GET()
        {
            // Retrieve v_Jiwa_SalesInformation records
            await AssertQueryGet(new v_Jiwa_SalesInformationQuery());
        }
        #endregion

        #region "/Queries/v_Jiwa_SalesOrders"
        [Test]
        public async Task v_Jiwa_SalesOrders_GET()
        {
            // Retrieve v_Jiwa_SalesOrders records
            await AssertQueryGet(new v_Jiwa_SalesOrdersQuery());
        }
        #endregion

        #region "/Queries/WarehouseSelection"
        [Test]
        public async Task WarehouseSelection_GET()
        {
            // Retrieve WarehouseSelection records
            await AssertQueryGet(new v_WarehouseSelectionQuery());
        }
        #endregion

        #region "/Queries/WH_Transfer"
        [Test]
        public async Task WH_Transfer_GET()
        {
            // Retrieve WH_Transfer records
            await AssertQueryGet(new WH_TransferQuery());
        }
        #endregion

        #region "/Queries/WorkOrderSelection"
        [Test]
        public async Task WorkOrderSelection_GET()
        {
            // Retrieve WorkOrderSelection records
            await AssertQueryGet(new vWorkOrderSelectionQuery());
        }
        #endregion

        #region "/Queries/WorkOrderStageSelection"
        [Test]
        public async Task WorkOrderStageSelection_GET()
        {
            // Retrieve WorkOrderStageSelection records
            await AssertQueryGet(new vWorkOrderStageSelectionQuery());
        }
        #endregion

        #region "/Queries/WorkOrderStatusesSelection"
        [Test]
        public async Task WorkOrderStatusesSelection_GET()
        {
            // Retrieve WorkOrderStatusesSelection records
            await AssertQueryGet(new v_WorkOrderStatusesQuery());
        }
        #endregion

        #region "/Queries/WorkOrderToDoSelection"
        [Test]
        public async Task WorkOrderToDoSelection_GET()
        {
            // Retrieve WorkOrderToDoSelection records
            await AssertQueryGet(new v_BM_WorkOrderToDoSelectionQuery());
        }
        #endregion
    }
}
