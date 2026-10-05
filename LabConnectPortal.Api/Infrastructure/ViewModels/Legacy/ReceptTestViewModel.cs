namespace LabConnectPortal.Api.Infrastructure.ViewModels.Legacy
{
    public class ReceptTestViewModel
    {
        public long Id { get; set; }
        public int SourceLabId { get; set; }
        public string? SourceReceptId { get; set; }
        public string? SourceSendDate { get; set; }
        public string? SourceSendTime { get; set; }
        public long SourceReportingItemId { get; set; }
        public string? SourceTestName { get; set; }
        public string? SourceCPN { get; set; }
        public int TargetLabId { get; set; }
        public string? TargetReceptId { get; set; }
        public string? TargetSendResultDate { get; set; }
        public bool TargetRejected { get; set; }
        public string? TargetReceptDate { get; set; }
        public string? TargetReceptTime { get; set; }
        public string? Result { get; set; }
        public string? SourceReceiveResultDate { get; set; }
        public string? TargetRejectDate { get; set; }
        public string? TestReturnCause { get; set; }
        public string? Comment { get; set; }
        public bool IsUrgent { get; set; } = false;
        public bool AddTestByTarget { get; set; } = false;

        public DateTime? TargetReportingDateTime { get; set; }
        public decimal? TargetApprovePrice { get; set; }
        public decimal? SourceApprovePrice { get; set; }
        public long? SourceTestId { get; set; }
        public string? SourceSectionName { get; set; }
        public int? Tracking { get; set; }
        public int? TargetSendAutoState { get; set; }

        public byte[]? imgResult { get; set; }
        public string? TestType { get; set; }


        public LabReceiverRangeDetailViewModel? RangeDetail { get; set; }
    }
}

