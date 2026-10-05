using LabConnectPortal.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Context;

public static class ProductCatalogReferenceDataSeeder
{
    private const string StorageRoot = "uploads";

    private static readonly string[] ExpectedGroupNames =
    [
        "کاریابی و استخدام",
        "کیت و مواد مصرفی",
        "تجهیزات آزمایشگاهی",
        "خدمات و بازرگانی آزمایشگاه",
    ];

    private static readonly Dictionary<string, string[]> GroupCategories = new()
    {
        ["کاریابی و استخدام"] =
        [
            "استخدام نیروی انسانی",
            "استخدام مسئول فنی آزمایشگاه",
            "استخدام مسئول فنی شرکت",
            "کاریابی مسئول فنی آزمایشگاه",
            "کاریابی نیروی انسانی",
            "کاریابی مسئول فنی شرکت",
        ],
        ["کیت و مواد مصرفی"] =
        [
            "کیت های آزمایشگاهی",
            "مواد مصرفی",
        ],
        ["تجهیزات آزمایشگاهی"] =
        [
            "تجهیزات عمومی",
            "تجهیزات سنجشی",
            "تجهیزات پایه آزمایشگاهی",
            "قطعات و لوازم جانبی",
            "خدمات تجهیزات",
        ],
        ["خدمات و بازرگانی آزمایشگاه"] =
        [
            "مشارکت در راه اندازی",
            "اجاره آزمایشگاه",
            "فروش آزمایشگاه",
            "انجام تست های آزمایشگاهی",
            "مشاوره و راه اندازی",
        ],
    };

    private static readonly Dictionary<string, string> GroupSeedImages = new()
    {
        ["کاریابی و استخدام"] = "recruitment.svg",
        ["کیت و مواد مصرفی"] = "kits-consumables.svg",
        ["تجهیزات آزمایشگاهی"] = "equipment.svg",
        ["خدمات و بازرگانی آزمایشگاه"] = "lab-services.svg",
    };

    public static async Task EnsureSeededAsync(LabConnectDbContext context, string contentRootPath)
    {
        if (await context.Products.AnyAsync())
        {
            await EnsureGroupImagesAsync(context, contentRootPath);
            await EnsureAttributesAsync(context);
            await EnsureBrandsAsync(context, contentRootPath);
            return;
        }

        if (!await IsCurrentCatalogAsync(context))
            await ResetAndSeedAsync(context, contentRootPath);
        else
            await EnsureGroupImagesAsync(context, contentRootPath);

        await EnsureAttributesAsync(context);
        await EnsureBrandsAsync(context, contentRootPath);
    }

