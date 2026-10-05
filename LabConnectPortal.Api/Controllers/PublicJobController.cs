using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.PublicJob;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[AllowAnonymous]
public class PublicJobController(IPublicJobService publicJobService) : ControllerBase
{
    [HttpPost("GetActivePostings")]
    public async Task<OperationResult> GetActivePostings([FromBody] GetActivePostingsQuery query)
        => await publicJobService.GetActivePostingsAsync(query);

    [HttpGet("GetPostingFilters")]
    public async Task<OperationResult> GetPostingFilters()
        => await publicJobService.GetPostingFiltersAsync();

    [HttpGet("GetById")]
    public async Task<OperationResult> GetById(Guid jobPostingId)
        => await publicJobService.GetByIdAsync(jobPostingId);
}
