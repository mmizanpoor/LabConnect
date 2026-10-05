namespace LabConnectPortal.Infra.ViewModels
{
    public class UpdateReportingItemsCommand
    {
        public long ID { get; set; }
        public int SourceLabId { get; set; }
        public string SourceReceptId { get; set; }
        public int TargetLabId { get; set; }
        public string TargetReceptId { get; set; }
        public string CPNCode { get; set; }
        public string SourceTestName { get; set; }
        public bool IsUrgent { get; set; }
        public string Result { get; set; }
        public string? Comment { get; set; }
        public int Tracking { get; set; }
        public long? LabReceiverRangeDetailId { get; set; }
        public long? ReportingItemId { get; set; }

        public LabReceiverRangeDetailViewModel? RangeDetail { get; set; }
    }
}
