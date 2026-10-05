namespace LabConnectPortal.Infra.ViewModels
{
    public class LabAgreementTestPriceCommand
    {
        public long Id { get; set; }
        public long TestId { get; set; }
        public string TestName { get; set; }
        public decimal Approved { get; set; }
        public decimal BaseTariffApproved { get; set; }
        public decimal FirstAdditions { get; set; }
        public decimal SecondAdditions { get; set; }
        public decimal UrgentAmount { get; set; }
        public string? CPNCode { get; set; }
        public string? NationalCode { get; set; }
    }
}
