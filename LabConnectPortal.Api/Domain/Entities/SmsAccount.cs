using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabConnectPortal.Api.Domain.Entities;

[Table("SmsAccount")]
public class SmsAccount
{
    [Key]
    [Column("smsAccountId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int smsAccountId { get; set; }

    public int LabCode { get; set; }

    public int Account { get; set; }

    public int UseAccount { get; set; }

    public long CashAccount { get; set; }

    [MaxLength(10)]
    public string CenterTag { get; set; } = string.Empty;

    public int PollAccount { get; set; }

    public int SuppAccount { get; set; }

    public long Wallet { get; set; }
}
