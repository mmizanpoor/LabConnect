using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

public class ContentPost
{
    [Key]
    public Guid ContentPostId { get; set; }

    public int? ContentGroupId { get; set; }

    public ContentPostType Type { get; set; } = ContentPostType.News;

    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ShortDescription { get; set; } = string.Empty;

    public string FullBody { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? FeaturedImagePath { get; set; }

    public bool IsActive { get; set; } = true;

    public bool ShowOnHomePage { get; set; }

    public bool ShowAuthor { get; set; }

    [MaxLength(200)]
    public string? AuthorName { get; set; }

    public DateTime? PublishedAt { get; set; }

    [MaxLength(300)]
    public string? BrowserTitle { get; set; }

    [MaxLength(500)]
    public string? MetaKeywords { get; set; }

    [MaxLength(500)]
    public string? MetaDescription { get; set; }

    [MaxLength(2000)]
    public string? CustomMetaTags { get; set; }

    [MaxLength(500)]
    public string? ExternalLink { get; set; }

    public int ViewCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ContentGroup? Group { get; set; }
}
