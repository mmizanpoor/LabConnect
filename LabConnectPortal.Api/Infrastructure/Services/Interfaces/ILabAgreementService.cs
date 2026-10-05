using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ILabAgreementService
{
    Task<OperationResult> IsExistLabAgreement(GetExistLabAgreementQuery query);
    Task<OperationResult> CreateAsync(LabAgreementCommand command);
    Task<OperationResult> RemoveLabAgreementAsync(long id);
    Task<OperationResult> GetLabAgreementByIdAsync(int id);
    Task<OperationResult> GetLabAgreementAsync(long id);
    Task<OperationResult> GetLabAgreementByLabCodeAsync(GetLabAgreementByLabCodeQuery query);
    Task<OperationResult> GetLabAgreementsAsync(GetLabAgreementQuery query);
    Task<OperationResult> GetAllLabAgreementsAsync(GetAllLabAgreementQuery query);
    Task<OperationResult> ReceiverSignAsync(LabAgreementCommand command);
    Task<OperationResult> ReceiverSeenAsync(LabAgreementCommand command);
    Task<OperationResult> ReceiverRejectAsync(LabAgreementCommand command);
    Task<OperationResult> PrimarySignAsync(LabAgreementCommand command);
    Task<OperationResult> AddTestPricesAsync(LabAgreementCommand command);
    Task<OperationResult> PrimarySuspendAgreementAsync(LabAgreementCommand command);
    Task<OperationResult> PrimaryTerminationAgreementAsync(LabAgreementCommand command);
    Task<OperationResult> ReceiverSuspendAgreementAsync(LabAgreementCommand command);
    Task<OperationResult> ReceiverTerminationAgreementAsync(LabAgreementCommand command);
    Task<OperationResult> ReceiverCanceledSuspendAgreementAsync(LabAgreementCommand command);
    Task<OperationResult> PrimaryCanceledSuspendAgreementAsync(LabAgreementCommand command);
    Task<OperationResult> PrimaryCanceledTerminationAgreementAsync(LabAgreementCommand command);
    Task<OperationResult> ReceiverCanceledTerminationAgreementAsync(LabAgreementCommand command);
    Task<OperationResult> UpdateLabAgreementAsync(LabAgreementCommand command);
    Task<OperationResult> UpdateLaboratoryAgreementStateAsync(UpdateLaboratoryAgreementStateCommand command);
    Task<OperationResult<LabAgreementStatsDto>> GetGlobalStatsAsync();
    Task<OperationResult<LabAgreementLabStatsDto>> GetStatsForCurrentLabAsync(Guid userId);
    Task<OperationResult<ActiveContractLaboratoryResult>> GetActiveContractLaboratory(GetActiveContractLaboratoryQuery query);
}
