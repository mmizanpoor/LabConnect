using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ProductCategoryPrice;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IProductCategoryPriceService
{
    Task<OperationResult<List<ProductCategoryPriceItemDto>>> GetAllAsync();
    Task<OperationResult<List<ProductCategoryPriceItemDto>>> SaveAllAsync(Guid userId, SaveProductCategoryPricesCommand command);
}
