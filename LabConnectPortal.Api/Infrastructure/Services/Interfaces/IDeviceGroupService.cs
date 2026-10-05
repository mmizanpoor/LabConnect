using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.DeviceGroup;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IDeviceGroupService
{
    Task<OperationResult<List<DeviceGroupDto>>> GetAllAsync(Guid userId);

    Task<OperationResult<DeviceGroupDto>> CreateAsync(Guid userId, CreateDeviceGroupCommand command);

    Task<OperationResult<DeviceGroupDto>> UpdateAsync(Guid userId, UpdateDeviceGroupCommand command);

    Task<OperationResult> DeleteAsync(Guid userId, long id);
}
