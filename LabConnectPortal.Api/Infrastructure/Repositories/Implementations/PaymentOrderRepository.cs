using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations;

public class PaymentOrderRepository(LabConnectDbContext context) : IPaymentOrderRepository
{
    public async Task CreateOrder(PaymentOrder order)
    {
        if (order.Id == Guid.Empty)
            order.Id = Guid.NewGuid();

        order.CreatedAt = DateTime.UtcNow.ToLocalTime();
        order.UpdatedAt = order.CreatedAt;
        await context.PaymentOrders.AddAsync(order);
    }

    public Task<PaymentOrder?> GetByIdAsync(Guid id)
        => context.PaymentOrders.FirstOrDefaultAsync(o => o.Id == id);

    public Task<PaymentOrder?> GetBySystemTransactionKeyAsync(string systemTransactionKey)
        => context.PaymentOrders.FirstOrDefaultAsync(o => o.SystemTransactionKey == systemTransactionKey);

    public Task<PaymentOrder?> GetByBankOrderKeyAsync(string bankOrderKey)
        => context.PaymentOrders.FirstOrDefaultAsync(o => o.BankOrderKey == bankOrderKey);

    public void Update(PaymentOrder order)
    {
        order.UpdatedAt = DateTime.UtcNow.ToLocalTime();
        context.PaymentOrders.Update(order);
    }

    public async Task<OperationResult> SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            return OperationResult.SuccessResult();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure(ex.InnerException?.Message ?? ex.Message);
        }
    }
}
