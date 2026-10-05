using LabConnectPortal.Api.Infrastructure.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.Legacy
{
    public class ReceiveGroupReportingItemFilter
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public ReceiveReportType ReceiveReportType { get; set; }
        public int? LabCode { get; set; }
        public List<int>? TargetLabIds { get; set; }
    }
}

