using System.ComponentModel.DataAnnotations;

namespace LabConnectPortal.Api.Domain.Entities;

/// <summary>
/// ردیف قیمت برای <see cref="SiteChargeService"/>.
/// Fixed: یک ردیف؛ Price = قیمت هر واحد؛ Min/Max/PackageQuantity خالی.
/// Range: چند ردیف؛ MinQuantity–MaxQuantity و Price = قیمت واحد در آن بازه.
/// Package: چند ردیف؛ PackageQuantity = تعداد واحد بسته، Price = مبلغ کل بسته.
/// </summary>
public class SiteChargeServicePrice
{
    [Key]
    public int SiteChargeServicePriceId { get; set; }

    public int SiteChargeServiceId { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>حد پایین بازه (حالت Range). شامل خود مقدار.</summary>
    public int? MinQuantity { get; set; }

    /// <summary>حد بالای بازه (حالت Range). شامل خود مقدار؛ null = بدون سقف.</summary>
    public int? MaxQuantity { get; set; }

    /// <summary>تعداد واحد داخل بسته (حالت Package).</summary>
    public int? PackageQuantity { get; set; }

    /// <summary>
    /// Fixed/Range: قیمت هر واحد (تومان).
    /// Package: مبلغ کل بسته (تومان).
    /// </summary>
    public decimal Price { get; set; }

    public int SortOrder { get; set; }

    public virtual SiteChargeService Service { get; set; } = null!;
}
