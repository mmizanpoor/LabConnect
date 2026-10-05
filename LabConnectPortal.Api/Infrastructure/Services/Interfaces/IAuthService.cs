using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IAuthService
{
    Task<OperationResult> SendOtpAsync(SendOtpCommand command);
    Task<OperationResult> SendLaboratoryRegistrationOtpAsync(SendOtpCommand command);
    Task<OperationResult> RegisterLaboratoryAsync(RegisterLaboratoryCommand command);
    Task<OperationResult<TokenResult>> LoginApprovedLaboratoryAsync(
        LoginApprovedLaboratoryCommand command,
        string? clientIp = null,
        string? userAgent = null);
    Task<OperationResult<TokenResult>> EnsurePortalLaboratoryAsync(
        EnsurePortalLaboratoryCommand command,
        string? clientIp = null,
        string? userAgent = null);
    Task<OperationResult<TokenResult>> VerifyOtpAsync(VerifyOtpCommand command, string? clientIp = null, string? userAgent = null);
    Task<OperationResult<TokenResult>> LoginWithPasswordAsync(LoginWithPasswordCommand command, string? clientIp = null, string? userAgent = null);
    Task<OperationResult<TokenResult>> RegisterStoreAsync(RegisterStoreCommand command);
    Task<OperationResult<ProfileDto>> GetProfileAsync(Guid userId);
    Task<OperationResult> UpdateProfileAsync(Guid userId, UpdateProfileCommand command);
    Task<OperationResult<TokenResult>> UpdateUsernameAsync(Guid userId, UpdateUsernameCommand command);
    Task<OperationResult> CheckUsernameAvailableAsync(Guid userId, CheckUsernameAvailableCommand command);
    Task<OperationResult> SendUsernameChangeOtpAsync(Guid userId, SendUsernameChangeOtpCommand command);
    Task<OperationResult> ChangePasswordAsync(Guid userId, ChangePasswordCommand command);
    Task<OperationResult> SendPasswordChangeOtpAsync(Guid userId, SendPasswordChangeOtpCommand command);
    Task<OperationResult<TokenResult>> RefreshAsync(RefreshTokenCommand command);
    Task<OperationResult> LogoutAsync(Guid userId);
    Task<OperationResult> ForgotPasswordAsync(SendOtpCommand command);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordCommand command);
    Task<OperationResult> SendEmailConfirmationAsync(Guid userId);
    Task<OperationResult> ConfirmEmailAsync(string token);
    Task<OperationResult> SendEmailOtpAsync(Guid userId, SendEmailOtpCommand command);
    Task<OperationResult> ConfirmEmailOtpAsync(Guid userId, ConfirmEmailOtpCommand command);
    Task<OperationResult> SendMobileConfirmationAsync(Guid userId);
    Task<OperationResult> ConfirmMobileAsync(ConfirmMobileCommand command);
}
