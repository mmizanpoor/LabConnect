using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.OrganizationLocation;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IOrganizationLocationService
{
    Task<OperationResult<List<OrganizationLocationDto>>> GetMyLocationsAsync(Guid userId);
    Task<OperationResult<OrganizationLocationDto>> CreateAsync(Guid userId, CreateOrganizationLocationCommand command);
    Task<OperationResult<OrganizationLocationDto>> UpdateAsync(Guid userId, UpdateOrganizationLocationCommand command);
    Task<OperationResult> DeleteAsync(Guid userId, Guid locationId);
}
