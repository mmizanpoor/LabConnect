using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces
{
    public interface ILabAgreementRepository : IRepository<LabAgreement>
    {
        bool IsExistLabAgreement(GetExistLabAgreementQuery query);

        Task<OperationResult<LabAgreementCommand?>> InsertLabAgreement(LabAgreementCommand command);

        Task<LabAgreementCommand?> GetLabAgreementById(long id);

        Task<IReadOnlyList<LabAgreementTestPriceCommand>> GetMergedTestPricesAsync(long agreementId);

        Task<LabAgreementCommand?> GetLabAgreement(long id);

        Task<LabAgreementCommand?> GetLabAgreementByLabCode(int primaryLabCode, int receiverLabCode);

        IReadOnlyList<LabAgreementCommand> GetLabAgreements(GetLabAgreementQuery query);

        IReadOnlyList<LabAgreementCommand> GetAllLabAgreements(GetAllLabAgreementQuery query);

        void ReceiverActionSeen(LabAgreementCommand command);

        void ReceiverReject(LabAgreementCommand command);

        void ReceiverSign(LabAgreementCommand command);

        void PrimarySign(LabAgreementCommand command);

        void AddTestPrices(LabAgreementCommand command);

        void PrimaryCanceledSuspendAgreement(LabAgreementCommand command);

        void ReceiverCanceledSuspendAgreement(LabAgreementCommand command);

        void PrimaryCanceledTerminationAgreement(LabAgreementCommand command);

        void ReceiverCanceledTerminationAgreement(LabAgreementCommand command);

        void PrimarySuspendAgreement(LabAgreementCommand command);

        void ReceiverSuspendAgreement(LabAgreementCommand command);

        void PrimaryTerminationAgreement(LabAgreementCommand command);

        void ReceiverTerminationAgreement(LabAgreementCommand command);

        bool UpdateLabAgreement(LabAgreementCommand command);

        bool UpdateLaboratoryAgreementState(long id, int laboratoryAgreementState);

        Task<LabAgreementStatsDto> GetGlobalStatsAsync();

        Task<LabAgreementLabStatsDto> GetStatsByLabCodeAsync(int labCodeNew);

        Task DeleteLabAgreementTreeAsync(long id);

        Task<ActiveContractLaboratoryResult> GetActiveContractLaboratory(GetActiveContractLaboratoryQuery query);
    }
}

