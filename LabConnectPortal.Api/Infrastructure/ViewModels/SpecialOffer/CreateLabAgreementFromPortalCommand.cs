using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.SpecialOffer;

public class CreateLabAgreementFromPortalCommand
{
    public long? SpecialOfferRequestId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime ExpDate { get; set; }
    public int PrimaryAgreementLabCodeNew { get; set; }
    public int ReceiverAgreementLabCodeNew { get; set; }
    public bool IsAddendum { get; set; }
    public long? ParentId { get; set; }
    public List<PortalLabAgreementAttachmentCommand> Attachments { get; set; } = [];
    public List<PortalLabAgreementTestPriceCommand> TestPrices { get; set; } = [];
}

public class PortalLabAgreementAttachmentCommand
{
    public string? Remark { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class PortalLabAgreementTestPriceCommand
{
    public long TestInfoId { get; set; }
}
