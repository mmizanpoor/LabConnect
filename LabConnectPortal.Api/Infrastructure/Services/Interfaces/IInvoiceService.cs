using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Invoice;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface IInvoiceService
{
    Task<OperationResult<List<Guid>>> SaveSentInvoicesAsync(InvoicesResult result);
    Task<OperationResult<InvoiceReportsResult>> GetReportsAsync(Guid invoiceId);
    Task<OperationResult<List<InvoiceReportListItem>>> GetByLabCodeAndDateRangeAsync(GetInvoiceReportsQuery query);
    Task<OperationResult> UpdateStateAsync(UpdateInvoiceStateCommand command);
    Task<OperationResult<InvoicePdfDocument>> GetPdfAsync(Guid invoiceId);
}
