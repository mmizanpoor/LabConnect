using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels.Invoice;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations;

public class InvoiceRepository : LabConnectRepository<Invoice>, IInvoiceRepository
{
    private readonly LabConnectDbContext _context;

    public InvoiceRepository(LabConnectDbContext context) : base(context)
    {
        _context = context;
    }

    public void AddRange(IEnumerable<Invoice> invoices)
        => _context.Invoices.AddRange(invoices);

    public Task<Invoice?> GetByIdAsync(Guid id)
        => _context.Invoices.AsNoTracking().FirstOrDefaultAsync(invoice => invoice.Id == id);

    public Task<List<Invoice>> GetByPrimaryLabCodeAndCreateDateRangeAsync(
        int labCode,
        DateTime fromDateTime,
        DateTime toDateTime,
        InvoiceDirection invoiceDirection)
    {
        var dbQuery = _context.Invoices
             .AsNoTracking()
             .Where(invoice => invoice.CreateDateTime.Date >= fromDateTime.Date
                               && invoice.CreateDateTime.Date <= toDateTime.Date).AsQueryable();

        if (invoiceDirection == InvoiceDirection.Sent)
            dbQuery = dbQuery.Where(x => x.PrimaryLabCodeNew == labCode);
        else
            dbQuery = dbQuery.Where(x => x.TargetLabCodeNew == labCode);

        return dbQuery.OrderByDescending(invoice => invoice.CreateDateTime)
             .ToListAsync();
    }

    public async Task<bool> UpdateStateAsync(Guid invoiceId, int state)
        => await _context.Invoices
            .Where(invoice => invoice.Id == invoiceId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(invoice => invoice.State, state)) > 0;
}
