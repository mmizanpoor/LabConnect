using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Filters;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
[SkipSystemEntity]
public class SecuritySmsReportController(
    ISecuritySmsReportService reportService,
    ILogger<SecuritySmsReportController> logger) : ControllerBase
{

    /// <summary>
    /// Wallet And SmsCount 
    /// </summary>
    /// <returns></returns>
    [HttpGet("ExportExcel")]
    [AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
    public async Task<OperationResult> ExportExcel()
    {
        logger.LogInformation("SecuritySmsReport/ExportExcel called by {User}", User.Identity?.Name);
        return await reportService.ExportExcelAsync();
    }
}
