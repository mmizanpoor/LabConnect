namespace LabConnectPortal.Api.Infrastructure.ViewModels.LabUser;

public class ToggleLabMemberActiveCommand
{
    public Guid MemberId { get; set; }
    public bool IsActive { get; set; }
}
