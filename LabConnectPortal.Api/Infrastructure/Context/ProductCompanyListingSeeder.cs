using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Context;

/// <summary>
/// One-time seed of store listings. Call once, then keep commented in DbSeeder.
/// </summary>
public static class ProductCompanyListingSeeder
{
    private const string StorageRoot = "uploads";

    public static async Task SeedOnceAsync(LabConnectDbContext context, string contentRootPath)
    {
        if (await context.Products.AnyAsync())
            return;

        var stores = await context.CenterProfiles.AsNoTracking()
            .Where(c => c.CenterType == CenterType.Store)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        if (stores.Count == 0)
            return;

        var owners = await context.Users.AsNoTracking()
            .Where(u => u.CenterProfileId != null)
            .Select(u => new { u.Id, u.CenterProfileId })
            .ToListAsync();

        var ownerByCenter = owners
            .GroupBy(u => u.CenterProfileId!.Value)
            .ToDictionary(g => g.Key, g => g.First().Id);

        var brands = await context.Brands.AsNoTracking()
            .Select(b => new { b.BrandId, b.Title })
            .ToListAsync();

        int BrandId(string title) =>
            brands.First(b => b.Title == title).BrandId;

        var categories = await context.ProductCategories.AsNoTracking()
            .Select(c => new { c.ProductCategoryId, c.Title })
            .ToListAsync();

        int CategoryId(string title) =>
            categories.First(c => c.Title == title).ProductCategoryId;

        var now = DateTime.UtcNow.ToLocalTime();
        var specs = BuildSpecs();

        foreach (var store in stores)
        {
            if (!ownerByCenter.TryGetValue(store.Id, out var ownerId))
                continue;

            foreach (var spec in specs)
            {
                var productId = Guid.NewGuid();
                var imagePath = CopyProductImage(contentRootPath, productId, spec.SeedImage);

                var product = new Product
                {
                    ProductId = productId,
                    Title = spec.Title,
                    BrandId = BrandId(spec.BrandTitle),
                    Warranty = spec.Warranty,
                    Description = spec.Description,
                    Status = ProductStatus.Approved,
                    FeaturedImagePath = imagePath,
                    Price = spec.Price,
                    IsNegotiablePrice = true,
                    IsUsed = spec.IsUsed,
                    StockQuantity = spec.Stock,
                    DiscountPercent = spec.Discount,
                    CreatedByUserId = ownerId,
                    CreatedByCenterProfileId = store.Id,
                    SubmittedAt = now,
                    ApprovedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now,
                };

                context.Products.Add(product);
                context.ProductCategoryAssignments.Add(new ProductCategoryAssignment
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ProductCategoryId = CategoryId(spec.CategoryTitle),
                });

                if (!string.IsNullOrWhiteSpace(imagePath))
                {
                    context.ProductImages.Add(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        ImagePath = imagePath,
                        SortOrder = 0,
                    });
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private static ListingSpec[] BuildSpecs() =>
    [
        new(
            "آنالایزر بیوشیمی Mindray BS-240",
            "Mindray",
            "تجهیزات سنجشی",
            "آنالایزر بیوشیمی رومیزی برای آزمایشگاه‌های متوسط، با ظرفیت مناسب و خدمات پس از فروش.",
            "12 ماه گارانتی شرکتی",
            185000000m,
            2,
            false,
            8,
            "prod-chemistry-analyzer.jpg"),
        new(
            "آنالایزر هماتولوژی Sysmex XN-550",
            "Sysmex",
            "تجهیزات سنجشی",
            "سیستم هماتولوژی پنج بخشی برای چکاپ روزانه و شمارش سلول‌های خونی.",
            "18 ماه گارانتی",
            265000000m,
            1,
            false,
            12,
            "prod-hematology-analyzer.jpg"),
        new(
            "کیت TSH برند Roche",
            "Roche",
            "کیت های آزمایشگاهی",
            "کیت ایمونواسی TSH با پایداری بالا برای دستگاه‌های Roche.",
            "۶ ماه از تاریخ تولید",
            18500000m,
            12,
            false,
            10,
            "prod-tsh-kit.jpg"),
        new(
            "کیت HbA1c نوین",
            "پارس پیوند",
            "کیت های آزمایشگاهی",
            "کیت اندازه‌گیری هموگلوبین A1c برای پایش دیابت در آزمایشگاه‌های بالینی.",
            "۹ ماه",
            12400000m,
            20,
            false,
            15,
            "prod-hba1c-kit.jpg"),
        new(
            "سانتریفیوژ آزمایشگاهی",
            "Human",
            "تجهیزات عمومی",
            "سانتریفیوژ رومیزی مناسب جداسازی سرم و پلاسما در بخش پذیرش و بیوشیمی.",
            "12 ماه",
            42000000m,
            4,
            false,
            null,
            "prod-centrifuge.jpg"),
        new(
            "میکروسکوپ نوری آزمایشگاهی",
            "Hitachi",
            "تجهیزات پایه آزمایشگاهی",
            "میکروسکوپ دوچشمی برای هماتولوژی و انگل‌شناسی با کیفیت اپتیک مناسب آموزش و تشخیص.",
            "24 ماه",
            38000000m,
            3,
            false,
            5,
            "prod-microscope.jpg"),
    ];

    private static string? CopyProductImage(string contentRootPath, Guid productId, string seedFileName)
    {
        var seedPath = Path.Combine(contentRootPath, "SeedAssets", "products", seedFileName);
        if (!File.Exists(seedPath))
            return null;

        var extension = Path.GetExtension(seedFileName).TrimStart('.').ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}.{extension}";
        var relativePath = Path.Combine(StorageRoot, "products", productId.ToString(), fileName)
            .Replace('\\', '/');
        var absolutePath = Path.Combine(contentRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        File.Copy(seedPath, absolutePath, overwrite: true);
        return relativePath;
    }

    private sealed record ListingSpec(
        string Title,
        string BrandTitle,
        string CategoryTitle,
        string Description,
        string Warranty,
        decimal Price,
        int Stock,
        bool IsUsed,
        decimal? Discount,
        string SeedImage);
}
