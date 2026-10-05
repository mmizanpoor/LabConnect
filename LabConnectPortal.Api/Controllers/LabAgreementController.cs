using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;
using LabConnectPortal.Api.Infrastructure.ViewModels.SpecialOffer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class LabAgreementController(
    ILabAgreementService labAgreementService,
    ILabAgreementPortalService labAgreementPortalService,
    IAgreementExpiryNotificationService agreementExpiryNotificationService,
    INotificationService notificationService) : ControllerBase
{
    [HttpPost("IsExistLabAgreement")]
    public async Task<OperationResult> IsExistLabAgreement([FromBody] GetExistLabAgreementQuery query)
    => await labAgreementService.IsExistLabAgreement(query);

    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] LabAgreementCommand labAgreement)
        => await labAgreementService.CreateAsync(labAgreement);

    [HttpPost("UpdateLabAgreement")]
    public async Task<OperationResult> UpdateLabAgreement([FromBody] LabAgreementCommand command)
        => await labAgreementService.UpdateLabAgreementAsync(command);

    [HttpPost("UpdateLaboratoryAgreementState")]
    public async Task<OperationResult> UpdateLaboratoryAgreementState([FromBody] UpdateLaboratoryAgreementStateCommand command)
        => await labAgreementService.UpdateLaboratoryAgreementStateAsync(command);

    [HttpGet("RemoveLabAgreement")]
    public async Task<OperationResult> RemoveLabAgreement(long id)
        => await labAgreementService.RemoveLabAgreementAsync(id);

    [HttpGet("GetLabAgreementById")]
    public async Task<OperationResult> GetLabAgreementById(int id)
        => await labAgreementService.GetLabAgreementByIdAsync(id);

    [HttpGet("GetLabAgreement")]
    public async Task<OperationResult> GetLabAgreement(long id)
        => await labAgreementService.GetLabAgreementAsync(id);

    [HttpPost("GetLabAgreementByLabCode")]
    public async Task<OperationResult> GetLabAgreementByLabCode([FromBody] GetLabAgreementByLabCodeQuery query)
        => await labAgreementService.GetLabAgreementByLabCodeAsync(query);

    [HttpPost("GetLabAgreements")]
    public async Task<OperationResult> GetLabAgreements([FromBody] GetLabAgreementQuery query)
        => await labAgreementService.GetLabAgreementsAsync(query);

    [HttpPost("GetAllLabAgreements")]
    public async Task<OperationResult> GetAllLabAgreements([FromBody] GetAllLabAgreementQuery query)
        => await labAgreementService.GetAllLabAgreementsAsync(query);

    [HttpPost("ReceiverSign")]
    public async Task<OperationResult> ReceiverSign([FromBody] LabAgreementCommand command)
        => await labAgreementService.ReceiverSignAsync(command);

    [HttpPost("ReceiverSeen")]
    public async Task<OperationResult> ReceiverSeen([FromBody] LabAgreementCommand command)
        => await labAgreementService.ReceiverSeenAsync(command);

    [HttpPost("ReceiverReject")]
    public async Task<OperationResult> ReceiverReject([FromBody] LabAgreementCommand command)
        => await labAgreementService.ReceiverRejectAsync(command);

    [HttpPost("PrimarySign")]
    public async Task<OperationResult> PrimarySign([FromBody] LabAgreementCommand command)
        => await labAgreementService.PrimarySignAsync(command);

    [HttpPost("AddTestPrices")]
    public async Task<OperationResult> AddTestPrices([FromBody] LabAgreementCommand command)
        => await labAgreementService.AddTestPricesAsync(command);

    [HttpPost("PrimarySuspendAgreement")]
    public async Task<OperationResult> PrimarySuspendAgreement([FromBody] LabAgreementCommand command)
        => await labAgreementService.PrimarySuspendAgreementAsync(command);

    [HttpPost("PrimaryTerminationAgreement")]
    public async Task<OperationResult> PrimaryTerminationAgreement([FromBody] LabAgreementCommand command)
        => await labAgreementService.PrimaryTerminationAgreementAsync(command);

    [HttpPost("ReceiverSuspendAgreement")]
    public async Task<OperationResult> ReceiverSuspendAgreement([FromBody] LabAgreementCommand command)
        => await labAgreementService.ReceiverSuspendAgreementAsync(command);

    [HttpPost("ReceiverTerminationAgreement")]
    public async Task<OperationResult> ReceiverTerminationAgreement([FromBody] LabAgreementCommand command)
        => await labAgreementService.ReceiverTerminationAgreementAsync(command);

    [HttpPost("ReceiverCanceledSuspendAgreement")]
    public async Task<OperationResult> ReceiverCanceledSuspendAgreement([FromBody] LabAgreementCommand command)
        => await labAgreementService.ReceiverCanceledSuspendAgreementAsync(command);

    [HttpPost("PrimaryCanceledSuspendAgreement")]
    public async Task<OperationResult> PrimaryCanceledSuspendAgreement([FromBody] LabAgreementCommand command)
        => await labAgreementService.PrimaryCanceledSuspendAgreementAsync(command);

    [HttpPost("ReceiverCanceledTerminationAgreement")]
    public async Task<OperationResult> ReceiverCanceledTerminationAgreement([FromBody] LabAgreementCommand command)
        => await labAgreementService.ReceiverCanceledTerminationAgreementAsync(command);

    [HttpPost("PrimaryCanceledTerminationAgreement")]
    public async Task<OperationResult> PrimaryCanceledTerminationAgreement([FromBody] LabAgreementCommand command)
        => await labAgreementService.PrimaryCanceledTerminationAgreementAsync(command);

    [HttpPost("GetActiveContractLaboratory")]
    public async Task<OperationResult> GetActiveContractLaboratory([FromBody] GetActiveContractLaboratoryQuery query)
        => await labAgreementService.GetActiveContractLaboratory(query);

    [HttpPost("CreateFromPortal")]
    [Authorize]
    public async Task<OperationResult> CreateFromPortal([FromBody] CreateLabAgreementFromPortalCommand command)
        => await labAgreementPortalService.CreateFromPortalAsync(GetUserId(), command);

    [HttpGet("GetStats")]
    [AuthorizeUserTypes(UserType.Administrator)]
    public async Task<OperationResult> GetStats()
        => await labAgreementService.GetGlobalStatsAsync();

    [HttpGet("GetStatsForCurrentLab")]
    [Authorize]
    public async Task<OperationResult> GetStatsForCurrentLab()
        => await labAgreementService.GetStatsForCurrentLabAsync(GetUserId());

    [HttpPost("RunExpiryNotifications")]
    [AuthorizeUserTypes(UserType.Administrator)]
    public async Task<OperationResult> RunExpiryNotifications()
        => await agreementExpiryNotificationService.RunAsync();

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
