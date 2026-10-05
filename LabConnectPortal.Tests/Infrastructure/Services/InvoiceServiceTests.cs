using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Implementations;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Invoice;
using LabConnectPortal.Api.Infrastructure.ViewModels.Site;
using Microsoft.Extensions.Options;
using iText.Kernel.Pdf;

namespace LabConnectPortal.Tests.Infrastructure.Services;

public class InvoiceServiceTests
{
    [Fact]
    public async Task GetPdfAsync_MergesBothStoredPdfs()
    {
        var invoiceId = Guid.NewGuid();
        var invoice = new Invoice
        {
            Id = invoiceId,
            SentTestIncomeContent = Convert.ToBase64String(CreatePdf()),
            SentTestIncomeFileName = "sent.pdf",
            SenderLaboratoryContent = Convert.ToBase64String(CreatePdf()),
            SenderLaboratoryFileName = "sender.pdf",
        };
        var service = new InvoiceService(new InvoiceRepositoryStub(invoice), null!, Options.Create(new PortalSettings()));

        var result = await service.GetPdfAsync(invoiceId);

        Assert.True(result.Status);
        Assert.NotNull(result.Data);
        Assert.Equal("sent_sender.pdf", result.Data.FileName);

        using var output = new MemoryStream(result.Data.Content);
        using var mergedDocument = new PdfDocument(new PdfReader(output));
        Assert.Equal(2, mergedDocument.GetNumberOfPages());
    }

    [Fact]
    public async Task GetReportsAsync_ReturnsInvoiceIdAndStoredReports()
    {
        var invoiceId = Guid.NewGuid();
        var invoice = new Invoice
        {
            Id = invoiceId,
            SentTestIncomeContent = "sent-content",
            SentTestIncomeFileName = "sent.pdf",
            SenderLaboratoryContent = "sender-content",
            SenderLaboratoryFileName = "sender.pdf",
        };
        var service = new InvoiceService(new InvoiceRepositoryStub(invoice), null!, Options.Create(new PortalSettings()));

        var result = await service.GetReportsAsync(invoiceId);

        Assert.True(result.Status);
        Assert.NotNull(result.Data);
        Assert.Equal(invoiceId, result.Data.InvoiceId);
        Assert.Equal("sent-content", result.Data.SentTestIncomeReport?.Content);
        Assert.Equal("sender-content", result.Data.SenderLaboratoryReport?.Content);
    }

    [Fact]
    public async Task GetByLabCodeAndDateRangeAsync_ReturnsMatchingInvoiceFileNames()
    {
        var fromDateTime = new DateTime(2026, 9, 1);
        var matchingInvoice = new Invoice
        {
            Id = Guid.NewGuid(),
            PrimaryLabCodeNew = 100,
            TargetLabCodeNew = 200,
            State = 1,
            CreateDateTime = fromDateTime.AddDays(2),
            SentTestIncomeFileName = "sent.pdf",
            SenderLaboratoryFileName = "sender.pdf",
        };
        var service = new InvoiceService(
            new InvoiceRepositoryStub(
                matchingInvoice,
                new Invoice { Id = Guid.NewGuid(), PrimaryLabCodeNew = 101, CreateDateTime = fromDateTime.AddDays(2) },
                new Invoice { Id = Guid.NewGuid(), PrimaryLabCodeNew = 100, CreateDateTime = fromDateTime.AddDays(11) }),
            null!,
            Options.Create(new PortalSettings()));

        var result = await service.GetByLabCodeAndDateRangeAsync(new GetInvoiceReportsQuery
        {
            LabCode = 100,
            FromDateTime = fromDateTime,
            ToDateTime = fromDateTime.AddDays(10),
        });

        var item = Assert.Single(result.Data!);
        Assert.True(result.Status);
        Assert.Equal(matchingInvoice.Id, item.InvoiceId);
        Assert.Equal(100, item.PrimaryLabCodeNew);
        Assert.Equal(200, item.TargetLabCodeNew);
        Assert.Equal(1, item.State);
        Assert.Equal("sent.pdf", item.SentTestIncomeReportFileName);
        Assert.Equal("sender.pdf", item.SenderLaboratoryReportFileName);
    }

    [Fact]
    public async Task UpdateStateAsync_UpdatesExistingInvoice()
    {
        var invoice = new Invoice { Id = Guid.NewGuid(), State = 1 };
        var service = new InvoiceService(new InvoiceRepositoryStub(invoice), null!, Options.Create(new PortalSettings()));

        var result = await service.UpdateStateAsync(new UpdateInvoiceStateCommand
        {
            InvoiceId = invoice.Id,
            State = 2,
        });

        Assert.True(result.Status);
        Assert.Equal(2, invoice.State);
    }

    private static byte[] CreatePdf()
    {
        using var output = new MemoryStream();
        using var writer = new PdfWriter(output);
        using var document = new PdfDocument(writer);
        document.AddNewPage();
        document.Close();
        return output.ToArray();
    }

    private sealed class InvoiceRepositoryStub(params Invoice[] invoices) : IInvoiceRepository
    {
        public void Add(Invoice model) { }
        public void AddRange(IEnumerable<Invoice> invoices) { }
        public void Delete(Invoice model) { }
        public Task<IEnumerable<Invoice>> GetAllAsync() => Task.FromResult<IEnumerable<Invoice>>([]);
        public Task<IEnumerable<Invoice>> GetAllAsync(System.Linq.Expressions.Expression<Func<Invoice, bool>> predicate)
            => Task.FromResult<IEnumerable<Invoice>>([]);
        public Task<Invoice?> GetByIdAsync(object id) => Task.FromResult(invoices.FirstOrDefault(invoice => invoice.Id.Equals(id)));
        public Task<Invoice?> GetByIdAsync(Guid id) => Task.FromResult(invoices.FirstOrDefault(invoice => invoice.Id == id));
        public Task<List<Invoice>> GetByPrimaryLabCodeAndCreateDateRangeAsync(int labCode, DateTime fromDateTime, DateTime toDateTime,InvoiceDirection invoiceDirection)
            => Task.FromResult(invoices
                .Where(invoice => invoice.PrimaryLabCodeNew == labCode
                                  && invoice.CreateDateTime.Date >= fromDateTime.Date
                                  && invoice.CreateDateTime.Date <= toDateTime.Date)
                .OrderByDescending(invoice => invoice.CreateDateTime)
                .ToList());
        public Task<bool> UpdateStateAsync(Guid invoiceId, int state)
        {
            var invoice = invoices.FirstOrDefault(invoice => invoice.Id == invoiceId);
            if (invoice == null)
                return Task.FromResult(false);

            invoice.State = state;
            return Task.FromResult(true);
        }
        public Task<OperationResult> SaveChangesAsync() => Task.FromResult(OperationResult.SuccessResult());
        public void Update(Invoice model) { }
    }
}
