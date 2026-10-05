using LabConnectPortal.Api.Infrastructure.Helpers;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[AllowAnonymous]
public class PublicSiteController(
    ISiteSettingsService siteSettingsService,
    ISliderGroupService sliderGroupService,
    IPublicSiteServiceService publicSiteServiceService,
    ISiteAnalyticsService siteAnalyticsService,
    IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    [HttpGet("GetSettings")]
    public async Task<OperationResult> GetSettings()
        => await siteSettingsService.GetPublicAsync();

    [HttpGet("GetActiveSlides")]
    public async Task<OperationResult> GetActiveSlides()
        => await sliderGroupService.GetActiveSlidesAsync();

    [HttpPost("TrackVisit")]
    public async Task<OperationResult> TrackVisit()
    {
        var clientIp = ClientIpHelper.ResolveClientIp(httpContextAccessor.HttpContext);
        await siteAnalyticsService.TrackVisitAsync(clientIp);
        return OperationResult.SuccessResult();
    }

    [HttpGet("GetActiveServices")]
    public async Task<OperationResult> GetActiveServices()
        => await publicSiteServiceService.GetActiveAsync();
}
