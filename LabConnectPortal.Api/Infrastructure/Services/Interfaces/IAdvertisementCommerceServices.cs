using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.AdvertisementCommerce;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IAdvertisementPositionService
{
    Task<OperationResult<List<AdvertisementPositionDto>>> GetAllAsync(bool activeOnly = false);
    Task<OperationResult<AdvertisementPositionDto>> GetByIdAsync(int id);
    Task<OperationResult<AdvertisementPositionDto>> CreateAsync(SaveAdvertisementPositionCommand command);
    Task<OperationResult<AdvertisementPositionDto>> UpdateAsync(UpdateAdvertisementPositionCommand command);
    Task<OperationResult> DeleteAsync(int id);
}

public interface IAdvertisementDurationService
{
    Task<OperationResult<List<AdvertisementDurationDto>>> GetAllAsync();
}

public interface IAdvertisementPriceService
{
    Task<OperationResult<AdvertisementPriceMatrixDto>> GetMatrixAsync();
    Task<OperationResult<AdvertisementPriceDto?>> GetCurrentPriceAsync(int positionId, int durationId, DateTime? asOf = null);
    Task<OperationResult<List<AdvertisementPriceDto>>> GetHistoryAsync(GetAdvertisementPriceHistoryQuery query);
    Task<OperationResult<AdvertisementPriceDto>> SetPriceAsync(Guid userId, SetAdvertisementPriceCommand command);
}

public interface IAdvertisementOrderService
{
    Task<OperationResult<List<AdvertisementOrderDto>>> GetAllAsync();
    Task<OperationResult<AdvertisementOrderDto>> GetByIdAsync(Guid id);
    Task<OperationResult<AdvertisementOrderDto>> CreateAsync(SaveAdvertisementOrderCommand command);
    Task<OperationResult<AdvertisementOrderDto>> UpdateStatusAsync(UpdateAdvertisementOrderStatusCommand command);
    Task<OperationResult> DeleteAsync(Guid id);
}
