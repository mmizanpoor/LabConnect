using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Invoice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class InvoiceController(IInvoiceService invoiceService) : ControllerBase
{
    [HttpPost("SentInvoicesToSenderLabs")]
    public Task<OperationResult<List<Guid>>> SentInvoicesToSenderLabs([FromBody] InvoicesResult result)
        => invoiceService.SaveSentInvoicesAsync(result);

    [HttpGet("{invoiceId:guid}/reports")]
    public Task<OperationResult<InvoiceReportsResult>> GetReports(Guid invoiceId)
        => invoiceService.GetReportsAsync(invoiceId);

    [HttpPost("search")]
    public Task<OperationResult<List<InvoiceReportListItem>>> Search([FromBody] GetInvoiceReportsQuery query)
        => invoiceService.GetByLabCodeAndDateRangeAsync(query);

    [HttpPost("UpdateState")]
    public Task<OperationResult> UpdateState([FromBody] UpdateInvoiceStateCommand command)
        => invoiceService.UpdateStateAsync(command);

    [HttpGet("{invoiceId:guid}/documents")]
    public async Task<IActionResult> GetPdf(Guid invoiceId)
    {
        var result = await invoiceService.GetPdfAsync(invoiceId);
        if (!result.Status || result.Data == null)
            return NotFound(result);

        var contentDisposition = new ContentDispositionHeaderValue("inline")
        {
            FileNameStar = result.Data.FileName,
        };
        Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();

        return File(result.Data.Content, "application/pdf");
    }
}
