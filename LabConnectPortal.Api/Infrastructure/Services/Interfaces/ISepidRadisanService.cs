using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ISepidRadisanService
{
    Task<OperationResult> GetInsurancersListAsync(
        SepidRadisanGetInsurancersCommand command,
        CancellationToken cancellationToken = default);

    Task<OperationResult> GetInquiryPoliciesAsync(
        SepidRadisanInquiryCommand command,
        CancellationToken cancellationToken = default);

    Task<OperationResult> GetInsuredInfoAsync(
        SepidRadisanInsuredInfoCommand command,
        CancellationToken cancellationToken = default);

    Task<OperationResult> PreCheckIntroductionAsync(
        SepidRadisanPreCheckIntroductionCommand command,
        CancellationToken cancellationToken = default);

    Task<OperationResult> CreateIntroductionAsync(
        SepidRadisanCreateIntroductionCommand command,
        CancellationToken cancellationToken = default);

    Task<OperationResult> PutDischargeAsync(
        SepidRadisanPutDischargeCommand command,
        CancellationToken cancellationToken = default);

    Task<OperationResult> DeleteIntroductionAsync(
        SepidRadisanDeleteIntroductionCommand command,
        CancellationToken cancellationToken = default);

    Task<OperationResult> AppendAttachmentAsync(
        SepidRadisanAppendAttachmentCommand command,
        CancellationToken cancellationToken = default);
}
