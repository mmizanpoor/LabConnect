using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabConnectPortal.Api.Domain.Entities;

[Table("Security")]
public class Security
{
    [Key]
    [Column("securityId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int securityId { get; set; }

    public int intLabCode { get; set; }

    public int intLabCodeNew { get; set; }

    [MaxLength(200)]
    public string? vchLockData { get; set; }

    [MaxLength(50)]
    public string vchCity { get; set; } = string.Empty;

    [MaxLength(50)]
    public string vchState { get; set; } = string.Empty;

    public bool bitWithSms { get; set; }

    [MaxLength(20)]
    public string vchSmsNumber { get; set; } = string.Empty;

    [MaxLength(10)]
    public string vchSmsServer { get; set; } = string.Empty;

    public bool bitWithOnlineReception { get; set; }

    public int intSmsCount { get; set; }

    public double fltUnitPrice { get; set; }

    public double fltCredit { get; set; }

    public bool WithApp { get; set; }

    [MaxLength(10)]
    [Column(TypeName = "char(10)")]
    public string chrSmsActiveDate { get; set; } = string.Empty;

    [MaxLength(1)]
    [Column(TypeName = "char(1)")]
    public string chrSenderType { get; set; } = string.Empty;

    public bool WithAppNew { get; set; }

    [MaxLength(10)]
    [Column(TypeName = "char(10)")]
    public string chrExpDate { get; set; } = string.Empty;

    public long binCash { get; set; }

    public bool bitActiveCash { get; set; }

    public bool bitActiveCashNew { get; set; }

    [MaxLength(10)]
    [Column(TypeName = "char(10)")]
    public string chrEstDate { get; set; } = string.Empty;

    public long binEstPrice { get; set; }

    public int intEstCount { get; set; }

    public bool bitActiveEst { get; set; }

    public bool bitFormal { get; set; }

    public bool bitNotFormal { get; set; }

    public int intLinkCount { get; set; }

    public bool isPro { get; set; }

    public int intPollCount { get; set; }

    public int intSuppInsurEstCount { get; set; }

    public int intTrackingLinkCount { get; set; }

    public int intPaymentLinkCount { get; set; }

    public int intMessengerCreditCount { get; set; }
}
