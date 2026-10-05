using LabConnectPortal.Domain;
using LabConnectPortal.Infra.ViewModels;

namespace LabConnectPortal.Infra.Service.Interfaces
{
    public interface ILabAgreement : IRepository<LabAgreement>
    {
        Task<OperationResult<LabAgreementCommand?>> InsertLabAgreement(LabAgreementCommand command);

        Task<LabAgreementCommand?> GetLabAgreementById(long id);

        Task<LabAgreementCommand?> GetLabAgreementByLabCode(int primaryLabCode,int receiverLabCode);

        IReadOnlyList<LabAgreementCommand> GetLabAgreements(GetLabAgreementQuery query);

        void ReceiverActionSeen(LabAgreementCommand command);

        void ReceiverReject(LabAgreementCommand command);

        void ReceiverSign(LabAgreementCommand command);

        void PrimarySign(LabAgreementCommand command);

        void AddTestPrices(LabAgreementCommand command);

        void PrimaryCanceledSuspendAgreement(LabAgreementCommand command);

        void ReceiverCanceledSuspendAgreement(LabAgreementCommand command);

        void PrimarySuspendAgreement(LabAgreementCommand command);

        void ReceiverSuspendAgreement(LabAgreementCommand command);

        void PrimaryTerminationAgreement(LabAgreementCommand command);

        void ReceiverTerminationAgreement(LabAgreementCommand command);
    }
}
