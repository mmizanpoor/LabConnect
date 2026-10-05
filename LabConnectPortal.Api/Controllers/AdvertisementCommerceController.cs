using System.Security.Claims;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.AdvertisementCommerce;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
public class AdvertisementPositionController(IAdvertisementPositionService service) : ControllerBase
{
    [HttpGet("GetAll")]
    public async Task<OperationResult> GetAll(bool activeOnly = false)
        => await service.GetAllAsync(activeOnly);

    [HttpGet("GetById")]
    public async Task<OperationResult> GetById(int id)
        => await service.GetByIdAsync(id);

    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] SaveAdvertisementPositionCommand command)
        => await service.CreateAsync(command);

    [HttpPost("Update")]
    public async Task<OperationResult> Update([FromBody] UpdateAdvertisementPositionCommand command)
        => await service.UpdateAsync(command);

    [HttpDelete("Delete")]
    public async Task<OperationResult> Delete(int id)
        => await service.DeleteAsync(id);
}

[Route("[controller]")]
[ApiController]
[AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
public class AdvertisementDurationController(IAdvertisementDurationService service) : ControllerBase
{
    [HttpGet("GetAll")]
    public async Task<OperationResult> GetAll()
        => await service.GetAllAsync();
}

[Route("[controller]")]
[ApiController]
[AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
public class AdvertisementPriceController(IAdvertisementPriceService service) : ControllerBase
{
    [HttpGet("GetMatrix")]
    public async Task<OperationResult> GetMatrix()
        => await service.GetMatrixAsync();

    [HttpGet("GetCurrent")]
    public async Task<OperationResult> GetCurrent(int positionId, int durationId)
        => await service.GetCurrentPriceAsync(positionId, durationId);

    [HttpGet("GetHistory")]
    public async Task<OperationResult> GetHistory([FromQuery] GetAdvertisementPriceHistoryQuery query)
        => await service.GetHistoryAsync(query);

    [HttpPost("SetPrice")]
    public async Task<OperationResult> SetPrice([FromBody] SetAdvertisementPriceCommand command)
        => await service.SetPriceAsync(GetUserId(), command);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[Route("[controller]")]
[ApiController]
[AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
public class AdvertisementOrderController(IAdvertisementOrderService service) : ControllerBase
{
    [HttpGet("GetAll")]
    public async Task<OperationResult> GetAll()
        => await service.GetAllAsync();

    [HttpGet("GetById")]
    public async Task<OperationResult> GetById(Guid id)
        => await service.GetByIdAsync(id);

    [HttpPost("Create")]
    public async Task<OperationResult> Create([FromBody] SaveAdvertisementOrderCommand command)
        => await service.CreateAsync(command);

    [HttpPost("UpdateStatus")]
    public async Task<OperationResult> UpdateStatus([FromBody] UpdateAdvertisementOrderStatusCommand command)
        => await service.UpdateStatusAsync(command);

    [HttpDelete("Delete")]
    public async Task<OperationResult> Delete(Guid id)
        => await service.DeleteAsync(id);
}
