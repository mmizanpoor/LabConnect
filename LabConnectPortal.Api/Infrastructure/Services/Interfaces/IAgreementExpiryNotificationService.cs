using LabConnectPortal.Api.Infrastructure.ViewModels;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IAgreementExpiryNotificationService
{
    Task<OperationResult> RunAsync(CancellationToken cancellationToken = default);
}
