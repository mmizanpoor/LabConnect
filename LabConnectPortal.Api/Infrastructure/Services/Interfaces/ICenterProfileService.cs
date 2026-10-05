using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;
using LabConnectPortal.Api.Infrastructure.ViewModels.PaymentGateway;
using LabConnectPortal.Api.Infrastructure.ViewModels.PaymentGateway.BehPardakht;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ICenterProfileService
{
    Task<OperationResult<CenterProfileDto>> GetMyProfileAsync(Guid userId);
    Task<OperationResult<CenterProfileDto>> UpdateMyProfileAsync(Guid userId, UpdateCenterProfileCommand command);
    Task<OperationResult<SmsChargeInfoDto>> GetSmsChargeInfoAsync(Guid userId);
    Task<OperationResult> SendContactChangeOtpAsync(Guid userId, SendCenterContactChangeOtpCommand command);
    Task<OperationResult<CenterProfileDto>> UploadFileAsync(Guid userId, CenterProfileFileKind kind, IFormFile file);
    Task<OperationResult<CenterProfileDto>> DeleteFileAsync(Guid userId, CenterProfileFileKind kind);
    Task<OperationResult<PagedResult<CenterProfileListItemDto>>> GetLaboratoriesAsync(GetCenterProfilesQuery query);
    Task<OperationResult<PagedResult<CenterProfileListItemDto>>> GetShopsAsync(GetCenterProfilesQuery query);
    Task<OperationResult<CenterProfileDto>> GetByIdAsync(Guid id);
    Task<OperationResult> ApproveAsync(Guid adminUserId, ApproveCenterProfileCommand command);
    Task<OperationResult> EnableApiKeyAsync(ApproveCenterProfileCommand command);
    Task<OperationResult> RejectAsync(Guid adminUserId, RejectCenterProfileCommand command);
    Task<OperationResult> RevokeApprovalAsync(Guid adminUserId, ApproveCenterProfileCommand command);
    Task<OperationResult<CenterProfileListItemDto>> CreateLaboratoryAsync(CreateLaboratoryCommand command);
    Task<OperationResult> UpdateLaboratoryMobileAsync(UpdateLaboratoryMobileCommand command);
    Task<OperationResult<InitialRequestResponseDto>> InitPay(Guid userId, InitPayChargeCommand command);
    Task<PaymentResponseDto> VerifyPayMellat(BehPardakhtCallbackResponseDto callbackResponse);
    Task<(Stream? Stream, string? ContentType, string? Error)> GetFileAsync(
        Guid requesterUserId,
        Guid profileId,
        CenterProfileFileKind kind);

    Task<(Stream? Stream, string? ContentType, string? Error)> GetPublicLogoAsync(Guid profileId);
}
