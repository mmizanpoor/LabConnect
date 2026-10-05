namespace LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

public class LoginWithPasswordCommand
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
