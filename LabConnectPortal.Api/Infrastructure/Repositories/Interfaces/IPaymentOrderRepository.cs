using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.ViewModels;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;

public interface IPaymentOrderRepository
{
    Task CreateOrder(PaymentOrder order);
    Task<PaymentOrder?> GetByIdAsync(Guid id);
    Task<PaymentOrder?> GetBySystemTransactionKeyAsync(string systemTransactionKey);
    Task<PaymentOrder?> GetByBankOrderKeyAsync(string bankOrderKey);
    void Update(PaymentOrder order);
    Task<OperationResult> SaveChangesAsync();
}
