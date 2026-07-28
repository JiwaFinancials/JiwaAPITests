using JiwaFinancials.Jiwa.JiwaServiceModel.Tables;
using JiwaFinancials.Jiwa.JiwaServiceModel.Tables.Or;
using NUnit.Framework;
using ServiceStack;
using System.Threading.Tasks;

namespace JiwaAPITests.AutoQueries
{
    [Route("/Queries/v_Jiwa_CreditorPurchases", "GET")]
    public class AutoQueryv_Jiwa_CreditorPurchasesRouteQuery : IReturn<QueryResponse<v_Jiwa_CreditorPurchases>>, IGet { }

    public partial class AutoQuery : JiwaAPITest
    {
        #region "/Queries/BackOrderList"
        [Test]
        public async Task BackOrderList_GET()
        {
            // Retrieve the back order list
            v_Jiwa_Debtor_BackOrders_ListQuery backOrderListReq = new v_Jiwa_Debtor_BackOrders_ListQuery();
            QueryResponse<v_Jiwa_Debtor_BackOrders_List> backOrderListRes = await Client.GetAsync(backOrderListReq);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(backOrderListRes, Is.Not.Null);
            Assert.That(backOrderListRes.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/BinLocationQuantities"
        [Test]
        public async Task BinLocationQuantities_GET()
        {
            // Retrieve bin location quantities
            v_Jiwa_Inventory_Bin_LocationsQuery req = new v_Jiwa_Inventory_Bin_LocationsQuery();
            QueryResponse<v_Jiwa_Inventory_Bin_Locations> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/BinLocationSelection"
        [Test]
        public async Task BinLocationSelection_GET()
        {
            // Retrieve bin location selection
            v_BinLocationLookupQuery req = new v_BinLocationLookupQuery();
            QueryResponse<v_BinLocationLookup> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/BMProductsForOutputs"
        [Test]
        public async Task BMProductsForOutputs_GET()
        {
            // Retrieve BM products for outputs
            V_BMProductsForOutputQuery req = new V_BMProductsForOutputQuery();
            QueryResponse<V_BMProductsForOutput> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/BM_Main"
        [Test]
        public async Task BM_Main_GET()
        {
            // Retrieve BM main records
            BM_MainQuery req = new BM_MainQuery();
            QueryResponse<BM_Main> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/BM_WorkCentreSelection"
        [Test]
        public async Task BM_WorkCentreSelection_GET()
        {
            // Retrieve BM work centre selection
            v_BM_WorkCentreSelectionQuery req = new v_BM_WorkCentreSelectionQuery();
            QueryResponse<v_BM_WorkCentreSelection> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/BM_WorkOrder"
        [Test]
        public async Task BM_WorkOrder_GET()
        {
            // Retrieve BM work orders
            BM_WorkOrderQuery req = new BM_WorkOrderQuery();
            QueryResponse<BM_WorkOrder> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/BM_WorkOrderSelection"
        [Test]
        public async Task BM_WorkOrderSelection_GET()
        {
            // Retrieve BM work order selection
            v_BM_WorkOrderSelectionQuery req = new v_BM_WorkOrderSelectionQuery();
            QueryResponse<v_BM_WorkOrderSelection> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/CR_BatchTrans"
        [Test]
        public async Task CR_BatchTrans_GET()
        {
            // Retrieve CR batch transactions
            CR_BatchTransQuery req = new CR_BatchTransQuery();
            QueryResponse<CR_BatchTrans> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/CR_Main"
        [Test]
        public async Task CR_Main_GET()
        {
            // Retrieve CR main records
            CR_MainQuery req = new CR_MainQuery();
            QueryResponse<CR_Main> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/CR_Warehouse"
        [Test]
        public async Task CR_Warehouse_GET()
        {
            // Retrieve CR warehouse records
            CR_WarehouseQuery req = new CR_WarehouseQuery();
            QueryResponse<CR_Warehouse> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/v_Jiwa_CreditorPurchaseInformation"
        [Test]
        public async Task v_Jiwa_CreditorPurchaseInformation_GET()
        {
            // Retrieve creditor purchase information records
            v_Jiwa_CreditorPurchaseInformationQuery req = new v_Jiwa_CreditorPurchaseInformationQuery();
            QueryResponse<v_Jiwa_CreditorPurchaseInformation> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/v_Jiwa_CreditorPurchases"
        [Test]
        public async Task v_Jiwa_CreditorPurchases_GET()
        {
            // Retrieve creditor purchase header records
            AutoQueryv_Jiwa_CreditorPurchasesRouteQuery req = new AutoQueryv_Jiwa_CreditorPurchasesRouteQuery();
            QueryResponse<v_Jiwa_CreditorPurchases> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/DB_Categories"
        [Test]
        public async Task DB_Categories_GET()
        {
            // Retrieve DB categories
            DB_CategoriesQuery req = new DB_CategoriesQuery();
            QueryResponse<DB_Categories> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/DB_Classification"
        [Test]
        public async Task DB_Classification_GET()
        {
            // Retrieve DB classification records
            DB_ClassificationQuery req = new DB_ClassificationQuery();
            QueryResponse<DB_Classification> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/DB_DebtorSystems"
        [Test]
        public async Task DB_DebtorSystems_GET()
        {
            // Retrieve DB debtor systems
            DB_DebtorSystemsQuery req = new DB_DebtorSystemsQuery();
            QueryResponse<DB_DebtorSystems> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/DB_DebtorSystemTemplates"
        [Test]
        public async Task DB_DebtorSystemTemplates_GET()
        {
            // Retrieve DB debtor system templates
            DB_DebtorSystemTemplatesQuery req = new DB_DebtorSystemTemplatesQuery();
            QueryResponse<DB_DebtorSystemTemplates> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/DB_Main"
        [Test]
        public async Task DB_Main_GET()
        {
            // Retrieve DB main records
            DB_MainQuery req = new DB_MainQuery();
            QueryResponse<DB_Main> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/DB_PricingGroups"
        [Test]
        public async Task DB_PricingGroups_GET()
        {
            // Retrieve DB pricing groups
            DB_PricingGroupsQuery req = new DB_PricingGroupsQuery();
            QueryResponse<DB_PricingGroups> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/DebtorList"
        [Test]
        public async Task DebtorList_GET()
        {
            // Retrieve the debtor list
            v_Jiwa_Debtor_ListQuery req = new v_Jiwa_Debtor_ListQuery();
            QueryResponse<v_Jiwa_Debtor_List> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/DebtorTransactionList"
        [Test]
        public async Task DebtorTransactionList_GET()
        {
            // Retrieve the debtor transaction list
            v_Jiwa_Debtor_Transactions_ListQuery req = new v_Jiwa_Debtor_Transactions_ListQuery();
            QueryResponse<v_Jiwa_Debtor_Transactions_List> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/EM_Main"
        [Test]
        public async Task EM_Main_GET()
        {
            // Retrieve EM main records
            EM_MainQuery req = new EM_MainQuery();
            QueryResponse<EM_Main> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/FR_Carriers"
        [Test]
        public async Task FR_Carriers_GET()
        {
            // Retrieve FR carriers
            FR_CarriersQuery req = new FR_CarriersQuery();
            QueryResponse<FR_Carriers> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/FX_Currency"
        [Test]
        public async Task FX_Currency_GET()
        {
            // Retrieve FX currency records
            FX_CurrencyQuery req = new FX_CurrencyQuery();
            QueryResponse<FX_Currency> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/FX_CurrencyRates"
        [Test]
        public async Task FX_CurrencyRates_GET()
        {
            // Retrieve FX currency rates
            FX_CurrencyRatesQuery req = new FX_CurrencyRatesQuery();
            QueryResponse<FX_CurrencyRates> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/GL_Category"
        [Test]
        public async Task GL_Category_GET()
        {
            // Retrieve GL categories
            GL_CategoryQuery req = new GL_CategoryQuery();
            QueryResponse<GL_Category> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/GL_Sets"
        [Test]
        public async Task GL_Sets_GET()
        {
            // Retrieve GL sets
            GL_SetsQuery req = new GL_SetsQuery();
            QueryResponse<GL_Sets> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/GR_ReceivalDocuments"
        [Test]
        public async Task GR_ReceivalDocuments_GET()
        {
            // Retrieve GR receival documents
            v_GR_ReceivalDocumentsQuery req = new v_GR_ReceivalDocumentsQuery();
            QueryResponse<v_GR_ReceivalDocuments> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/HR_Departments"
        [Test]
        public async Task HR_Departments_GET()
        {
            // Retrieve HR departments
            HR_DepartmentsQuery req = new HR_DepartmentsQuery();
            QueryResponse<HR_Departments> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/INSOHWithBinLocations"
        [Test]
        public async Task INSOHWithBinLocations_GET()
        {
            // Retrieve inventory SOH with bin locations
            v_IN_SOHWithBinLocationsQuery req = new v_IN_SOHWithBinLocationsQuery();
            QueryResponse<v_IN_SOHWithBinLocations> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/InventoryBarCodeList"
        [Test]
        public async Task InventoryBarCodeList_GET()
        {
            // Retrieve the inventory barcode list
            v_InventoryBarCodeListQuery req = new v_InventoryBarCodeListQuery();
            QueryResponse<v_InventoryBarCodeList> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/InventoryItemList"
        [Test]
        public async Task InventoryItemList_GET()
        {
            // Retrieve the inventory item list
            v_Jiwa_Inventory_Item_ListQuery req = new v_Jiwa_Inventory_Item_ListQuery();
            QueryResponse<v_Jiwa_Inventory_Item_List> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/IN_AttributeGroupTemplate"
        [Test]
        public async Task IN_AttributeGroupTemplate_GET()
        {
            // Retrieve IN attribute group templates
            IN_AttributeGroupTemplateQuery req = new IN_AttributeGroupTemplateQuery();
            QueryResponse<IN_AttributeGroupTemplate> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/IN_BinLocationLookup"
        [Test]
        public async Task IN_BinLocationLookup_GET()
        {
            // Retrieve IN bin location lookup records
            IN_BinLocationLookupQuery req = new IN_BinLocationLookupQuery();
            QueryResponse<IN_BinLocationLookup> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/IN_Categories"
        [Test]
        public async Task IN_Categories_GET()
        {
            // Retrieve IN categories
            IN_CategoriesQuery req = new IN_CategoriesQuery();
            QueryResponse<IN_Categories> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/IN_Classification"
        [Test]
        public async Task IN_Classification_GET()
        {
            // Retrieve IN classification records
            IN_ClassificationQuery req = new IN_ClassificationQuery();
            QueryResponse<IN_Classification> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/IN_Logical"
        [Test]
        public async Task IN_Logical_GET()
        {
            // Retrieve IN logical warehouse records
            IN_LogicalQuery req = new IN_LogicalQuery();
            QueryResponse<IN_Logical> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/IN_Main"
        [Test]
        public async Task IN_Main_GET()
        {
            // Retrieve IN main inventory records
            IN_MainQuery req = new IN_MainQuery();
            QueryResponse<IN_Main> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/IN_Physical"
        [Test]
        public async Task IN_Physical_GET()
        {
            // Retrieve IN physical warehouse records
            IN_PhysicalQuery req = new IN_PhysicalQuery();
            QueryResponse<IN_Physical> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/IN_Region"
        [Test]
        public async Task IN_Region_GET()
        {
            // Retrieve IN region records
            IN_RegionQuery req = new IN_RegionQuery();
            QueryResponse<IN_Region> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/IN_SOH"
        [Test]
        public async Task IN_SOH_GET()
        {
            // Retrieve IN stock on hand records
            IN_SOHQuery req = new IN_SOHQuery();
            QueryResponse<IN_SOH> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/IN_Transfer"
        [Test]
        public async Task IN_Transfer_GET()
        {
            // Retrieve IN transfer records
            IN_TransferQuery req = new IN_TransferQuery();
            QueryResponse<IN_Transfer> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/IN_WarehouseSOH"
        [Test]
        public async Task IN_WarehouseSOH_GET()
        {
            // Retrieve IN warehouse SOH records
            IN_WarehouseSOHQuery req = new IN_WarehouseSOHQuery();
            QueryResponse<IN_WarehouseSOH> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/JobCostingSelection"
        [Test]
        public async Task JobCostingSelection_GET()
        {
            // Retrieve job costing selection records
            vJobCostingSelectionQuery req = new vJobCostingSelectionQuery();
            QueryResponse<vJobCostingSelection> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/OR/BM_Main"
        [Test]
        public async Task OR_BM_Main_GET()
        {
            // Retrieve OR BM main records
            BM_MainORQuery req = new BM_MainORQuery();
            QueryResponse<BM_MainOR> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/OR/BM_WorkOrder"
        [Test]
        public async Task OR_BM_WorkOrder_GET()
        {
            // Retrieve OR BM work orders
            BM_WorkOrderORQuery req = new BM_WorkOrderORQuery();
            QueryResponse<BM_WorkOrderOR> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/OR/CR_Main"
        [Test]
        public async Task OR_CR_Main_GET()
        {
            // Retrieve OR CR main records
            CR_MainORQuery req = new CR_MainORQuery();
            QueryResponse<CR_MainOR> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/OR/CR_Warehouse"
        [Test]
        public async Task OR_CR_Warehouse_GET()
        {
            // Retrieve OR CR warehouse records
            CR_WarehouseORQuery req = new CR_WarehouseORQuery();
            QueryResponse<CR_WarehouseOR> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/OR/CR_BatchTrans"
        [Test]
        public async Task OR_CR_BatchTrans_GET()
        {
            // Retrieve OR CR batch transactions
            CR_BatchTransORQuery req = new CR_BatchTransORQuery();
            QueryResponse<CR_BatchTransOR> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/OR/v_Jiwa_CreditorSummary"
        [Test]
        public async Task OR_v_Jiwa_CreditorSummary_GET()
        {
            // Retrieve OR creditor summary records
            v_Jiwa_CreditorSummaryORQuery req = new v_Jiwa_CreditorSummaryORQuery();
            QueryResponse<v_Jiwa_CreditorSummaryOR> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/OR/v_Jiwa_CreditorPurchaseInformation"
        [Test]
        public async Task OR_v_Jiwa_CreditorPurchaseInformation_GET()
        {
            // Retrieve OR creditor purchase information records
            v_Jiwa_CreditorPurchaseInformationORQuery req = new v_Jiwa_CreditorPurchaseInformationORQuery();
            QueryResponse<v_Jiwa_CreditorPurchaseInformationOR> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/OR/v_Jiwa_CreditorPurchases"
        [Test]
        public async Task OR_v_Jiwa_CreditorPurchases_GET()
        {
            // Retrieve OR creditor purchase header records
            v_Jiwa_CreditorPurchasesORQuery req = new v_Jiwa_CreditorPurchasesORQuery();
            QueryResponse<v_Jiwa_CreditorPurchasesOR> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/OR/DB_Main"
        [Test]
        public async Task OR_DB_Main_GET()
        {
            // Retrieve OR DB main records
            DB_MainORQuery req = new DB_MainORQuery();
            QueryResponse<DB_MainOR> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/OR/DB_DebtorSystems"
        [Test]
        public async Task OR_DB_DebtorSystems_GET()
        {
            // Retrieve OR DB debtor systems
            DB_DebtorSystemsORQuery req = new DB_DebtorSystemsORQuery();
            QueryResponse<DB_DebtorSystemsOR> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

        #region "/Queries/OR/DB_DebtorSystemTemplates"
        [Test]
        public async Task OR_DB_DebtorSystemTemplates_GET()
        {
            // Retrieve OR DB debtor system templates
            DB_DebtorSystemTemplatesORQuery req = new DB_DebtorSystemTemplatesORQuery();
            QueryResponse<DB_DebtorSystemTemplatesOR> res = await Client.GetAsync(req);
            Assert.That(LastHttpStatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Results, Is.Not.Null);
        }
        #endregion

    }
}
