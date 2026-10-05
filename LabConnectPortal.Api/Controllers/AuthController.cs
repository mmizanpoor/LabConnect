using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Helpers;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("SendOtp")]
    [AllowAnonymous]
    public async Task<OperationResult> SendOtp([FromBody] SendOtpCommand command)
        => await authService.SendOtpAsync(command);

    [HttpPost("SendLaboratoryRegistrationOtp")]
    [AllowAnonymous]
    public async Task<OperationResult> SendLaboratoryRegistrationOtp([FromBody] SendOtpCommand command)
        => await authService.SendLaboratoryRegistrationOtpAsync(command);

    [HttpPost("RegisterLaboratory")]
    [AllowAnonymous]
    public async Task<OperationResult> RegisterLaboratory([FromBody] RegisterLaboratoryCommand command)
        => await authService.RegisterLaboratoryAsync(command);

    [HttpPost("LoginApprovedLaboratory")]
    [AllowAnonymous]
    public async Task<OperationResult> LoginApprovedLaboratory([FromBody] LoginApprovedLaboratoryCommand command)
        => await authService.LoginApprovedLaboratoryAsync(
            command,
            ClientIpHelper.ResolveClientIp(HttpContext),
            ResolveUserAgent());

    [HttpPost("EnsurePortalLaboratory")]
    [AllowAnonymous]
    public async Task<OperationResult> EnsurePortalLaboratory([FromBody] EnsurePortalLaboratoryCommand command)
        => await authService.EnsurePortalLaboratoryAsync(
            command,
            ClientIpHelper.ResolveClientIp(HttpContext),
            ResolveUserAgent());

    [HttpPost("VerifyOtp")]
    [AllowAnonymous]
    public async Task<OperationResult> VerifyOtp([FromBody] VerifyOtpCommand command)
        => await authService.VerifyOtpAsync(command, ClientIpHelper.ResolveClientIp(HttpContext), ResolveUserAgent());

    [HttpPost("LoginWithPassword")]
    [AllowAnonymous]
    public async Task<OperationResult> LoginWithPassword([FromBody] LoginWithPasswordCommand command)
        => await authService.LoginWithPasswordAsync(command, ClientIpHelper.ResolveClientIp(HttpContext), ResolveUserAgent());

    private string? ResolveUserAgent()
    {
        var ua = Request.Headers.UserAgent.ToString();
        return string.IsNullOrWhiteSpace(ua) ? null : ua.Trim();
    }

    [HttpPost("RegisterStore")]
    [AllowAnonymous]
    public async Task<OperationResult> RegisterStore([FromBody] RegisterStoreCommand command)
        => await authService.RegisterStoreAsync(command);

    [HttpGet("GetProfile")]
    [Authorize]
    public async Task<OperationResult> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await authService.GetProfileAsync(userId);
    }

    [HttpPost("UpdateProfile")]
    [Authorize]
    public async Task<OperationResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await authService.UpdateProfileAsync(userId, command);
    }

    [HttpPost("UpdateUsername")]
    [Authorize]
    public async Task<OperationResult<TokenResult>> UpdateUsername([FromBody] UpdateUsernameCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await authService.UpdateUsernameAsync(userId, command);
    }

    [HttpPost("CheckUsernameAvailable")]
    [Authorize]
    public async Task<OperationResult> CheckUsernameAvailable([FromBody] CheckUsernameAvailableCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await authService.CheckUsernameAvailableAsync(userId, command);
    }

    [HttpPost("SendUsernameChangeOtp")]
    [Authorize]
    public async Task<OperationResult> SendUsernameChangeOtp([FromBody] SendUsernameChangeOtpCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await authService.SendUsernameChangeOtpAsync(userId, command);
    }

    [HttpPost("ChangePassword")]
    [Authorize]
    public async Task<OperationResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await authService.ChangePasswordAsync(userId, command);
    }

    [HttpPost("SendPasswordChangeOtp")]
    [Authorize]
    public async Task<OperationResult> SendPasswordChangeOtp([FromBody] SendPasswordChangeOtpCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await authService.SendPasswordChangeOtpAsync(userId, command);
    }

    [HttpPost("Refresh")]
    [AllowAnonymous]
    public async Task<OperationResult> Refresh([FromBody] RefreshTokenCommand command)
        => await authService.RefreshAsync(command);

    [HttpPost("Logout")]
    [Authorize]
    public async Task<OperationResult> Logout()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await authService.LogoutAsync(userId);
    }

    [HttpPost("ForgotPassword")]
    [AllowAnonymous]
    public async Task<OperationResult> ForgotPassword([FromBody] SendOtpCommand command)
        => await authService.ForgotPasswordAsync(command);

    [HttpPost("ResetPassword")]
    [AllowAnonymous]
    public async Task<OperationResult> ResetPassword([FromBody] ResetPasswordCommand command)
        => await authService.ResetPasswordAsync(command);

    [HttpPost("SendEmailConfirmation")]
    [Authorize]
    public async Task<OperationResult> SendEmailConfirmation()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await authService.SendEmailConfirmationAsync(userId);
    }

    [HttpPost("SendEmailOtp")]
    [Authorize]
    public async Task<OperationResult> SendEmailOtp([FromBody] SendEmailOtpCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await authService.SendEmailOtpAsync(userId, command);
    }

    [HttpPost("ConfirmEmailOtp")]
    [Authorize]
    public async Task<OperationResult> ConfirmEmailOtp([FromBody] ConfirmEmailOtpCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await authService.ConfirmEmailOtpAsync(userId, command);
    }

    [HttpGet("ConfirmEmail")]
    [AllowAnonymous]
    public async Task<OperationResult> ConfirmEmail([FromQuery] string token)
        => await authService.ConfirmEmailAsync(token);

    [HttpPost("SendMobileConfirmation")]
    [Authorize]
    public async Task<OperationResult> SendMobileConfirmation()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await authService.SendMobileConfirmationAsync(userId);
    }

    [HttpPost("ConfirmMobile")]
    [Authorize]
    public async Task<OperationResult> ConfirmMobile([FromBody] ConfirmMobileCommand command)
        => await authService.ConfirmMobileAsync(command);
}
