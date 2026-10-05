using System.Security.Claims;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Filters;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.LabAgreementSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class LabAgreementSettingsController(ILabAgreementSettingsService settingsService) : ControllerBase
{
    [HttpGet("GetMine")]
    public async Task<OperationResult> GetMine()
        => await settingsService.GetMineAsync(GetUserId());

    [HttpGet("GetLabAgreementSettingByLabCodeNew")]
    [AllowAnonymous]
    [SkipSystemEntity]
    public async Task<OperationResult> GetByLabCodeNew(int labCodeNew)
        => await settingsService.GetByLabCodeNewAsync(labCodeNew);

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpsertLabAgreementSettingsCommand command)
        => await settingsService.UpsertAsync(GetUserId(), command);

    [HttpPost("UploadHeaderImage")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadHeaderImage(IFormFile file)
        => await settingsService.UploadFileAsync(GetUserId(), LabAgreementSettingsFileKind.HeaderImage, file);

    [HttpPost("UploadHeaderLogo")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadHeaderLogo(IFormFile file)
        => await settingsService.UploadFileAsync(GetUserId(), LabAgreementSettingsFileKind.HeaderLogo, file);

    [HttpGet("GetFile")]
    public async Task<IActionResult> GetFile(LabAgreementSettingsFileKind kind)
    {
        var (stream, contentType, error) = await settingsService.GetFileAsync(GetUserId(), kind);
        if (stream == null)
            return NotFound(OperationResult.Failure(error ?? "فایل یافت نشد"));

        return File(stream, contentType ?? "application/octet-stream");
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
