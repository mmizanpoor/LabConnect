namespace LabConnectPortal.Api.Infrastructure.ViewModels.Invoice;

public class SentInvoicesCommand
{
    public int PrimaryLabCodeNew { get; set; }
    public int TargetLabCodeNew { get; set; }
    public string Username { get; set; } = string.Empty;
    public ReportDetailResult? SentTestIncomeReport { get; set; }
    public ReportDetailResult? SenderLaboratoryReport { get; set; }
}

public class InvoicesResult
{
    public List<SentInvoicesCommand> SentInvoices { get; set; } = [];
}

public class InvoiceReportsResult
{
    public Guid InvoiceId { get; set; }
    public ReportDetailResult? SentTestIncomeReport { get; set; }
    public ReportDetailResult? SenderLaboratoryReport { get; set; }
}

public class GetInvoiceReportsQuery
{
    public int LabCode { get; set; }
    public DateTime FromDateTime { get; set; }
    public DateTime ToDateTime { get; set; }
    public InvoiceDirection InvoiceDirection { get; set; }
}

public enum InvoiceDirection
{
    Sent,
    Received,
}

public class InvoiceReportListItem
{
    public long Id { get; set; }
    public int PrimaryLabCodeNew { get; set; }
    public int TargetLabCodeNew { get; set; }
    public int State { get; set; }
    public Guid InvoiceId { get; set; }
    public string? SentTestIncomeReportFileName { get; set; }
    public string? SenderLaboratoryReportFileName { get; set; }
    public DateTime CreateDateTime { get; set; }
}

public class UpdateInvoiceStateCommand
{
    public Guid InvoiceId { get; set; }
    public int State { get; set; }
}

public class ReportDetailResult
{
    public ReportDetailResult(string content, string fileName)
    {
        Content = content;
        FileName = fileName;
    }

    public string Content { get; }
    public string FileName { get; }
}

public class InvoicePdfDocument
{
    public byte[] Content { get; set; } = [];
    public string FileName { get; set; } = string.Empty;
}
