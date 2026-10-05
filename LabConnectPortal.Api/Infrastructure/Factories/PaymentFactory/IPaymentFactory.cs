using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;

namespace LabConnectPortal.Api.Infrastructure.Factories.PaymentFactory
{
    public interface IPaymentFactory
    {
        IPaymentGatewayService GetPaymentGatewayService(PaymentGatewayType paymentGatewayType);
    }
}
