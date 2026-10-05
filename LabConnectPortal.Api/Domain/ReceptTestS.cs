using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabConnectPortal.Domain
{
    public class ReceptTestS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get; set; }

        public int? intSourceLabId { get; set; }

        public string? chrSourceReceptId { get; set; }

        public string chrSourceSendDate { get; set; } = string.Empty;

        public string? vchSourceCPN { get; set; }

        public string? vchSourceTestName { get; set; }

        public int? intTargetLabId { get; set; }

        public string? chrTargetReceptId { get; set; }

        public string? chrTargetSendResultDate { get; set; }

        public bool bitTargetRejected { get; set; } = false;

        public string? chrTargetReceptDate { get; set; }

        public byte[]? imgResult { get; set; }

        public string? chrSourceReceiveResultDate { get; set; }

        public string? chrTargetRejectDate { get; set; }

        public string? vchTestReturnCause { get; set; }

        public string? vchComment { get; set; }

        public bool? bitEmg { get; set; } = false;

        public bool bitAddTestByTarget { get; set; } = false;

        public DateTime? TargetReportingDateTime { get; set; }
        public decimal? TargetApprovePrice { get; set; }
        public decimal? SourceApprovePrice { get; set; }
        public long? SourceTestId { get; set; }
        public string? SourceSectionName { get; set; }
        public int? Tracking { get; set; }


        //navigate properties
        public ReceptionNew? Reception { get; set; }
    }
}
