using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

public class Invoice
{
    [Key]
    public Guid Id { get; set; }

    public int PrimaryLabCodeNew { get; set; }

    public int TargetLabCodeNew { get; set; }

    [MaxLength(256)]
    public string Username { get; set; } = string.Empty;

    public int State { get; set; }

    public string? SentTestIncomeContent { get; set; }

    public string? SentTestIncomeFileName { get; set; }

    public string? SenderLaboratoryContent { get; set; }

    public string? SenderLaboratoryFileName { get; set; }

    public DateTime CreateDateTime { get; set; }
}
