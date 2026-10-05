using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabConnectPortal.Domain
{
    public class LabAgreementAttachment
    {
        [Key]
        public long Id { get; set; }
        public string? Remark { get; set; }
        public string FileName { get; set; }
        public long LabAgreementId { get; set; }


        // Navigation property (many-to-one)
        [ForeignKey(nameof(LabAgreementId))]
        public virtual LabAgreement LabAgreement { get; set; }
    }
}
