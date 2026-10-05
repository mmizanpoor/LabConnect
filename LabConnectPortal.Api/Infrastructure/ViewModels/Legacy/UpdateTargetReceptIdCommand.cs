namespace LabConnectPortal.Api.Infrastructure.ViewModels.Legacy
{
    public class UpdateTargetReceptIdCommand
    {
        public int Id { get; set; }
        public int SourceLabId { get; set; }
        public string SourceReceptId { get; set; }

        public int TargetLabId { get; set; }
        public string TargetReceptId { get; set; }
        public string TargetReceptDate { get; set; }

        public string? TargetCPN { get; set; }
        public DateTime? TargetReportingDateTime { get; set; }
        public decimal? TargetApprovePrice { get; set; }
        public int Tracking { get; set; }
        public int? TargetSendAutoState { get; set; }
    }
}

