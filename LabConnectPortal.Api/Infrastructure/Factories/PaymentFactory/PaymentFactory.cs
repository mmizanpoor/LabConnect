using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;

namespace LabConnectPortal.Api.Infrastructure.Factories.PaymentFactory
{
    public class PaymentFactory(IServiceProvider serviceProvider) : IPaymentFactory
    {
        public IPaymentGatewayService GetPaymentGatewayService(PaymentGatewayType paymentGatewayType)
        {
            return paymentGatewayType switch
            {
                PaymentGatewayType.BehPardakht => serviceProvider.GetRequiredService<IBehPardakhtService>(),
                _ => throw new ArgumentException("سرویس درگاه پرداخت یافت نشد")
            };
        }
    }
}
