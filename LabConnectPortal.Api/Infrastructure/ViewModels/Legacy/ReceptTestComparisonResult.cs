namespace LabConnectPortal.Api.Infrastructure.ViewModels.Legacy
{
    public class ReceptTestComparisonResult
    {
        public long Id { get; set; }
        public string? SourceReceptId { get; set; }
        public string? SourceTestName { get; set; }
        public decimal? TargetApprovePrice { get; set; }
    }
}
