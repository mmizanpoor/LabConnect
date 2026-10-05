using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("Api/[controller]")]
[ApiController]
[AllowAnonymous]
public class SepidRadisanController(ISepidRadisanService sepidRadisanService) : ControllerBase
{
    [HttpPost("GetInsurancersList")]
    public Task<OperationResult> GetInsurancersList(
        [FromBody] SepidRadisanGetInsurancersCommand command,
        CancellationToken cancellationToken)
        => sepidRadisanService.GetInsurancersListAsync(command, cancellationToken);

    [HttpPost("GetInquiryPolicies")]
    public Task<OperationResult> GetInquiryPolicies(
        [FromBody] SepidRadisanInquiryCommand command,
        CancellationToken cancellationToken)
        => sepidRadisanService.GetInquiryPoliciesAsync(command, cancellationToken);

    [HttpPost("GetInsuredInfo")]
    public Task<OperationResult> GetInsuredInfo(
        [FromBody] SepidRadisanInsuredInfoCommand command,
        CancellationToken cancellationToken)
        => sepidRadisanService.GetInsuredInfoAsync(command, cancellationToken);

    [HttpPost("InsertPreCheck")]
    public Task<OperationResult> InsertPreCheck(
        [FromBody] SepidRadisanPreCheckIntroductionCommand command,
        CancellationToken cancellationToken)
        => sepidRadisanService.PreCheckIntroductionAsync(command, cancellationToken);

    [HttpPost("CreateIntroductions")]
    public Task<OperationResult> CreateIntroductions(
        [FromBody] SepidRadisanCreateIntroductionCommand command,
        CancellationToken cancellationToken)
        => sepidRadisanService.CreateIntroductionAsync(command, cancellationToken);

    [HttpPost("PutDischarge")]
    public Task<OperationResult> PutDischarge(
        [FromBody] SepidRadisanPutDischargeCommand command,
        CancellationToken cancellationToken)
        => sepidRadisanService.PutDischargeAsync(command, cancellationToken);

    [HttpPost("DeleteIntroduction")]
    public Task<OperationResult> DeleteIntroduction(
        [FromBody] SepidRadisanDeleteIntroductionCommand command,
        CancellationToken cancellationToken)
        => sepidRadisanService.DeleteIntroductionAsync(command, cancellationToken);

    [HttpPost("SendAttach")]
    public Task<OperationResult> SendAttach(
        [FromBody] SepidRadisanAppendAttachmentCommand command,
        CancellationToken cancellationToken)
        => sepidRadisanService.AppendAttachmentAsync(command, cancellationToken);
}
