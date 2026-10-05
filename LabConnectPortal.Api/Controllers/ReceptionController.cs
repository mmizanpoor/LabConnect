using System.Security.Claims;
using System.Text.RegularExpressions;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class ReceptionController(IReceptionService receptionService) : ControllerBase
{
    [HttpPost("PostReceptionGroup")]
    public async Task<OperationResult> PostReceptionGroup(List<ReceptionViewModel> list)
        => await receptionService.PostReceptionGroupAsync(list);

    [HttpPost("GetReceptions")]
    public async Task<OperationResult> GetReceptions(ReceiveReceptionGroupFilterQuery filter)
        => await receptionService.GetReceptionsByFilterAsync(filter);

    [HttpPost("GetReceptTestComparisonResults")]
    public async Task<OperationResult> GetReceptTestComparisonResults(TestPriceComparisonFilterQuery filter)
    => await receptionService.GetReceptTestComparisonResultsAsync(filter);

    [HttpPost("GetReportingByReceptId")]
    public async Task<OperationResult> GetReportingByReceptId([FromBody] GetReceptionQuery query)
        => await receptionService.GetReportingByReceptIdAsync(query);

    [HttpPost("UpdateTargetReceptId")]
    public async Task<OperationResult> UpdateTargetReceptId([FromBody] List<UpdateTargetReceptIdCommand> command)
        => await receptionService.UpdateTargetReceptIdAsync(command);

    [HttpPost("ClearReceiverReceptions")]
    [Authorize]
    public async Task<OperationResult> ClearReceiverReceptions([FromBody] ClearReceiverReceptionCommand command)
        => await receptionService.ClearReceiverReceptionsAsync(command);

    [HttpPost("UpdateResultAsync")]
    public async Task<OperationResult> UpdateResultAsync(List<UpdateReportingItemsCommand> reportingItems)
        => await receptionService.UpdateResultAsync(reportingItems);

    [HttpPost("ReceiveReportingItemsAsync")]
    public async Task<OperationResult> ReceiveReportingItemsAsync(ReceiveGroupReportingItemFilter filter)
        => await receptionService.GetReportingItemsByFilterAsync(filter);

    [HttpPost("UpdateResultReceiveDate")]
    public async Task<OperationResult> UpdateResultReceiveDate(List<UpdateReportingItemsCommand> reportingItems)
        => await receptionService.UpdateResultReceiveDateAsync(reportingItems);

    [HttpPost("UpdateTracking")]
    public async Task<OperationResult> UpdateTracking(ChangeTrackingCommand model)
        => await receptionService.UpdateTrackingAsync(model);

    [HttpGet("GetLabDetailAsync")]
    public async Task<OperationResult> GetLabDetailAsync(int labcode)
        => await receptionService.GetLabDetailAsync(labcode);

    [HttpPost("GetLabs")]
    public async Task<OperationResult> GetLabs(List<int> labcodes)
        => await receptionService.GetLabsAsync(labcodes);

    [HttpGet("GetSourcesLabName")]
    public async Task<OperationResult> GetSourcesLabName(int labcode)
        => await receptionService.GetSourcesLabNameAsync(labcode);

    [HttpGet("GetTargetsLabName")]
    public async Task<OperationResult> GetTargetsLabName(int labcode)
        => await receptionService.GetTargetsLabNameAsync(labcode);

    [HttpGet("RemoveReceptTest")]
    public async Task<OperationResult> RemoveReceptTest(long receptTestId)
        => await receptionService.RemoveReceptTestAsync(receptTestId);

    [HttpPost("RejectReceptTests")]
    public async Task<OperationResult> RejectReceptTests(RejectReceptTestCommand model)
        => await receptionService.RejectReceptTestsAsync(model);

    [HttpGet("RejectCount")]
    public async Task<OperationResult> RejectCount(int labCode)
        => await receptionService.GetRejectCountAsync(labCode);

    [HttpGet("GetAllSRLabs")]
    public async Task<OperationResult> GetAllSRLabs(int labCode, bool incoming)
        => await receptionService.GetAllSRLabs(labCode, incoming);

    [HttpGet("GetAdminDashboardStats")]
    [Authorize]
    public async Task<OperationResult> GetAdminDashboardStats(int? labCode, AdminReceptionDashboardSection? section)
        => await receptionService.GetAdminReceptionDashboardStatsAsync(GetUserId(), labCode, section);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
