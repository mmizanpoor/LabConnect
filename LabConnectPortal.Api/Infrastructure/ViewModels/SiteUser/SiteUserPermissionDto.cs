namespace LabConnectPortal.Api.Infrastructure.ViewModels.SiteUser;

public class SiteUserPermissionDto
{
    public Guid SystemEntityId { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
}

public class SetSiteMemberPermissionsCommand
{
    public Guid MemberId { get; set; }
    public List<SiteUserPermissionDto> Permissions { get; set; } = [];
}
