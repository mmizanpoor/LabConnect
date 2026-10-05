using LabConnectPortal.Api.Domain.Entities;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.PaymentGateway
{
    public class PaymentGatewayResultDto
    {
        public PaymentGatewayType PaymentGatewayType { get; set; }
        public string? MerchantId { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
