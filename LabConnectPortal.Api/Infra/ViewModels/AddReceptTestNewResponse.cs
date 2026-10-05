namespace LabConnectPortal.Infra.ViewModels
{
    public class AddReceptTestNewResponse
    {
        public long ID { get; set; }
        public long? ReportingItemId { get; set; }
        public string SourceTestName { get; set; }
        public string SourceCPN { get; set; }
        public bool AddTestByTarget { get; set; }
    }
}
