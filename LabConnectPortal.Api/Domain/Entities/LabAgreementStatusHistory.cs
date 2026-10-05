using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabConnectPortal.Api.Domain.Entities;

public class LabAgreementStatusHistory
{
    [Key]
    public long Id { get; set; }

    public long LabAgreementId { get; set; }

    public int UserType { get; set; }

    public int PartyActionType { get; set; }

    public string ActionUserName { get; set; } = string.Empty;

    public DateTime ActionDateTime { get; set; }

    public string? Reason { get; set; }

    [ForeignKey(nameof(LabAgreementId))]
    public virtual LabAgreement LabAgreement { get; set; } = null!;
}
