using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class SiteSettings
{
    [Key]
    public int Id { get; set; }

    [MaxLength(200)]
    public string SiteTitle { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Tagline { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LogoPath { get; set; }

    [MaxLength(500)]
    public string? FooterLogoPath { get; set; }

    [MaxLength(50)]
    public string SupportLandline { get; set; } = string.Empty;

    [MaxLength(50)]
    public string SupportMobile { get; set; } = string.Empty;

    public string? ENamadEmbedCode { get; set; }

    [MaxLength(1000)]
    public string? ENamadLinkUrl { get; set; }

    public string? GoogleMapEmbedCode { get; set; }

    [MaxLength(200)]
    public string MetaTitle { get; set; } = string.Empty;

    [MaxLength(500)]
    public string MetaDescription { get; set; } = string.Empty;

    [MaxLength(500)]
    public string MetaKeywords { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? MetaViewport { get; set; }

    [MaxLength(1000)]
    public string? MetaCanonical { get; set; }

    public string? FooterAboutText { get; set; }

    [MaxLength(500)]
    public string FooterAddress { get; set; } = string.Empty;

    [MaxLength(200)]
    public string FooterEmail { get; set; } = string.Empty;

    [MaxLength(500)]
    public string FooterCopyrightText { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();
}
