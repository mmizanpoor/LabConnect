using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;

public interface ICenterProfileRepository : IRepository<CenterProfile>
{
    Task<CenterProfile?> GetByLabCodeAsync(int labCode);
    Task<CenterProfile?> GetByLabCodeNewAsync(int labCodeNew);
    Task<CenterProfile?> GetByOwnerUserIdAsync(Guid ownerUserId);
    Task<CenterProfile?> GetByIdWithTrackingAsync(Guid id);
    Task<PagedResult<CenterProfileListItemDto>> GetLaboratoriesPagedAsync(GetCenterProfilesQuery query);
    Task<PagedResult<CenterProfileListItemDto>> GetShopsPagedAsync(GetCenterProfilesQuery query);
    Task<string?> GetAdminLabMobileAsync(int labCode);
    Task<Guid?> GetAdminLabUserIdAsync(int labCode);
    Task<CenterProfile?> GetProfileByUserId(Guid userId);
}
