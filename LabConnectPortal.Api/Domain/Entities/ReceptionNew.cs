using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabConnectPortal.Api.Domain.Entities
{
    public class ReceptionNew
    {
        public ReceptionNew()
        {
        }

        public ReceptionNew(
            int intSourceLabId,
            string chrSourceReceptId,
            int intTargetLabId,
            string chrSourceSendReceptDate,
            byte tinAge,
            string? chrAgeType,
            string? vchFname,
            string? vchLName,
            bool bitSex,
            string? vchPrevRec,
            bool bitEmg,
            string? vchNIC,
            string? vchMobile,
            string? DocCode
            )
        {
            this.intSourceLabId = intSourceLabId;
            this.intTargetLabId = intTargetLabId;
            this.chrSourceReceptId = chrSourceReceptId;
            this.chrSourceSendReceptDate = chrSourceSendReceptDate;
            this.tinAge = tinAge;
            this.chrAgeType = chrAgeType;
            this.vchFname = vchFname;
            this.vchLName = vchLName;
            this.bitSex = bitSex;
            this.vchPrevRec = vchPrevRec;
            this.bitEmg = bitEmg;
            this.vchNIC = vchNIC;
            this.vchMobile = vchMobile;
            this.DocCode = DocCode;
        }

        [Key]
        [Column(Order = 1)]
        public int intSourceLabId { get; set; }

        [Key]
        [Column(Order = 2)]
        public string chrSourceReceptId { get; set; } = string.Empty;

        public int intTargetLabId { get; set; }

        public string? chrTargetReceptId { get; set; }

        [Required]
        public string chrSourceSendReceptDate { get; set; }

        public byte tinAge { get; set; }

        public string? chrAgeType { get; set; }

        public string? vchFname { get; set; }

        public string? vchLName { get; set; }

        public bool bitSex { get; set; }

        public string? vchPrevRec { get; set; }

        [DefaultValue(false)]
        public bool bitFromSite { get; set; }

        public bool bitEmg { get; set; }

        public string? vchNIC { get; set; } = string.Empty;

        public string? vchMobile { get; set; } = string.Empty;

        public string? DocCode { get; set; } = string.Empty;


        //navigate properties
        public ICollection<ReceptTestNew>? ReceptTests { get; set; }
        public ICollection<ReceptTestP>? ReceptTestPs { get; set; }
        public ICollection<ReceptTestS>? ReceptTestSs { get; set; }
    }
}

