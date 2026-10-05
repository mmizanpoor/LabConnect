using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabConnectPortal.Api.Domain.Entities
{
    public class LabAgreementTestPrice
    {
        [Key]
        public long Id { get; set; }
        public long TestId { get; set; }
        public string TestName { get; set; }
        public decimal Approved { get; set; }
        public decimal BaseTariffApproved { get; set; }
        public decimal FirstAdditions { get; set; }
        public decimal SecondAdditions { get; set; }
        public decimal UrgentAmount { get; set; }
        public string? CPNCode { get; set; }
        public string? NationalCode { get; set; }
        public long LabAgreementId { get; set; }


        // Navigation property (many-to-one)
        [ForeignKey(nameof(LabAgreementId))]
        public virtual LabAgreement LabAgreement { get; set; }
    }
}