    private static async Task<bool> IsCurrentCatalogAsync(LabConnectDbContext context)
    {
        var groups = await context.ProductCategoryGroups.AsNoTracking()
            .OrderBy(g => g.ProductCategoryGroupId)
            .Select(g => g.Name)
            .ToListAsync();

        if (groups.Count != ExpectedGroupNames.Length)
            return false;

        for (var i = 0; i < ExpectedGroupNames.Length; i++)
        {
            if (!string.Equals(groups[i], ExpectedGroupNames[i], StringComparison.Ordinal))
                return false;
        }

        var expectedCategoryCount = GroupCategories.Values.Sum(c => c.Length);
        var actualCategories = await context.ProductCategories.AsNoTracking()
            .Select(c => new { c.Title, GroupName = c.CategoryGroup!.Name })
            .ToListAsync();

        if (actualCategories.Count != expectedCategoryCount)
            return false;

        foreach (var (groupName, categories) in GroupCategories)
        {
            foreach (var categoryTitle in categories)
            {
                if (!actualCategories.Any(c =>
                        c.GroupName == groupName &&
                        string.Equals(c.Title, categoryTitle, StringComparison.Ordinal)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static async Task ResetAndSeedAsync(LabConnectDbContext context, string contentRootPath)
    {
        // Hard stop: this method wipes the whole catalog. It may only build an empty database,
        // never touch data that was entered by hand.
        if (await context.Products.AnyAsync()
            || await context.ProductCategoryGroups.AnyAsync()
            || await context.Brands.AnyAsync())
        {
            return;
        }

        await context.ProductReviewReplies.ExecuteDeleteAsync();
        await context.ProductUserReviews.ExecuteDeleteAsync();
        await context.Database.ExecuteSqlRawAsync(
            "UPDATE [dbo].[ProductOrderItems] SET [ProductId] = NULL WHERE [ProductId] IS NOT NULL");
        await context.CartItems.Where(x => x.ProductId != null).ExecuteDeleteAsync();
        await context.ProductViews.ExecuteDeleteAsync();
        await context.ProductExpertReviews.ExecuteDeleteAsync();
        await context.ProductImages.ExecuteDeleteAsync();
        await context.ProductAttributeValues.ExecuteDeleteAsync();
        await context.ProductCategoryAssignments.ExecuteDeleteAsync();
        await context.Products.ExecuteDeleteAsync();
        await context.ProductAttributes.ExecuteDeleteAsync();
        await context.ProductCategories.ExecuteDeleteAsync();
        await context.ProductCategoryGroups.ExecuteDeleteAsync();
        await context.Brands.ExecuteDeleteAsync();

        foreach (var groupName in ExpectedGroupNames)
        {
            var group = new ProductCategoryGroup
            {
                Name = groupName,
                ShowOnHomePage = true,
            };
            context.ProductCategoryGroups.Add(group);

            foreach (var categoryTitle in GroupCategories[groupName])
            {
                context.ProductCategories.Add(new ProductCategory
                {
                    Title = categoryTitle,
                    CategoryGroup = group,
                });
            }
        }

        await context.SaveChangesAsync();
        await EnsureGroupImagesAsync(context, contentRootPath);
        await EnsureAttributesAsync(context);
    }

    private static async Task EnsureAttributesAsync(LabConnectDbContext context)
    {
        if (await AreAttributesCurrentAsync(context))
            return;

        var categories = await context.ProductCategories
            .Include(c => c.Attributes)
            .ToListAsync();

        var changed = false;
        foreach (var category in categories)
        {
            if (!ProductCatalogAttributeSeeds.ByCategory.TryGetValue(category.Title, out var expected))
                continue;

            var expectedTitles = expected.Select(a => a.Title).ToHashSet(StringComparer.Ordinal);
            var currentTitles = category.Attributes.Select(a => a.Title).ToHashSet(StringComparer.Ordinal);

            if (expectedTitles.SetEquals(currentTitles))
                continue;

            if (category.Attributes.Count > 0)
            {
                var attributeIds = category.Attributes.Select(a => a.ProductAttributeId).ToList();
                await context.ProductAttributeValues
                    .Where(v => attributeIds.Contains(v.ProductAttributeId))
                    .ExecuteDeleteAsync();
                context.ProductAttributes.RemoveRange(category.Attributes);
                category.Attributes.Clear();
            }

            foreach (var seed in expected)
            {
                context.ProductAttributes.Add(new ProductAttribute
                {
                    ProductCategoryId = category.ProductCategoryId,
                    Title = seed.Title,
                    FieldType = seed.FieldType,
                });
            }

            changed = true;
        }

        if (changed)
            await context.SaveChangesAsync();
    }

    private static async Task<bool> AreAttributesCurrentAsync(LabConnectDbContext context)
    {
        var categories = await context.ProductCategories.AsNoTracking()
            .Select(c => new
            {
                c.Title,
                AttributeTitles = c.Attributes.Select(a => a.Title).ToList(),
            })
            .ToListAsync();

        if (categories.Count != ProductCatalogAttributeSeeds.ByCategory.Count)
            return false;

        foreach (var (categoryTitle, expected) in ProductCatalogAttributeSeeds.ByCategory)
        {
            var category = categories.FirstOrDefault(c => c.Title == categoryTitle);
            if (category == null)
                return false;

            var expectedTitles = expected.Select(a => a.Title).ToHashSet(StringComparer.Ordinal);
            var actualTitles = category.AttributeTitles.ToHashSet(StringComparer.Ordinal);
            if (!expectedTitles.SetEquals(actualTitles))
                return false;
        }

        return true;
    }

    private static async Task EnsureGroupImagesAsync(LabConnectDbContext context, string contentRootPath)
    {
        var groups = await context.ProductCategoryGroups.ToListAsync();
        var changed = false;

        foreach (var group in groups)
        {
            var imagePath = CopySeedImage(contentRootPath, group);
            if (imagePath == null)
                continue;

            if (!string.Equals(group.HomePageImagePath, imagePath, StringComparison.OrdinalIgnoreCase))
            {
                TryDeleteGroupImage(contentRootPath, group.HomePageImagePath);
                group.HomePageImagePath = imagePath;
                group.ShowOnHomePage = true;
                changed = true;
            }
            else if (!group.ShowOnHomePage)
            {
                group.ShowOnHomePage = true;
                changed = true;
            }
        }

        if (changed)
            await context.SaveChangesAsync();
    }

    private static string? CopySeedImage(string contentRootPath, ProductCategoryGroup group)
    {
        if (!GroupSeedImages.TryGetValue(group.Name, out var seedFileName))
            return null;

        var seedPath = Path.Combine(contentRootPath, "SeedAssets", "category-groups", seedFileName);
        if (!File.Exists(seedPath))
            return null;

        var extension = Path.GetExtension(seedFileName).TrimStart('.').ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
            extension = "svg";

        var fileName = $"home.{extension}";
        var relativePath = Path.Combine(StorageRoot, "category-groups", group.ProductCategoryGroupId.ToString(), fileName)
            .Replace('\\', '/');
        var absolutePath = Path.Combine(contentRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        File.Copy(seedPath, absolutePath, overwrite: true);

        return relativePath;
    }

    private static void TryDeleteGroupImage(string contentRootPath, string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        var absolutePath = Path.Combine(contentRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);
    }

    private static async Task EnsureBrandsAsync(LabConnectDbContext context, string contentRootPath)
    {
        if (await AreBrandsCurrentAsync(context))
            return;

        var existingBrands = await context.Brands.ToListAsync();
        var expectedTitles = ProductCatalogBrandSeeds.All
            .Select(b => b.Title)
            .ToHashSet(StringComparer.Ordinal);

        var existingTitles = existingBrands
            .Select(b => b.Title)
            .ToHashSet(StringComparer.Ordinal);

        if (!expectedTitles.SetEquals(existingTitles))
        {
            if (await context.Products.AnyAsync())
                return;

            await context.Brands.ExecuteDeleteAsync();
            existingBrands = [];
        }

        var brandsByTitle = existingBrands.ToDictionary(b => b.Title, StringComparer.Ordinal);
        var changed = false;

        foreach (var seed in ProductCatalogBrandSeeds.All)
        {
            if (!brandsByTitle.TryGetValue(seed.Title, out var brand))
            {
                brand = new Brand
                {
                    Title = seed.Title,
                    ShowOnHomePage = seed.ShowOnHomePage,
                };
                context.Brands.Add(brand);
                brandsByTitle[seed.Title] = brand;
                changed = true;
            }
            else if (brand.ShowOnHomePage != seed.ShowOnHomePage)
            {
                brand.ShowOnHomePage = seed.ShowOnHomePage;
                changed = true;
            }
        }

        if (changed)
            await context.SaveChangesAsync();

        var brands = await context.Brands.ToListAsync();
        changed = false;

        foreach (var seed in ProductCatalogBrandSeeds.All)
        {
            var brand = brands.First(b => b.Title == seed.Title);
            var imagePath = CopyBrandSeedImage(contentRootPath, brand.BrandId, seed.SeedFileName);
            if (imagePath == null)
                continue;

            if (!string.Equals(brand.ImagePath, imagePath, StringComparison.OrdinalIgnoreCase))
            {
                TryDeleteBrandImage(contentRootPath, brand.ImagePath);
                brand.ImagePath = imagePath;
                changed = true;
            }
        }

        if (changed)
            await context.SaveChangesAsync();
    }

    private static async Task<bool> AreBrandsCurrentAsync(LabConnectDbContext context)
    {
        var brands = await context.Brands.AsNoTracking()
            .Select(b => new { b.Title, b.ImagePath, b.ShowOnHomePage })
            .ToListAsync();

        if (brands.Count != ProductCatalogBrandSeeds.All.Length)
            return false;

        foreach (var seed in ProductCatalogBrandSeeds.All)
        {
            var brand = brands.FirstOrDefault(b => b.Title == seed.Title);
            if (brand == null)
                return false;

            if (string.IsNullOrWhiteSpace(brand.ImagePath))
                return false;

            var expectedExt = Path.GetExtension(seed.SeedFileName);
            if (!brand.ImagePath.EndsWith(expectedExt, StringComparison.OrdinalIgnoreCase)
                && !brand.ImagePath.EndsWith($"/logo{expectedExt}", StringComparison.OrdinalIgnoreCase))
                return false;

            if (brand.ShowOnHomePage != seed.ShowOnHomePage)
                return false;
        }

        return true;
    }

    private static string? CopyBrandSeedImage(string contentRootPath, int brandId, string seedFileName)
    {
        var seedPath = Path.Combine(contentRootPath, "SeedAssets", "brands", seedFileName);
        if (!File.Exists(seedPath))
            return null;

        var extension = Path.GetExtension(seedFileName).TrimStart('.').ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
            extension = "jpg";

        // Stable filename so restarts overwrite the same logo path.
        var fileName = $"logo.{extension}";
        var relativePath = Path.Combine(StorageRoot, "brands", brandId.ToString(), fileName)
            .Replace('\\', '/');
        var absolutePath = Path.Combine(contentRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        File.Copy(seedPath, absolutePath, overwrite: true);

        return relativePath;
    }

    private static void TryDeleteBrandImage(string contentRootPath, string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        var absolutePath = Path.Combine(contentRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);
    }
}
