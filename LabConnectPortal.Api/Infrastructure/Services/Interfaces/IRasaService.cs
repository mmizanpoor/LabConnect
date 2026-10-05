using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Rasa;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IRasaService
{
    Task<OperationResult> GetOAuthTokenAsync(CancellationToken cancellationToken = default);

    Task<OperationResult> GetContractListAsync(
        RasaContractListCommand command,
        CancellationToken cancellationToken = default);

    Task<OperationResult> ValidationDinaAsync(
        RasaClaimInfoModelCommand command,
        CancellationToken cancellationToken = default);

    Task<OperationResult> CancelDinaAsync(
        RasaCancelDinaCommand command,
        CancellationToken cancellationToken = default);

    //Task<OperationResult<TResponse>> PostAsync<TResponse>(
    //    string endpoint,
    //    object data,
    //    CancellationToken cancellationToken = default);
}
