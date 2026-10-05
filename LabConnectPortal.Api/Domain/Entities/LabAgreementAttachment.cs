using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabConnectPortal.Api.Domain.Entities
{
    public class LabAgreementAttachment
    {
        [Key]
        public long Id { get; set; }
        public string? Remark { get; set; }
        public string FileName { get; set; }
        public string? ContentType { get; set; }
        public long LabAgreementId { get; set; }


        // Navigation property (many-to-one)
        [ForeignKey(nameof(LabAgreementId))]
        public virtual LabAgreement LabAgreement { get; set; }
    }
}

