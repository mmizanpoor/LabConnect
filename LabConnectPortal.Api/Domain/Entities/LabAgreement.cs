using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabConnectPortal.Api.Domain.Entities
{
    public class LabAgreement
    {
        [Key]
        public long Id { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime ExpDate { get; set; }
        public string ContractNumber { get; set; }
        public int ReceiverAgreementLabCodeNew { get; set; }
        public int PrimaryAgreementLabCodeNew { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public int LaboratoryAgreementState { get; set; }
        public string? PrimaryAgreementSign { get; set; }
        public DateTime? PrimaryAgreementSignDateTime { get; set; }
        public string? ReceiverAgreementSign { get; set; }
        public DateTime? ReceiverAgreementSignDateTime { get; set; }
        public string? ReceiverAgreementUserName { get; set; }
        public bool? GetSampling { get; set; }
        public bool? GetRecept { get; set; }
        public long? ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public virtual LabAgreement? Parent { get; set; }

        public virtual ICollection<LabAgreement>? Children { get; set; }

        public virtual ICollection<LabAgreementAttachment>? Attachments { get; set; }

        public virtual ICollection<LabAgreementTestPrice>? TestPrices { get; set; }

        public virtual ICollection<LabAgreementStatusHistory>? StatusHistories { get; set; }
    }
}

