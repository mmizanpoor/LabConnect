using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Domain
{
    public class ReceptTestNew
    {
        [Key]
        public long ID { get; set; }

        public int intSourceLabId { get; set; }

        public string? chrSourceReceptId { get; set; }

        public string chrSourceSendDate { get; set; }

        public string? vchSourceCPN { get; set; }

        public string? vchSourceTestName { get; set; }

        public int intTargetLabId { get; set; }

        public string? chrTargetReceptId { get; set; }

        public string? chrTargetSendResultDate { get; set; }

        public bool bitTargetRejected { get; set; } = false;

        public string? chrTargetReceptDate { get; set; }

        public string? vchResult { get; set; }

        public string? chrSourceReceiveResultDate { get; set; }

        public string? chrTargetRejectDate { get; set; }

        public string? vchTestReturnCause { get; set; }

        public string? vchTargetNormalRange { get; set; }

        public string? vchComment { get; set; }

        public bool bitEmg { get; set; } = false;

        public bool bitAddTestByTarget { get; set; } = false;


        public DateTime? TargetReportingDateTime { get; set; }
        public decimal? TargetApprovePrice { get; set; }
        public decimal? SourceApprovePrice { get; set; }
        public long? SourceTestId { get; set; }
        public string? SourceSectionName { get; set; }
        public int? Tracking { get; set; }
        public long? LabReceiverRangeDetailId { get; set; }

        /// <summary>
        /// وضعیت کامل بودن جواب در مقصد
        /// </summary>
        public int? TargetSendAutoState { get; set; }




        //navigate properties
        public ReceptionNew? Reception { get; set; }
        public LabReceiverRangeDetail? LabReceiverRangeDetail { get; set; }
    }
}
