using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.AdvertisementCommerce;

public class AdvertisementPositionDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxConcurrentSlots { get; set; }
    public int? MaxDisplayCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SaveAdvertisementPositionCommand
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxConcurrentSlots { get; set; }
    public int? MaxDisplayCount { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateAdvertisementPositionCommand : SaveAdvertisementPositionCommand
{
    public int Id { get; set; }
}

public class AdvertisementDurationDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int DaysCount { get; set; }
    public int SortOrder { get; set; }
}

public class AdvertisementPriceDto
{
    public Guid Id { get; set; }
    public int AdvertisementPositionId { get; set; }
    public string PositionTitle { get; set; } = string.Empty;
    public int AdvertisementDurationId { get; set; }
    public string DurationTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}

public class AdvertisementPriceMatrixCellDto
{
    public int AdvertisementPositionId { get; set; }
    public int AdvertisementDurationId { get; set; }
    public decimal? CurrentPrice { get; set; }
    public Guid? CurrentPriceId { get; set; }
    public DateTime? ValidFrom { get; set; }
}

public class AdvertisementPriceMatrixDto
{
    public List<AdvertisementPositionDto> Positions { get; set; } = [];
    public List<AdvertisementDurationDto> Durations { get; set; } = [];
    public List<AdvertisementPriceMatrixCellDto> Cells { get; set; } = [];
}

public class SetAdvertisementPriceCommand
{
    public int AdvertisementPositionId { get; set; }
    public int AdvertisementDurationId { get; set; }
    public decimal Price { get; set; }
    public DateTime? ValidFrom { get; set; }
}

public class GetAdvertisementPriceHistoryQuery
{
    public int AdvertisementPositionId { get; set; }
    public int AdvertisementDurationId { get; set; }
}

public class AdvertisementOrderDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserDisplayName { get; set; } = string.Empty;
    public int AdvertisementPositionId { get; set; }
    public string PositionTitle { get; set; } = string.Empty;
    public int AdvertisementDurationId { get; set; }
    public string DurationTitle { get; set; } = string.Empty;
    public Guid AdvertisementPriceId { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AdvertisementOrderStatus Status { get; set; }
    public string StatusTitle { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SaveAdvertisementOrderCommand
{
    public Guid UserId { get; set; }
    public int AdvertisementPositionId { get; set; }
    public int AdvertisementDurationId { get; set; }
    public DateTime StartDate { get; set; }
    public AdvertisementOrderStatus Status { get; set; } = AdvertisementOrderStatus.Draft;
}

public class UpdateAdvertisementOrderStatusCommand
{
    public Guid Id { get; set; }
    public AdvertisementOrderStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
}

public class AdvertisementOrderIdCommand
{
    public Guid Id { get; set; }
}
