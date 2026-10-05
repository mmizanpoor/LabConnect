namespace LabConnectPortal.Api.Infrastructure.ViewModels.LabUser;

public class LabUserPermissionDto
{
    public Guid SystemEntityId { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
}

public class SetLabMemberPermissionsCommand
{
    public Guid MemberId { get; set; }
    public List<LabUserPermissionDto> Permissions { get; set; } = [];
}
