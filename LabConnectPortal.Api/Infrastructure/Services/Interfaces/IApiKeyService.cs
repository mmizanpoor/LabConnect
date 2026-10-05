using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ApiKey;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IApiKeyService
{
    Task<OperationResult<List<ApiKeyDto>>> GetMineAsync(Guid userId);
    Task<OperationResult<ApiKeyDto>> CreateAsync(Guid userId, CreateApiKeyCommand command);
    Task<OperationResult<ApiKeyDto>> UpdateMineAsync(Guid userId, UpdateApiKeyCommand command);
    Task<OperationResult> DeleteMineAsync(Guid userId, Guid id);
    Task<OperationResult<List<ApiKeyDto>>> GetAllAsync();
    Task<OperationResult<ApiKeyDto>> UpdateAsync(UpdateApiKeyCommand command);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<ApiKeyAuthorizationDto>> AuthorizeAsync(string? rawKey, ApiKeyPermission permission);
}
