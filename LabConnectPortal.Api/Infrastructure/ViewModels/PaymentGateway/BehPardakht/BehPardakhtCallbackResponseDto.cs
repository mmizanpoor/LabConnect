namespace LabConnectPortal.Api.Infrastructure.ViewModels.PaymentGateway.BehPardakht
{
    public class BehPardakhtCallbackResponseDto
    {
        public string ResCode { get; set; }
        public string? SaleReferenceId { get; set; }
        public string SaleOrderId { get; set; }
        public string? RefId { get; set; }
    }
}
