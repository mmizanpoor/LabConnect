using System.ComponentModel.DataAnnotations;
using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Domain.Entities;

/// <summary>
/// تعریف سرویس شارژ سایت (مثل شارژ پیامک) و حالت قیمت‌گذاری آن.
/// جدا از <see cref="SiteService"/> که کاتالوگ نمایشی سایت است.
/// </summary>
public class SiteChargeService
{
    [Key]
    public int SiteChargeServiceId { get; set; }

    public SiteChargeServiceCode Code { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public SiteChargePricingMode PricingMode { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime UpdatedAt { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public virtual User? UpdatedByUser { get; set; }

    public virtual ICollection<SiteChargeServicePrice> Prices { get; set; } = new List<SiteChargeServicePrice>();
}
