namespace LabConnectPortal.Api.Infrastructure.ViewModels.User;

public class ToggleActiveCommand
{
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
}
