namespace LabConnectPortal.Api.Infrastructure.ViewModels.Advertisement;

public class AdvertisementDto
{
    public Guid AdvertisementId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public bool HasImage { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
}

public class SaveAdvertisementCommand
{
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateAdvertisementCommand : SaveAdvertisementCommand
{
    public Guid AdvertisementId { get; set; }
}

public class PublicAdvertisementCardDto
{
    public Guid AdvertisementId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public string? ProductImagePath { get; set; }
}
