namespace LabConnectPortal.Api.Infrastructure.ViewModels.SiteService;

public class SiteServiceDto
{
    public int SiteServiceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public string? LinkUrl { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

public class SaveSiteServiceCommand
{
    public string Title { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class UpdateSiteServiceCommand : SaveSiteServiceCommand
{
    public int SiteServiceId { get; set; }
}
