using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.ViewModels.Dashboard;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces
{
    public interface IReceptionRepository : IRepository<ReceptionNew>
    {
        Task<ReceptionNew?> GetReception(int sourceLabId, string chrSourceReceptId, int targetLabId);
        Task<List<ReceptionNew>?> GetReceptionsByKeys(List<ReceptionKeyViewModel> keys);
        Task<ReceptionNew?> GetReceptionByReceptId(int sourceLabId, string chrSourceReceptId);
        List<ReceptTestNew> GetReceptTests(int sourceLabId, string chrSourceReceptId, int intTargetLabId, string testName);

        void AddRangeReceptions(List<ReceptionNew> receptions);
        void AddRangeReceptTestNew(List<ReceptTestNew> ReceptTestsNew);
        void AddRangeReceptTestP(List<ReceptTestP> ReceptTestsPs);
        void AddRangeReceptTestS(List<ReceptTestS> ReceptTestSs);

        List<ReceptTestComparisonResult> GetReceptTestComparisonResults(TestPriceComparisonFilterQuery query);

        List<ReceptionViewModel> GetReceptionsByFilter(ReceiveReceptionGroupFilterQuery filter);

        List<ReceptionViewModel> GetReportingItemsByFilter(ReceiveGroupReportingItemFilter filter);

        void UpdateTargetReceptId(List<UpdateTargetReceptIdCommand> command);

        /// <summary>
        /// Clears destination reception numbers for tests without result (empty vchResult).
        /// </summary>
        int ClearReceiverReceptions(List<ClearReceiverReceptionItem> items);

        void UpdateReceptionId(List<UpdateReceptionVM> viewModel);

        List<AddReceptTestNewResponse>? UpdateResult(List<UpdateReportingItemsCommand> reportingItems);

        void UpdateResultReceiveDate(List<UpdateReportingItemsCommand> reportingItems);


        Task<LabReceiverRangeDetailViewModel?> GetRangeDetail(long targetLabId, long targetRangeDetialId);
        void AddRangeDetail(LabReceiverRangeDetail rangeDetail);
        void UpdateRangeDetail(LabReceiverRangeDetail item);
        void UpdateTracking(long id, int tracking);


        List<ReceptionNew> GetReceptionsByReceptTest(int sourceLabId);

        List<ReceptionNew> GetReceptionsByFilterReceptTest(List<ReceptTestNew> receptTests);

        

        ReceptTestNew? GetReceptTestById(long id);
        void DeleteReceptTest(ReceptTestNew receptTest);
        void RejectTest(RejectReceptTest receptTest);
        List<string?> GetRejectCount(int labcode);

        Task<AdminReceptionDashboardStatsDto> GetAdminReceptionDashboardStatsAsync(
            int? labCode = null,
            AdminReceptionDashboardSection? section = null);
    }
}

