using LabConnectPortal.Domain;
using LabConnectPortal.Infra.ViewModels;

namespace LabConnectPortal.Infra.Service.Interfaces
{
    public interface IReception : IRepository<ReceptionNew>
    {
        Task<ReceptionNew?> GetReception(int sourceLabId, string chrSourceReceptId, int targetLabId);
        Task<ReceptionNew?> GetReceptionByReceptId(int sourceLabId, string chrSourceReceptId);
        List<ReceptTestNew> GetReceptTests(int sourceLabId, string chrSourceReceptId, int intTargetLabId, string testName);

        void AddRangeReceptions(List<ReceptionNew> receptions);
        void AddRangeReceptTestNew(List<ReceptTestNew> ReceptTestsNew);
        void AddRangeReceptTestP(List<ReceptTestP> ReceptTestsPs);
        void AddRangeReceptTestS(List<ReceptTestS> ReceptTestSs);

        List<ReceptionViewModel> GetReceptionsByFilter(ReceiveReceptionGroupFilterQuery filter);

        List<ReceptionViewModel> GetReportingItemsByFilter(ReceiveGroupReportingItemFilter filter);

        void UpdateTargetReceptId(List<UpdateTargetReceptIdCommand> command);

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
    }
}
