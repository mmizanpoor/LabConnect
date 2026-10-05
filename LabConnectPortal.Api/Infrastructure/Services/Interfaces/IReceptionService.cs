using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Dashboard;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IReceptionService
{
    Task<OperationResult> PostReceptionGroupAsync(List<ReceptionViewModel> list);
    Task<OperationResult> GetReceptTestComparisonResultsAsync(TestPriceComparisonFilterQuery query);
    Task<OperationResult> GetReceptionsByFilterAsync(ReceiveReceptionGroupFilterQuery filter);
    Task<OperationResult> GetReportingByReceptIdAsync(GetReceptionQuery query);
    Task<OperationResult> UpdateTargetReceptIdAsync(List<UpdateTargetReceptIdCommand> command);
    Task<OperationResult> ClearReceiverReceptionsAsync(ClearReceiverReceptionCommand command);
    Task<OperationResult> UpdateResultAsync(List<UpdateReportingItemsCommand> reportingItems);
    Task<OperationResult> GetReportingItemsByFilterAsync(ReceiveGroupReportingItemFilter filter);
    Task<OperationResult> UpdateResultReceiveDateAsync(List<UpdateReportingItemsCommand> reportingItems);
    Task<OperationResult> UpdateTrackingAsync(ChangeTrackingCommand model);
    Task<OperationResult> GetLabDetailAsync(int labcode);
    Task<OperationResult> GetLabsAsync(List<int> labcodes);
    Task<OperationResult> GetSourcesLabNameAsync(int labcode);
    Task<OperationResult> GetTargetsLabNameAsync(int labcode);
    Task<OperationResult> RemoveReceptTestAsync(long receptTestId);
    Task<OperationResult> RejectReceptTestsAsync(RejectReceptTestCommand model);
    Task<OperationResult> GetRejectCountAsync(int labCode);
    Task<OperationResult> GetAllSRLabs(int labCode, bool incoming);
    Task<OperationResult<AdminReceptionDashboardStatsDto>> GetAdminReceptionDashboardStatsAsync(
        Guid userId,
        int? labCode = null,
        AdminReceptionDashboardSection? section = null);
}
