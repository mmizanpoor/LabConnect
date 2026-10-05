using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.ProductResumeApplication;

public class SubmitProductResumeCommand
{
    public Guid ProductId { get; set; }
}

public class GetProductResumeApplicationsQuery
{
    public Guid ProductId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ReviewProductResumeApplicationCommand
{
    public Guid ProductResumeApplicationId { get; set; }
    public bool Approved { get; set; }
    public string ReviewNotes { get; set; } = string.Empty;
}

public class ProductResumeApplicationDto
{
    public Guid ProductResumeApplicationId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public ProductResumeApplicationStatus Status { get; set; }
    public string ReviewNotes { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class ProductResumeApplicationForOwnerDto
{
    public Guid ProductResumeApplicationId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public Guid ApplicantUserId { get; set; }
    public string ApplicantFirstName { get; set; } = string.Empty;
    public string ApplicantLastName { get; set; } = string.Empty;
    public string ApplicantMobileNumber { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DegreeLevel? DegreeLevel { get; set; }
    public ProductResumeApplicationStatus Status { get; set; }
    public string ReviewNotes { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class ProductResumeApplicationsPageDto
{
    public string ProductTitle { get; set; } = string.Empty;
    public List<ProductResumeApplicationForOwnerDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
