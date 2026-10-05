using LabConnectPortal.Api.Domain.Entities;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.PaymentGateway
{
    public class InitialRequestResponseDto
    {
        public string? Code { get; set; }
        public string Key { get; set; }
        public string? RedirectUrl { get; set; }
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public PaymentGatewayType PaymentGatewayType { get; set; }
    }
}
