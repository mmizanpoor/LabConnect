using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.ViewModels.Invoice;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;

public interface IInvoiceRepository : IRepository<Invoice>
{
    void AddRange(IEnumerable<Invoice> invoices);
    Task<Invoice?> GetByIdAsync(Guid id);
    Task<List<Invoice>> GetByPrimaryLabCodeAndCreateDateRangeAsync(int labCode, DateTime fromDateTime, DateTime toDateTime, InvoiceDirection invoiceDirection);
    Task<bool> UpdateStateAsync(Guid invoiceId, int state);
}
