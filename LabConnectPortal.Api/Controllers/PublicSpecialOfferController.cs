using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SpecialOffer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class PublicSpecialOfferController(
    IPublicSpecialOfferService publicSpecialOfferService,
    ICenterProfileService centerProfileService) : ControllerBase
{
    [HttpGet("GetActiveForHome")]
    [AllowAnonymous]
    public async Task<OperationResult> GetActiveForHome()
        => await publicSpecialOfferService.GetActiveForHomeAsync();

    [HttpGet("GetDetail")]
    [AllowAnonymous]
    public async Task<OperationResult> GetDetail(long id)
        => await publicSpecialOfferService.GetDetailAsync(id, TryGetUserId());

    [HttpGet("GetLabLogo")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLabLogo(Guid profileId)
    {
        var (stream, contentType, error) = await centerProfileService.GetPublicLogoAsync(profileId);
        if (stream == null || contentType == null)
            return NotFound(new OperationResult { Success = false, Message = error ?? "فایل یافت نشد" });

        return File(stream, contentType);
    }

    [HttpPost("SubmitRequest")]
    [Authorize]
    public async Task<OperationResult> SubmitRequest(long id, [FromBody] SubmitSpecialOfferRequestCommand command)
        => await publicSpecialOfferService.SubmitRequestAsync(id, GetUserId(), command);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private Guid? TryGetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
