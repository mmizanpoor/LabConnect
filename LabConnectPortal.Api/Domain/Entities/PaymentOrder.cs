using LabConnectPortal.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities
{
    [Index(nameof(RefId)), Index(nameof(BankOrderKey)), Index(nameof(SystemTransactionKey)), Index(nameof(UserId))]
    public class PaymentOrder
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        /// <summary>تعداد واحد اعتباری که پس از پرداخت موفق باید به حساب اضافه شود.</summary>
        public int CreditQuantity { get; set; }

        public SiteChargeServiceCode SiteChargeServiceCode { get; set; }
        public string? SystemTransactionKey { get; set; }
        public string? BankOrderKey { get; set; }
        public PaymentGatewayType PaymentGatewayType { get; set; }
        public PaymentState PaymentState { get; set; }
        public decimal Amount { get; set; }
        public string? StatusCode { get; set; }
        public string? RefId { get; set; }
        public string? CardNumber { get; set; }
        public string? CardHash { get; set; }
        public bool HasFailed { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.ToLocalTime();
        public virtual User User { get; set; } = null!;
    }
    public enum PaymentState
    {
        Pending,
        Verifying,
        Completed
    }
    public enum PaymentGatewayType
    {
        Zarinpal = 1,
        BehPardakht = 2
    }
}
