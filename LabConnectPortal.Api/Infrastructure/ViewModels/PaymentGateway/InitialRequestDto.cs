namespace LabConnectPortal.Api.Infrastructure.ViewModels.PaymentGateway
{
    public class InitialRequestDto
    {
        public decimal Amount { get; set; }
        public string Domain { get; set; }
        public string? SystemTransactionKey { get; set; }
    }
}
