using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.KitGroup;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IKitGroupService
{
    Task<OperationResult<List<KitGroupDto>>> GetAllAsync(Guid userId);

    Task<OperationResult<KitGroupDto>> CreateAsync(Guid userId, CreateKitGroupCommand command);

    Task<OperationResult<KitGroupDto>> UpdateAsync(Guid userId, UpdateKitGroupCommand command);

    Task<OperationResult> DeleteAsync(Guid userId, long id);
}
