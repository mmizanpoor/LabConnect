namespace LabConnectPortal.Api.Infrastructure.ViewModels.PaymentGateway
{
    public class VerifyResponseDto
    {
        public string Code { get; set; }
        public string RefId { get; set; }
        public string? CardNumber { get; set; }
        public string? CardHash { get; set; }
        public bool IsSuccess { get; set; }
        /// <summary>
        /// کارمزد
        /// </summary>
        public decimal Fee { get; set; }
        public string? Message { get; set; }
    }
}
