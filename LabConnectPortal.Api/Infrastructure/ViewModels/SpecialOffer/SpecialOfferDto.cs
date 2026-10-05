namespace LabConnectPortal.Api.Infrastructure.ViewModels.SpecialOffer;

public class SpecialOfferListItemDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int TestCount { get; set; }
    public int RequestCount { get; set; }
    public bool IsExpired { get; set; }
}

public class AdminSpecialOfferListItemDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public int LabCodeNew { get; set; }
    public string LabName { get; set; } = string.Empty;
    public string RequesterLabNames { get; set; } = string.Empty;
    public int TestCount { get; set; }
    public int RequestCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminSpecialOfferDetailDto : SpecialOfferDetailDto
{
    public int LabCodeNew { get; set; }
    public string LabName { get; set; } = string.Empty;
    public bool IsExpired { get; set; }
    public int RequestCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SpecialOfferDetailDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string FullBody { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public List<SpecialOfferTestItemDto> Tests { get; set; } = [];
}

public class SpecialOfferTestItemDto
{
    public long TestInfoId { get; set; }
    public string? CpnCode { get; set; }
    public string? NationalCode { get; set; }
    public string? FullName { get; set; }
    public string? ShortName { get; set; }
    public string? SectionName { get; set; }
    public decimal? ApprovePrice { get; set; }
    public decimal? Discount { get; set; }
    public int? MaxSamples { get; set; }
}

public class SaveSpecialOfferTestCommand
{
    public long TestInfoId { get; set; }
    public decimal? Discount { get; set; }
    public int? MaxSamples { get; set; }
}

public class CreateSpecialOfferCommand
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string FullBody { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public List<SaveSpecialOfferTestCommand> Tests { get; set; } = [];
}

public class UpdateSpecialOfferCommand : CreateSpecialOfferCommand
{
    public long Id { get; set; }
}

public class SpecialOfferRequestDto
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public string UserDisplayName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? RejectionReason { get; set; }
    public int RequesterLabCodeNew { get; set; }
    public string RequesterLabName { get; set; } = string.Empty;
    public long? LabAgreementId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class SubmitSpecialOfferRequestCommand
{
    public string? Description { get; set; }
}

public class RejectSpecialOfferRequestCommand
{
    public long RequestId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class SpecialOfferRequestForAgreementDto
{
    public long RequestId { get; set; }
    public long SpecialOfferId { get; set; }
    public string OfferTitle { get; set; } = string.Empty;
    public DateTime OfferStartDate { get; set; }
    public DateTime OfferEndDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public int PrimaryLabCodeNew { get; set; }
    public string PrimaryLabName { get; set; } = string.Empty;
    public int ReceiverLabCodeNew { get; set; }
    public string ReceiverLabName { get; set; } = string.Empty;
    public bool IsAddendum { get; set; }
    public long? ParentId { get; set; }
    public string? ActiveAgreementContractNumber { get; set; }
    public DateTime? ActiveAgreementExpDate { get; set; }
    public List<SpecialOfferTestItemDto> Tests { get; set; } = [];
}

public class PublicSpecialOfferCardDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime EndDate { get; set; }
    public string LabName { get; set; } = string.Empty;
    public Guid? LabProfileId { get; set; }
    public bool HasLabLogo { get; set; }
}

public class PublicSpecialOfferDetailDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string FullBody { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string LabName { get; set; } = string.Empty;
    public string ProposerFirstName { get; set; } = string.Empty;
    public int LabCodeNew { get; set; }
    public bool HasSubmittedRequest { get; set; }
    public bool CanSubmitRequest { get; set; }
    public bool IsOwnLabOffer { get; set; }
    public List<PublicSpecialOfferTestDto> Tests { get; set; } = [];
}

public class PublicSpecialOfferTestDto
{
    public string? CpnCode { get; set; }
    public string? NationalCode { get; set; }
    public string? FullName { get; set; }
    public string? ShortName { get; set; }
    public string? SectionName { get; set; }
    public decimal? ApprovePrice { get; set; }
    public decimal? Discount { get; set; }
    public int? MaxSamples { get; set; }
}
