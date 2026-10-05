using LabConnectPortal.Api.Infrastructure.ViewModels.PaymentGateway;

namespace LaboratoryApi.Models.DTO.PaymentGateway
{
    public class VerifyRequestDto
    {
        public decimal Amount { get; set; }
        public string Key { get; set; }
        public string? SystemTransactionKey { get; set; }
        public string? Code { get; set; }
    }
}
