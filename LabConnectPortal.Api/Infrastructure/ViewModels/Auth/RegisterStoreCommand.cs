namespace LabConnectPortal.Api.Infrastructure.ViewModels.Auth;

public class RegisterStoreCommand
{
    public string Username { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
}
