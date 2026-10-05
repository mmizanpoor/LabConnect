namespace LabConnectPortal.Api.Infrastructure.ViewModels.Site;

public class PortalSettings
{
    public string PublicBaseUrl { get; set; } = "https://connect.ptnapi.ir";

    public string BuildPublicUrl(string path)
        => $"{PublicBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
}
