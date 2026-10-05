using System.Security.Claims;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Filters;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ProductResumeApplication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class ProductResumeApplicationController(IProductResumeApplicationService productResumeApplicationService) : ControllerBase
{
    [HttpPost("Submit")]
    public async Task<OperationResult> Submit([FromBody] SubmitProductResumeCommand command)
        => await productResumeApplicationService.SubmitAsync(GetUserId(), command);

    [HttpGet("GetMyApplications")]
    public async Task<OperationResult> GetMyApplications()
        => await productResumeApplicationService.GetMyApplicationsAsync(GetUserId());

    [HttpPost("GetForProduct")]
    public async Task<OperationResult> GetForProduct([FromBody] GetProductResumeApplicationsQuery query)
        => await productResumeApplicationService.GetForProductAsync(GetUserId(), query);

    [HttpGet("GetApplicantResume")]
    public async Task<OperationResult> GetApplicantResume(Guid productResumeApplicationId)
        => await productResumeApplicationService.GetApplicantResumeAsync(GetUserId(), productResumeApplicationId);

    [HttpGet("GetApplicantResumePhoto")]
    public async Task<IActionResult> GetApplicantResumePhoto(Guid productResumeApplicationId)
    {
        var (stream, contentType, error) = await productResumeApplicationService.GetApplicantResumePhotoAsync(
            GetUserId(),
            productResumeApplicationId);
        if (!string.IsNullOrWhiteSpace(error))
            return StatusCode(StatusCodes.Status403Forbidden, OperationResult.Failure(error));
        if (stream == null || contentType == null)
            return NotFound();
        return File(stream, contentType);
    }

    [HttpGet("GetApplicantResumeFile")]
    public async Task<IActionResult> GetApplicantResumeFile(Guid productResumeApplicationId)
    {
        var (stream, contentType, fileName, error) = await productResumeApplicationService.GetApplicantResumeFileAsync(
            GetUserId(),
            productResumeApplicationId);
        if (!string.IsNullOrWhiteSpace(error))
            return StatusCode(StatusCodes.Status403Forbidden, OperationResult.Failure(error));
        if (stream == null || contentType == null)
            return NotFound();
        return File(stream, contentType, fileName ?? "resume");
    }

    [HttpPost("Review")]
    [LabPermissionAction(LabPermissionAction.Update)]
    public async Task<OperationResult> Review([FromBody] ReviewProductResumeApplicationCommand command)
        => await productResumeApplicationService.ReviewAsync(GetUserId(), command);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
