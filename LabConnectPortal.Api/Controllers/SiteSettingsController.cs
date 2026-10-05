using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Site;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
public class SiteSettingsController(ISiteSettingsService siteSettingsService) : ControllerBase
{
    [HttpGet("Get")]
    public async Task<OperationResult> Get()
        => await siteSettingsService.GetAsync();

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpdateSiteSettingsCommand command)
        => await siteSettingsService.UpdateAsync(command);

    [HttpPost("UploadLogo")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadLogo(IFormFile file)
        => await siteSettingsService.UploadLogoAsync(file);

    [HttpGet("GetLogo")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLogo()
    {
        var (stream, contentType) = await siteSettingsService.OpenLogoAsync();
        if (stream == null || contentType == null)
            return NotFound(new OperationResult { Success = false, Message = "لوگو یافت نشد" });

        return File(stream, contentType);
    }

    [HttpPost("UploadFooterLogo")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<OperationResult> UploadFooterLogo(IFormFile file)
        => await siteSettingsService.UploadFooterLogoAsync(file);

    [HttpGet("GetFooterLogo")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFooterLogo()
    {
        var (stream, contentType) = await siteSettingsService.OpenFooterLogoAsync();
        if (stream == null || contentType == null)
            return NotFound(new OperationResult { Success = false, Message = "لوگوی فوتر یافت نشد" });

        return File(stream, contentType);
    }
}
