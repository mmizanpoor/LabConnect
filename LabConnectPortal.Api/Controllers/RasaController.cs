using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Rasa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("Api/[controller]")]
[ApiController]
[AllowAnonymous]
public class RasaController(IRasaService rasaService) : ControllerBase
{
    [HttpPost("GetContractList")]
    public Task<OperationResult> GetContractList([FromBody] RasaContractListCommand command, CancellationToken cancellationToken)
        => rasaService.GetContractListAsync(command, cancellationToken);

    [HttpPost("ValidationDina")]
    public Task<OperationResult> ValidationDina([FromBody] RasaClaimInfoModelCommand command, CancellationToken cancellationToken)
        => rasaService.ValidationDinaAsync(command, cancellationToken);

    [HttpPost("CancelDina")]
    public Task<OperationResult> CancelDina([FromBody] RasaCancelDinaCommand command, CancellationToken cancellationToken)
        => rasaService.CancelDinaAsync(command, cancellationToken);
}
