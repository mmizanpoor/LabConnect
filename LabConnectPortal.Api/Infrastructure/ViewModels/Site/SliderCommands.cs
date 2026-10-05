namespace LabConnectPortal.Api.Infrastructure.ViewModels.Site;

public class GetSliderGroupsQuery
{
    public string? Title { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SliderGroupListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public int SlideCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SliderSlideDto
{
    public Guid Id { get; set; }
    public Guid SliderGroupId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class SliderGroupDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SliderSlideDto> Slides { get; set; } = [];
}

public class SaveSliderGroupCommand
{
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class UpdateSliderGroupCommand
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateSliderSlideCommand
{
    public Guid Id { get; set; }
    public Guid SliderGroupId { get; set; }
    public string? LinkUrl { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class ReorderSlidesCommand
{
    public Guid SliderGroupId { get; set; }
    public List<Guid> SlideIds { get; set; } = [];
}

public class DeleteSliderSlideCommand
{
    public Guid Id { get; set; }
    public Guid SliderGroupId { get; set; }
}

public class ActiveSliderSlideDto
{
    public Guid Id { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
