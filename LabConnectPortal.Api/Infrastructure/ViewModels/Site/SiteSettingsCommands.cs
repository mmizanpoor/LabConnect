namespace LabConnectPortal.Api.Infrastructure.ViewModels.Site;

public class SiteUsefulLinkDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class SiteUsefulLinkCommand
{
    public Guid? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class SiteSettingsDto
{
    public int Id { get; set; }
    public string SiteTitle { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public bool HasLogo { get; set; }
    public bool HasFooterLogo { get; set; }
    public string SupportLandline { get; set; } = string.Empty;
    public string SupportMobile { get; set; } = string.Empty;
    public string? ENamadEmbedCode { get; set; }
    public string? ENamadLinkUrl { get; set; }
    public string? GoogleMapEmbedCode { get; set; }
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string MetaKeywords { get; set; } = string.Empty;
    public string? MetaViewport { get; set; }
    public string? MetaCanonical { get; set; }
    public string FooterAboutText { get; set; } = string.Empty;
    public string FooterAddress { get; set; } = string.Empty;
    public string FooterEmail { get; set; } = string.Empty;
    public string FooterCopyrightText { get; set; } = string.Empty;
    public List<SiteUsefulLinkDto> UsefulLinks { get; set; } = [];
    public DateTime UpdatedAt { get; set; }
}

public class SiteSettingsPublicDto
{
    public string SiteTitle { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public bool HasLogo { get; set; }
    public bool HasFooterLogo { get; set; }
    public string SupportLandline { get; set; } = string.Empty;
    public string SupportMobile { get; set; } = string.Empty;
    public string? ENamadEmbedCode { get; set; }
    public string? ENamadLinkUrl { get; set; }
    public string? GoogleMapEmbedCode { get; set; }
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string MetaKeywords { get; set; } = string.Empty;
    public string? MetaViewport { get; set; }
    public string? MetaCanonical { get; set; }
    public string FooterAboutText { get; set; } = string.Empty;
    public string FooterAddress { get; set; } = string.Empty;
    public string FooterEmail { get; set; } = string.Empty;
    public string FooterCopyrightText { get; set; } = string.Empty;
    public List<SiteUsefulLinkDto> UsefulLinks { get; set; } = [];
}

public class UpdateSiteSettingsCommand
{
    public string SiteTitle { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public string SupportLandline { get; set; } = string.Empty;
    public string SupportMobile { get; set; } = string.Empty;
    public string? ENamadEmbedCode { get; set; }
    public string? ENamadLinkUrl { get; set; }
    public string? GoogleMapEmbedCode { get; set; }
    public string MetaTitle { get; set; } = string.Empty;
    public string MetaDescription { get; set; } = string.Empty;
    public string MetaKeywords { get; set; } = string.Empty;
    public string? MetaViewport { get; set; }
    public string? MetaCanonical { get; set; }
    public string FooterAboutText { get; set; } = string.Empty;
    public string FooterAddress { get; set; } = string.Empty;
    public string FooterEmail { get; set; } = string.Empty;
    public string FooterCopyrightText { get; set; } = string.Empty;
    public List<SiteUsefulLinkCommand> UsefulLinks { get; set; } = [];
}
