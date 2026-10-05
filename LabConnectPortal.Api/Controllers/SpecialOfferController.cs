using System.Security.Claims;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SpecialOffer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class SpecialOfferController(ISpecialOfferService specialOfferService) : ControllerBase
{
    [HttpGet("GetAll")]
    public async Task<OperationResult> GetAll()
        => await specialOfferService.GetAllAsync(GetUserId());

    [HttpGet("GetById")]
    public async Task<OperationResult> GetById(long id)
        => await specialOfferService.GetByIdAsync(GetUserId(), id);

    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] CreateSpecialOfferCommand command)
        => await specialOfferService.CreateAsync(GetUserId(), command);

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpdateSpecialOfferCommand command)
        => await specialOfferService.UpdateAsync(GetUserId(), command);

    [HttpDelete("Delete")]
    public async Task<OperationResult> Delete(long id)
        => await specialOfferService.DeleteAsync(GetUserId(), id);

    [HttpGet("GetRequests")]
    public async Task<OperationResult> GetRequests(long specialOfferId)
        => await specialOfferService.GetRequestsAsync(GetUserId(), specialOfferId);

    [HttpPost("ApproveRequest")]
    public async Task<OperationResult> ApproveRequest(long requestId)
        => await specialOfferService.ApproveRequestAsync(GetUserId(), requestId);

    [HttpPost("RejectRequest")]
    public async Task<OperationResult> RejectRequest([FromBody] RejectSpecialOfferRequestCommand command)
        => await specialOfferService.RejectRequestAsync(GetUserId(), command);

    [HttpGet("GetRequestForAgreement")]
    public async Task<OperationResult> GetRequestForAgreement(long requestId)
        => await specialOfferService.GetRequestForAgreementAsync(GetUserId(), requestId);

    [HttpGet("GetDashboardStats")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> GetDashboardStats()
        => await specialOfferService.GetAdminDashboardStatsAsync();

    [HttpGet("GetAllForAdmin")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> GetAllForAdmin()
        => await specialOfferService.GetAllForAdminAsync();

    [HttpGet("GetByIdForAdmin")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> GetByIdForAdmin(long id)
        => await specialOfferService.GetByIdForAdminAsync(id);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
