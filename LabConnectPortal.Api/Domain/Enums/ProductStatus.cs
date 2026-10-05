namespace LabConnectPortal.Api.Domain.Enums;

public enum ProductStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    /// <summary>Previously approved product taken offline by the owner; can republish without re-approval if unchanged.</summary>
    Unpublished = 4,
}
