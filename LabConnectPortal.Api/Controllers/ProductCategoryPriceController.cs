using System.Security.Claims;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ProductCategoryPrice;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
[AuthorizeUserTypes(UserType.Administrator, UserType.Admin)]
public class ProductCategoryPriceController(IProductCategoryPriceService service) : ControllerBase
{
    [HttpGet("GetAll")]
    public async Task<OperationResult> GetAll()
        => await service.GetAllAsync();

    [HttpPost("SaveAll")]
    public async Task<OperationResult> SaveAll([FromBody] SaveProductCategoryPricesCommand command)
        => await service.SaveAllAsync(GetUserId(), command);

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
