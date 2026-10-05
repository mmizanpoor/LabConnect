namespace LabConnectPortal.Api.Infrastructure.ViewModels.Legacy
{
    public class ReceiveReceptionGroupFilterQuery
    {
        public int LabCode { get; set; }
        public List<int> SourceLabCodes { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool? IsReception { get; set; }
        public bool IsReject { get; set; }
        public string? ReceptNoSender { get; set; }
    }
}

