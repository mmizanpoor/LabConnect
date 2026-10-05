using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Factories.PaymentFactory;
using LabConnectPortal.Api.Infrastructure.Filters;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;
using LabConnectPortal.Api.Infrastructure.ViewModels.PaymentGateway;
using LabConnectPortal.Api.Infrastructure.ViewModels.PaymentGateway.BehPardakht;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class CenterProfileController(
    ICenterProfileService centerProfileService,
    ILaboratoryLookupService laboratoryLookupService,
    IPaymentFactory paymentFactory) : ControllerBase
{
    [HttpGet("GetMyProfile")]
    [Authorize]
    public async Task<OperationResult> GetMyProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await centerProfileService.GetMyProfileAsync(userId);
    }

    [HttpPost("UpdateMyProfile")]
    [Authorize]
    public async Task<OperationResult> UpdateMyProfile([FromBody] UpdateCenterProfileCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await centerProfileService.UpdateMyProfileAsync(userId, command);
    }

    [HttpPost("SendContactChangeOtp")]
    [Authorize]
    public async Task<OperationResult> SendContactChangeOtp([FromBody] SendCenterContactChangeOtpCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await centerProfileService.SendContactChangeOtpAsync(userId, command);
    }

    [HttpGet("GetSmsChargeInfo")]
    [Authorize]
    public async Task<OperationResult> GetSmsChargeInfo()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await centerProfileService.GetSmsChargeInfoAsync(userId);
    }

    [HttpPost("InitPayCharge")]
    [Authorize]
    public async Task<OperationResult> InitPayCharge([FromBody] InitPayChargeCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await centerProfileService.InitPay(userId, command);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> VerifyPayMellat([FromForm] BehPardakhtCallbackResponseDto callbackResponse)
    {
        var result = await centerProfileService.VerifyPayMellat(callbackResponse);
        return RedirectToPaymentResult(result);
    }
    private IActionResult RedirectToPaymentResult(PaymentResponseDto paymentResponse)
    {
        var frontendDomain = $"{Request.Scheme}://{Request.Host.Value}";
#if DEBUG
        frontendDomain = "http://localhost:4500";
#endif
        var query = $"isSuccess={(paymentResponse.IsSuccess ? "true" : "false")}";
        if (!string.IsNullOrEmpty(paymentResponse.RefId))
            query += $"&refId={Uri.EscapeDataString(paymentResponse.RefId)}";
        if (!string.IsNullOrEmpty(paymentResponse.Message))
        {
            var cleanMessage = paymentResponse.Message
                .Replace("<br/>", "\n")
                .Replace("<br>", "\n")
                .Replace("<br />", "\n");
            query += $"&message={Uri.EscapeDataString(cleanMessage)}";
        }
        return Redirect($"{frontendDomain}/payment-response?{query}");
    }

    [HttpPost("UploadLogo")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadLogo(IFormFile file)
        => await UploadFile(CenterProfileFileKind.Logo, file);

    [HttpPost("UploadNationalCard")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadNationalCard(IFormFile file)
        => await UploadFile(CenterProfileFileKind.NationalCard, file);

    [HttpPost("UploadLicense")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadLicense(IFormFile file)
        => await UploadFile(CenterProfileFileKind.License, file);

    [HttpPost("UploadOfficialImage")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadOfficialImage(IFormFile file)
        => await UploadFile(CenterProfileFileKind.OfficialImage, file);

    [HttpPost("UploadTradeCard")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadTradeCard(IFormFile file)
        => await UploadFile(CenterProfileFileKind.TradeCard, file);

    [HttpPost("DeleteFile")]
    [Authorize]
    [LabPermissionAction(LabPermissionAction.Update)]
    public async Task<OperationResult> DeleteFile([FromBody] DeleteCenterProfileFileCommand command)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await centerProfileService.DeleteFileAsync(userId, command.Kind);
    }

    [HttpPost("GetLaboratories")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> GetLaboratories([FromBody] GetCenterProfilesQuery query)
        => await centerProfileService.GetLaboratoriesAsync(query);

    [HttpPost("GetShops")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> GetShops([FromBody] GetCenterProfilesQuery query)
        => await centerProfileService.GetShopsAsync(query);

    [HttpGet("GetById")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> GetById(Guid id)
        => await centerProfileService.GetByIdAsync(id);

    [HttpGet("GetByLabCodeNew")]
    [AllowAnonymous]
    [SkipSystemEntity]
    public async Task<OperationResult> GetByLabCodeNew([FromQuery] int labCodeNew)
        => await laboratoryLookupService.GetByLabCodeNewAsync(labCodeNew);

    [HttpPost("Approve")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Approve([FromBody] ApproveCenterProfileCommand command)
    {
        var adminUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await centerProfileService.ApproveAsync(adminUserId, command);
    }

    [HttpPost("EnableApiKey")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> EnableApiKey([FromBody] ApproveCenterProfileCommand command)
        => await centerProfileService.EnableApiKeyAsync(command);

    [HttpPost("Reject")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> Reject([FromBody] RejectCenterProfileCommand command)
    {
        var adminUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await centerProfileService.RejectAsync(adminUserId, command);
    }

    [HttpPost("RevokeApproval")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> RevokeApproval([FromBody] ApproveCenterProfileCommand command)
    {
        var adminUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await centerProfileService.RevokeApprovalAsync(adminUserId, command);
    }

    [HttpPost("CreateLaboratory")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> CreateLaboratory([FromBody] CreateLaboratoryCommand command)
        => await centerProfileService.CreateLaboratoryAsync(command);

    [HttpPost("UpdateLaboratoryMobile")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> UpdateLaboratoryMobile([FromBody] UpdateLaboratoryMobileCommand command)
        => await centerProfileService.UpdateLaboratoryMobileAsync(command);

    [HttpGet("GetFile")]
    [Authorize]
    [LabPermissionAction(LabPermissionAction.View)]
    public async Task<IActionResult> GetFile(Guid profileId, CenterProfileFileKind kind)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var (stream, contentType, error) = await centerProfileService.GetFileAsync(userId, profileId, kind);

        if (stream == null || contentType == null)
            return NotFound(new OperationResult { Success = false, Message = error ?? "فایل یافت نشد" });

        return File(stream, contentType);
    }

    private async Task<OperationResult> UploadFile(CenterProfileFileKind kind, IFormFile file)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await centerProfileService.UploadFileAsync(userId, kind, file);
    }
}
