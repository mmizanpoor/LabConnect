using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ProductResumeApplication;
using LabConnectPortal.Api.Infrastructure.ViewModels.UserProfile;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IProductResumeApplicationService
{
    Task<OperationResult<ProductResumeApplicationDto>> SubmitAsync(Guid userId, SubmitProductResumeCommand command);
    Task<OperationResult<List<ProductResumeApplicationDto>>> GetMyApplicationsAsync(Guid userId);
    Task<OperationResult<ProductResumeApplicationsPageDto>> GetForProductAsync(
        Guid userId,
        GetProductResumeApplicationsQuery query);
    Task<OperationResult<ResumeDto>> GetApplicantResumeAsync(Guid userId, Guid productResumeApplicationId);
    Task<(Stream? Stream, string? ContentType, string? Error)> GetApplicantResumePhotoAsync(
        Guid userId,
        Guid productResumeApplicationId);
    Task<(Stream? Stream, string? ContentType, string? FileName, string? Error)> GetApplicantResumeFileAsync(
        Guid userId,
        Guid productResumeApplicationId);
    Task<OperationResult<ProductResumeApplicationForOwnerDto>> ReviewAsync(
        Guid userId,
        ReviewProductResumeApplicationCommand command);
}
