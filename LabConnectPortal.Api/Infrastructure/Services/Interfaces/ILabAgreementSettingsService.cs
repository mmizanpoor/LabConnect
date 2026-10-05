using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.LabAgreementSettings;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ILabAgreementSettingsService
{
    Task<OperationResult<LabAgreementSettingsDto>> GetMineAsync(Guid userId);

    Task<OperationResult<LabAgreementSettingsDto>> GetByLabCodeNewAsync(int labCodeNew);

    Task<OperationResult<LabAgreementSettingsDto>> UpsertAsync(Guid userId, UpsertLabAgreementSettingsCommand command);

    Task<OperationResult<LabAgreementSettingsDto>> UploadFileAsync(
        Guid userId,
        LabAgreementSettingsFileKind kind,
        IFormFile file);

    Task<(Stream? Stream, string? ContentType, string? Error)> GetFileAsync(
        Guid userId,
        LabAgreementSettingsFileKind kind);
}
