using iText.Kernel.Pdf;
using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Invoice;
using LabConnectPortal.Api.Infrastructure.ViewModels.Notification;
using LabConnectPortal.Api.Infrastructure.ViewModels.Site;
using Microsoft.Extensions.Options;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class InvoiceService(
    IInvoiceRepository invoiceRepository,
    INotificationService notificationService,
    IOptions<PortalSettings> portalSettings) : IInvoiceService
{
    private readonly PortalSettings _portalSettings = portalSettings.Value;

    public async Task<OperationResult<List<Guid>>> SaveSentInvoicesAsync(InvoicesResult result)
    {
        var invoices = (result.SentInvoices ?? [])
            .Select(command => new Invoice
            {
                Id = Guid.NewGuid(),
                PrimaryLabCodeNew = command.PrimaryLabCodeNew,
                TargetLabCodeNew = command.TargetLabCodeNew,
                Username = command.Username,
                State = 1,
                SentTestIncomeContent = command.SentTestIncomeReport?.Content,
                SentTestIncomeFileName = command.SentTestIncomeReport?.FileName,
                SenderLaboratoryContent = command.SenderLaboratoryReport?.Content,
                SenderLaboratoryFileName = command.SenderLaboratoryReport?.FileName,
                CreateDateTime = DateTime.Now,
            })
            .ToList();

        if (invoices.Count == 0)
            return OperationResult<List<Guid>>.Success([]);

        invoiceRepository.AddRange(invoices);
        var saveResult = await invoiceRepository.SaveChangesAsync();
        if (!saveResult.Status)
            return OperationResult<List<Guid>>.Failure(saveResult.Message ?? "Unable to save invoices.");

        var customersData = await notificationService.GetActiveCustomers();
        if (customersData.Data != null)
        {
            var customerToDictionary = customersData.Data.ToDictionary(x => x.Code);
            foreach (var invoice in invoices)
            {
                customerToDictionary.TryGetValue(invoice.TargetLabCodeNew, out var customer);
                customerToDictionary.TryGetValue(invoice.PrimaryLabCodeNew, out var targetCustomer);

                if (customer != null)
                {
                    var targetUser = new IdTitleDto() { Id = customer.Id, Title = customer.Title };

                    await notificationService.SendNotificationAsync(new NotificationCommandBase
                    {
                        Title = $"صورتحساب آزمایشات",
                        Message = $"صورتحساب جدیدی از {targetCustomer!.Title} برای شما صادر گردید ." + (!string.IsNullOrEmpty(invoice.SentTestIncomeFileName) ? Environment.NewLine + invoice.SentTestIncomeFileName : string.Empty) + (!string.IsNullOrEmpty(invoice.SenderLaboratoryFileName) ? Environment.NewLine + invoice.SenderLaboratoryFileName : string.Empty),
                        Picture = string.Empty,
                        ExpireDate = DateTime.UtcNow.ToLocalTime().AddDays(30),
                        IsActive = true,
                        TargetUsers = [targetUser],
                        NotificationActions =
                        [
                            new NotificationAction
                            {
                                NotificationActionType = Domain.Enums.NotificationActionType.SystemEntity,
                                Label = "مشاهده",
                                Color = "bg-primary-600 text-white",
                                SystemEntityId = new Guid("16ff7d85-ede3-45b6-95e4-9accb91681ad"),
                                Url = _portalSettings.BuildPublicUrl($"Invoice/{invoice.Id}/documents"),
                                OpenTab = true
                            },
                        ],
                    });
                }
            }
        }
        return OperationResult<List<Guid>>.Success(invoices.Select(invoice => invoice.Id).ToList());
    }

    public async Task<OperationResult<InvoiceReportsResult>> GetReportsAsync(Guid invoiceId)
    {
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId);
        if (invoice == null)
            return OperationResult<InvoiceReportsResult>.Failure("Invoice not found.");

        return OperationResult<InvoiceReportsResult>.Success(new InvoiceReportsResult
        {
            InvoiceId = invoice.Id,
            SentTestIncomeReport = ToReportDetail(invoice.SentTestIncomeContent, invoice.SentTestIncomeFileName),
            SenderLaboratoryReport = ToReportDetail(invoice.SenderLaboratoryContent, invoice.SenderLaboratoryFileName),
        });
    }

    public async Task<OperationResult<List<InvoiceReportListItem>>> GetByLabCodeAndDateRangeAsync(
        GetInvoiceReportsQuery query)
    {
        if (query.FromDateTime > query.ToDateTime)
            return OperationResult<List<InvoiceReportListItem>>.Failure("FromDateTime must be earlier than or equal to ToDateTime.");

        var invoices = await invoiceRepository.GetByPrimaryLabCodeAndCreateDateRangeAsync(
            query.LabCode,
            query.FromDateTime,
            query.ToDateTime,
            query.InvoiceDirection);

        var result = invoices.Select((invoice, Index) => new InvoiceReportListItem
        {
            Id = ++Index,
            InvoiceId = invoice.Id,
            PrimaryLabCodeNew = invoice.PrimaryLabCodeNew,
            TargetLabCodeNew = invoice.TargetLabCodeNew,
            State = invoice.State,
            SentTestIncomeReportFileName = invoice.SentTestIncomeFileName,
            SenderLaboratoryReportFileName = invoice.SenderLaboratoryFileName,
            CreateDateTime = invoice.CreateDateTime
        }).ToList();

        return OperationResult<List<InvoiceReportListItem>>.Success(result);
    }

    public async Task<OperationResult> UpdateStateAsync(UpdateInvoiceStateCommand command)
    {
        var wasUpdated = await invoiceRepository.UpdateStateAsync(command.InvoiceId, command.State);
        return wasUpdated
            ? OperationResult.SuccessResult()
            : OperationResult.Failure("Invoice not found.");
    }

    public async Task<OperationResult<InvoicePdfDocument>> GetPdfAsync(Guid invoiceId)
    {
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId);
        if (invoice == null)
            return OperationResult<InvoicePdfDocument>.Failure("Invoice not found.");

        var documents = new List<InvoicePdfDocument>(2);
        if (!TryAddStoredPdf(documents, invoice.SentTestIncomeContent, invoice.SentTestIncomeFileName))
            return OperationResult<InvoicePdfDocument>.Failure("The stored SentTestIncome document is not a valid PDF payload.");

        if (!TryAddStoredPdf(documents, invoice.SenderLaboratoryContent, invoice.SenderLaboratoryFileName))
            return OperationResult<InvoicePdfDocument>.Failure("The stored SenderLaboratory document is not a valid PDF payload.");

        if (documents.Count == 0)
            return OperationResult<InvoicePdfDocument>.Failure("No PDF is available for this invoice.");

        if (documents.Count == 1)
            return OperationResult<InvoicePdfDocument>.Success(documents[0]);

        try
        {
            return OperationResult<InvoicePdfDocument>.Success(new InvoicePdfDocument
            {
                FileName = BuildMergedFileName(documents),
                Content = Convert.FromBase64String(MergeBase64Pdfs(
                [
                    Convert.ToBase64String(documents[0].Content),
                    Convert.ToBase64String(documents[1].Content),
                ])),
            });
        }
        catch (Exception)
        {
            return OperationResult<InvoicePdfDocument>.Failure("The stored invoice documents could not be merged.");
        }
    }

    private static bool TryAddStoredPdf(List<InvoicePdfDocument> documents, string? content, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(content))
            return true;

        if (!TryReadStoredPdf(content, out var pdfBytes))
            return false;

        documents.Add(new InvoicePdfDocument
        {
            FileName = string.IsNullOrWhiteSpace(fileName) ? "invoice.pdf" : fileName,
            Content = pdfBytes,
        });
        return true;
    }

    private static ReportDetailResult? ToReportDetail(string? content, string? fileName)
        => string.IsNullOrWhiteSpace(content)
            ? null
            : new ReportDetailResult(content, fileName ?? string.Empty);

    private static string MergeBase64Pdfs(List<string> base64Pdfs)
    {
        if (base64Pdfs.Count == 1)
            return base64Pdfs.First();

        using var outputStream = new MemoryStream();
        using var writer = new PdfWriter(outputStream);
        using var mergedDoc = new PdfDocument(writer);

        foreach (var pdfBytes in base64Pdfs.Select(Convert.FromBase64String))
        {
            using var sourceStream = new MemoryStream(pdfBytes);
            using var sourceDocument = new PdfDocument(new PdfReader(sourceStream));
            sourceDocument.CopyPagesTo(1, sourceDocument.GetNumberOfPages(), mergedDoc);
        }

        mergedDoc.Close();
        return Convert.ToBase64String(outputStream.ToArray());
    }

    private static string BuildMergedFileName(IReadOnlyList<InvoicePdfDocument> documents)
    {
        var firstName = Path.GetFileNameWithoutExtension(documents[0].FileName);
        var secondName = Path.GetFileNameWithoutExtension(documents[1].FileName);
        return $"{firstName}_{secondName}.pdf";
    }

    private static bool TryReadStoredPdf(string content, out byte[] pdfBytes)
    {
        var payload = content.Trim();
        var commaIndex = payload.IndexOf(',');
        if (payload.StartsWith("data:application/pdf;base64,", StringComparison.OrdinalIgnoreCase) && commaIndex >= 0)
            payload = payload[(commaIndex + 1)..];

        try
        {
            pdfBytes = Convert.FromBase64String(payload);
            return pdfBytes.Length > 0;
        }
        catch (FormatException)
        {
            pdfBytes = [];
            return false;
        }
    }
}
