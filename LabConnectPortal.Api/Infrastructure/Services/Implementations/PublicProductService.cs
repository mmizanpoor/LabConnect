using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Helpers;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Product;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class PublicProductService(LabConnectDbContext context, IConfiguration configuration) : IPublicProductService
{
    private string IpHashPepper =>
        configuration["ViewTracking:IpHashPepper"]
        ?? configuration["JwtSettings:Secret"]
        ?? "LabConnectPortal-View-Tracking-Pepper";

    public async Task<OperationResult<PagedResult<PublicProductListingCardDto>>> GetPublishedProductsAsync(GetPublishedListingsQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 100);
        var sortBy = query.SortBy?.Trim().ToLowerInvariant() ?? "newest";

        var dbQuery = context.Products.AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.CategoryAssignments)
                .ThenInclude(a => a.Category)
                    .ThenInclude(c => c!.CategoryGroup)
            .Include(p => p.CreatedByCenterProfile)
            .Where(p =>
                p.Status == ProductStatus.Approved &&
                p.StockQuantity > 0);

        if (!string.IsNullOrWhiteSpace(query.Title))
            dbQuery = dbQuery.Where(p => p.Title.Contains(query.Title.Trim()));
        var brandIds = (query.BrandIds ?? [])
            .Where(id => id > 0)
            .Distinct()
            .ToList();
        if (query.BrandId is > 0 && !brandIds.Contains(query.BrandId.Value))
            brandIds.Add(query.BrandId.Value);
        if (brandIds.Count == 1)
            dbQuery = dbQuery.Where(p => p.BrandId == brandIds[0]);
        else if (brandIds.Count > 1)
            dbQuery = dbQuery.Where(p => p.BrandId.HasValue && brandIds.Contains(p.BrandId.Value));
        if (query.CategoryId.HasValue)
            dbQuery = dbQuery.Where(p => p.CategoryAssignments.Any(a => a.ProductCategoryId == query.CategoryId.Value));
        if (query.CategoryGroupId.HasValue)
            dbQuery = dbQuery.Where(p => p.CategoryAssignments.Any(a =>
                a.Category != null && a.Category.ProductCategoryGroupId == query.CategoryGroupId.Value));

        if (query.ProvinceId.HasValue)
        {
            var provinceId = query.ProvinceId.Value;
            dbQuery = dbQuery.Where(p =>
                p.ProvinceId == provinceId ||
                (
                    p.CreatedByCenterProfile != null &&
                    p.CreatedByCenterProfile.OwnerUserId != null &&
                    (
                        context.OrganizationLocations.Any(l =>
                            l.UserId == p.CreatedByCenterProfile.OwnerUserId &&
                            l.ProvinceId == provinceId) ||
                        context.UserProfiles.Any(up =>
                            up.UserId == p.CreatedByCenterProfile.OwnerUserId &&
                            up.ProvinceId == provinceId)
                    )
                ));
        }

        var attributeFilters = query.AttributeFilters
            .Where(f => f.ProductAttributeId > 0 && !string.IsNullOrWhiteSpace(f.Value))
            .Select(f => new { f.ProductAttributeId, Value = f.Value.Trim() })
            .ToList();

        if (attributeFilters.Count > 0)
        {
            var filterIds = attributeFilters.Select(f => f.ProductAttributeId).Distinct().ToList();
            var titlesById = await context.ProductAttributes.AsNoTracking()
                .Where(a => filterIds.Contains(a.ProductAttributeId))
                .Select(a => new { a.ProductAttributeId, a.Title })
                .ToDictionaryAsync(a => a.ProductAttributeId, a => a.Title);

            foreach (var filter in attributeFilters)
            {
                var value = filter.Value;
                if (!titlesById.TryGetValue(filter.ProductAttributeId, out var title)
                    || string.IsNullOrWhiteSpace(title))
                {
                    var attributeId = filter.ProductAttributeId;
                    dbQuery = dbQuery.Where(p =>
                        p.AttributeValues.Any(v =>
                            v.ProductAttributeId == attributeId &&
                            v.Value.Contains(value)));
                    continue;
                }

                dbQuery = dbQuery.Where(p =>
                    p.AttributeValues.Any(v =>
                        v.Attribute.Title == title &&
                        v.Value.Contains(value)));
            }
        }

        if (query.MinPrice.HasValue)
            dbQuery = dbQuery.Where(p =>
                !p.IsNegotiablePrice &&
                p.Price * (100m - (p.DiscountPercent ?? 0m)) / 100m >= query.MinPrice.Value);
        if (query.MaxPrice.HasValue)
            dbQuery = dbQuery.Where(p =>
                !p.IsNegotiablePrice &&
                p.Price * (100m - (p.DiscountPercent ?? 0m)) / 100m <= query.MaxPrice.Value);

        if (sortBy == "views")
            dbQuery = dbQuery.Where(p => p.ViewCount > 0);

        var totalCount = await dbQuery.CountAsync();

        var orderedQuery = sortBy switch
        {
            "views" => dbQuery
                .OrderByDescending(p => p.ViewCount)
                .ThenByDescending(p => p.ApprovedAt ?? p.UpdatedAt),
            "discount" => dbQuery
                .OrderByDescending(p => p.IsNegotiablePrice ? 0 : (p.DiscountPercent ?? 0))
                .ThenByDescending(p => p.ApprovedAt ?? p.UpdatedAt),
            "price_asc" => dbQuery
                .OrderBy(p => p.IsNegotiablePrice)
                .ThenBy(p => p.Price * (100m - (p.DiscountPercent ?? 0m)) / 100m),
            "price_desc" => dbQuery
                .OrderBy(p => p.IsNegotiablePrice)
                .ThenByDescending(p => p.Price * (100m - (p.DiscountPercent ?? 0m)) / 100m),
            _ => dbQuery.OrderByDescending(p => p.ApprovedAt ?? p.UpdatedAt),
        };

        var products = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = products.Select(p =>
        {
            var finalPrice = p.IsNegotiablePrice ? 0 : CartService.CalculateFinalPrice(p.Price, p.DiscountPercent);
            var category = p.CategoryAssignments
                .Where(a => a.Category != null)
                .Select(a => a.Category!)
                .FirstOrDefault();
            return new PublicProductListingCardDto
            {
                ProductId = p.ProductId,
                Title = p.Title,
                BrandTitle = p.Brand != null ? p.Brand.Title : string.Empty,
                CenterName = p.CreatedByCenterProfile?.Name ?? string.Empty,
                CenterProfileId = p.CreatedByCenterProfileId ?? Guid.Empty,
                HasCenterLogo = p.CreatedByCenterProfile is { IsApproved: true, LogoPath: not null and not "" },
                CategoryGroupTitle = category?.CategoryGroup?.Name,
                CategoryTitle = category?.Title,
                Price = p.Price,
                DiscountPercent = p.IsNegotiablePrice ? null : p.DiscountPercent,
                FinalPrice = finalPrice,
                IsNegotiablePrice = p.IsNegotiablePrice,
                IsUsed = p.IsUsed,
                ImagePath = ProductListingImageHelper.ResolvePrimaryImagePath(p),
                PublishedAt = p.ApprovedAt,
                AcceptsResume = category!.AcceptsResume
            };
        }).ToList();

        return OperationResult<PagedResult<PublicProductListingCardDto>>.Success(new PagedResult<PublicProductListingCardDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    public async Task<OperationResult<PublicProductListingDetailDto>> GetProductDetailAsync(Guid productId)
    {
        var product = await LoadPublishedProductAsync(productId);
        if (product == null)
            return OperationResult<PublicProductListingDetailDto>.Failure("محصول یافت نشد");

        var reviews = await context.ProductUserReviews.AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Replies).ThenInclude(reply => reply.User)
            .Where(r => r.ProductId == product.ProductId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Take(20)
            .ToListAsync();

        var finalPrice = product.IsNegotiablePrice
            ? 0
            : CartService.CalculateFinalPrice(product.Price, product.DiscountPercent);

        return OperationResult<PublicProductListingDetailDto>.Success(new PublicProductListingDetailDto
        {
            ProductId = product.ProductId,
            Title = product.Title,
            BrandTitle = product.Brand?.Title ?? string.Empty,
            CenterName = product.CreatedByCenterProfile?.Name ?? string.Empty,
            CenterProfileId = product.CreatedByCenterProfileId ?? Guid.Empty,
            HasCenterLogo = product.CreatedByCenterProfile is { IsApproved: true, LogoPath: not null and not "" },
            Warranty = product.Warranty,
            Description = product.Description,
            Price = product.Price,
            DiscountPercent = product.IsNegotiablePrice ? null : product.DiscountPercent,
            FinalPrice = finalPrice,
            IsNegotiablePrice = product.IsNegotiablePrice,
            IsUsed = product.IsUsed,
            StockQuantity = product.StockQuantity,
            PublishedAt = product.ApprovedAt,
            CategoryTitles = product.CategoryAssignments
                .Where(a => a.Category != null)
                .Select(a => a.Category.Title)
                .ToList(),
            AttributeValues = product.AttributeValues
                .Where(v => v.Attribute != null)
                .Select(v => new ProductAttributeValueDto
                {
                    ProductAttributeId = v.ProductAttributeId,
                    AttributeTitle = v.Attribute.Title,
                    ProductCategoryId = v.Attribute.ProductCategoryId,
                    CategoryTitle = v.Attribute.Category?.Title ?? string.Empty,
                    Value = v.Value,
                    FieldType = v.Attribute.FieldType,
                }).ToList(),
            Images = ProductListingImageHelper.ResolveDisplayImages(product),
            ExpertReviews = product.ExpertReviews.OrderBy(r => r.SortOrder).Select(r => new ProductExpertReviewDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                SortOrder = r.SortOrder,
            }).ToList(),
            UserReviews = reviews.Select(r => new ProductUserReviewDto
            {
                Id = r.Id,
                AuthorName = FormatPublicUserName(r.User),
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                IsApproved = r.IsApproved,
                ProductTitle = product.Title,
                Replies = r.Replies.OrderBy(reply => reply.CreatedAt).Select(reply => new ProductReviewReplyDto
                {
                    Id = reply.Id,
                    AuthorName = FormatPublicUserName(reply.User),
                    Comment = reply.Comment,
                    CreatedAt = reply.CreatedAt,
                }).ToList(),
            }).ToList(),
            Latitude = product.Latitude,
            Longitude = product.Longitude,
            ProvinceId = product.ProvinceId,
            ProvinceName = product.Province?.ProvinceName,
            AcceptsResume = product.CategoryAssignments.Any(a => a.Category != null && a.Category.AcceptsResume),
        });
    }

    private async Task<Product?> LoadPublishedProductAsync(Guid productId)
        => await context.Products.AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.Province)
            .Include(p => p.CategoryAssignments).ThenInclude(a => a.Category)
            .Include(p => p.AttributeValues).ThenInclude(v => v.Attribute).ThenInclude(a => a!.Category)
            .Include(p => p.Images)
            .Include(p => p.ExpertReviews)
            .Include(p => p.CreatedByCenterProfile)
            .FirstOrDefaultAsync(p =>
                p.ProductId == productId &&
                p.Status == ProductStatus.Approved);

    private static string FormatPublicUserName(User? user)
    {
        if (user == null) return string.Empty;
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.Username : fullName;
    }

    public async Task<OperationResult<List<ProductSearchSuggestionDto>>> SearchSuggestionsAsync(SearchProductSuggestionsQuery query)
    {
        var term = query.Term?.Trim() ?? string.Empty;
        if (term.Length < 2)
            return OperationResult<List<ProductSearchSuggestionDto>>.Success([]);

        var limit = query.Limit < 1 ? 8 : Math.Min(query.Limit, 20);
        var categoryLimit = Math.Min(3, limit / 2);
        var productLimit = limit - categoryLimit;

        var publishedProducts = context.Products.AsNoTracking()
            .Where(p =>
                p.Status == ProductStatus.Approved &&
                p.StockQuantity > 0);

        var categories = await context.ProductCategories.AsNoTracking()
            .Where(c => c.Title.Contains(term))
            .Where(c => c.ProductAssignments.Any(a =>
                a.Product.Status == ProductStatus.Approved &&
                a.Product.StockQuantity > 0))
            .OrderBy(c => c.Title)
            .Take(categoryLimit)
            .Select(c => new ProductSearchSuggestionDto
            {
                Type = "category",
                Title = c.Title,
                CategoryId = c.ProductCategoryId,
            })
            .ToListAsync();

        var products = await publishedProducts
            .Include(p => p.CategoryAssignments).ThenInclude(a => a.Category)
            .Where(p => p.Title.Contains(term))
            .OrderByDescending(p => p.ApprovedAt ?? p.UpdatedAt)
            .Take(productLimit)
            .ToListAsync();

        var productSuggestions = products.Select(p =>
        {
            var categoryTitles = p.CategoryAssignments
                .Select(a => a.Category.Title)
                .Distinct()
                .ToList();

            return new ProductSearchSuggestionDto
            {
                Type = "product",
                Title = p.Title,
                CategoryLabel = categoryTitles.Count > 0 ? string.Join("، ", categoryTitles) : null,
                ProductId = p.ProductId,
            };
        }).ToList();

        var suggestions = new List<ProductSearchSuggestionDto>(categories.Count + productSuggestions.Count);
        suggestions.AddRange(categories);
        suggestions.AddRange(productSuggestions);

        return OperationResult<List<ProductSearchSuggestionDto>>.Success(suggestions);
    }

    public async Task<OperationResult<SellerContactDto>> GetSellerContactAsync(Guid productId)
    {
        var product = await context.Products.AsNoTracking()
            .Include(p => p.CreatedByCenterProfile)
            .FirstOrDefaultAsync(p =>
                p.ProductId == productId &&
                p.Status == ProductStatus.Approved &&
                p.CreatedByCenterProfileId != null);

        if (product?.CreatedByCenterProfile == null)
            return OperationResult<SellerContactDto>.Failure("اطلاعات تماس برای این محصول در دسترس نیست");

        var mobile = await ResolveSellerMobileAsync(product.CreatedByCenterProfile);

        return OperationResult<SellerContactDto>.Success(new SellerContactDto
        {
            Phone = product.CreatedByCenterProfile.Phone,
            MobileNumber = mobile ?? string.Empty,
        });
    }

    private async Task<string?> ResolveSellerMobileAsync(CenterProfile profile)
    {
        if (profile.CenterType == CenterType.Store && profile.OwnerUserId.HasValue)
        {
            return await context.Users.AsNoTracking()
                .Where(u => u.Id == profile.OwnerUserId.Value)
                .Select(u => u.MobileNumber)
                .FirstOrDefaultAsync();
        }

        if (profile.CenterType == CenterType.Lab)
        {
            return await context.Users.AsNoTracking()
                .Where(u => u.UserType == UserType.AdminLab && u.CenterProfileId == profile.Id)
                .Select(u => u.MobileNumber)
                .FirstOrDefaultAsync();
        }

        return null;
    }

    public async Task<OperationResult<List<PublicBrandCardDto>>> GetHomeBrandsAsync()
    {
        var items = await context.Brands.AsNoTracking()
            .Where(b => b.ShowOnHomePage
                && b.ImagePath != null && b.ImagePath != string.Empty)
            .OrderBy(b => b.Title)
            .Select(b => new PublicBrandCardDto
            {
                BrandId = b.BrandId,
                Title = b.Title,
                ImagePath = b.ImagePath!,
            })
            .ToListAsync();

        return OperationResult<List<PublicBrandCardDto>>.Success(items);
    }

    public async Task<OperationResult<List<PublicCategoryGroupCardDto>>> GetHomeCategoriesAsync()
    {
        var items = await context.ProductCategoryGroups.AsNoTracking()
            .Where(g => g.ShowOnHomePage
                && g.HomePageImagePath != null && g.HomePageImagePath != string.Empty)
            .OrderBy(g => g.Name)
            .Select(g => new PublicCategoryGroupCardDto
            {
                ProductCategoryGroupId = g.ProductCategoryGroupId,
                Title = g.Name,
                HomePageImagePath = g.HomePageImagePath!,
            })
            .ToListAsync();

        return OperationResult<List<PublicCategoryGroupCardDto>>.Success(items);
    }

    public async Task<OperationResult<PublicListingFiltersDto>> GetListingFiltersAsync()
    {
        var publishedProducts = context.Products.AsNoTracking()
            .Where(p =>
                p.Status == ProductStatus.Approved &&
                p.StockQuantity > 0);

        var brands = await context.Brands.AsNoTracking()
            .Where(b => publishedProducts.Any(p => p.BrandId == b.BrandId))
            .OrderBy(b => b.Title)
            .Select(b => new PublicBrandFilterDto
            {
                BrandId = b.BrandId,
                Title = b.Title,
            })
            .ToListAsync();

        var categories = await context.ProductCategories.AsNoTracking()
            .Where(c => publishedProducts.Any(p =>
                p.CategoryAssignments.Any(a => a.ProductCategoryId == c.ProductCategoryId)))
            .OrderBy(c => c.CategoryGroup != null ? c.CategoryGroup.Name : "")
            .ThenBy(c => c.Title)
            .Select(c => new PublicCategoryFilterDto
            {
                ProductCategoryId = c.ProductCategoryId,
                Title = c.Title,
                ProductCategoryGroupId = c.ProductCategoryGroupId,
                CategoryGroupTitle = c.CategoryGroup != null ? c.CategoryGroup.Name : null,
            })
            .ToListAsync();

        var categoryGroups = categories
            .Where(c => c.ProductCategoryGroupId.HasValue && !string.IsNullOrWhiteSpace(c.CategoryGroupTitle))
            .GroupBy(c => c.ProductCategoryGroupId!.Value)
            .Select(g => new PublicCategoryGroupFilterDto
            {
                ProductCategoryGroupId = g.Key,
                Title = g.First().CategoryGroupTitle!,
            })
            .OrderBy(g => g.Title)
            .ToList();

        var provinces = await context.Provinces.AsNoTracking()
            .OrderBy(p => p.ProvinceName)
            .Select(p => new PublicProvinceFilterDto
            {
                ProvinceId = p.ProvinceId,
                Name = p.ProvinceName,
            })
            .ToListAsync();

        var finalPrices = await publishedProducts
            .Where(p => !p.IsNegotiablePrice && p.Price > 0)
            .Select(p => p.Price * (100m - (p.DiscountPercent ?? 0m)) / 100m)
            .ToListAsync();

        decimal? minPrice = null;
        decimal? maxPrice = null;
        if (finalPrices.Count > 0)
        {
            minPrice = finalPrices.Min();
            maxPrice = finalPrices.Max();
        }
        else if (await publishedProducts.AnyAsync())
        {
            minPrice = 0;
            maxPrice = 100_000_000;
        }

        return OperationResult<PublicListingFiltersDto>.Success(new PublicListingFiltersDto
        {
            Brands = brands,
            CategoryGroups = categoryGroups,
            Categories = categories,
            Provinces = provinces,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
        });
    }

    public async Task<OperationResult<List<PublicFilterAttributeDto>>> GetFilterAttributesByGroupAsync(
        GetFilterAttributesByGroupQuery query)
    {
        if (query.CategoryGroupId < 1)
            return OperationResult<List<PublicFilterAttributeDto>>.Success([]);

        var attributesQuery = context.ProductAttributes.AsNoTracking()
            .Include(a => a.Category)
            .Where(a =>
                a.Category != null &&
                a.Category.ProductCategoryGroupId == query.CategoryGroupId);

        if (query.CategoryId.HasValue)
            attributesQuery = attributesQuery.Where(a => a.ProductCategoryId == query.CategoryId.Value);

        var items = await attributesQuery
            .OrderBy(a => a.Title)
            .ThenBy(a => a.ProductAttributeId)
            .Select(a => new PublicFilterAttributeDto
            {
                ProductAttributeId = a.ProductAttributeId,
                Title = a.Title,
                ProductCategoryId = a.ProductCategoryId,
                CategoryTitle = a.Category != null ? a.Category.Title : string.Empty,
                FieldType = a.FieldType,
            })
            .ToListAsync();

        return OperationResult<List<PublicFilterAttributeDto>>.Success(DeduplicateFilterAttributes(items));
    }

    public async Task<OperationResult<List<PublicBrandFilterDto>>> GetFilterBrandsByGroupAsync(
        GetFilterAttributesByGroupQuery query)
    {
        if (query.CategoryGroupId < 1)
            return OperationResult<List<PublicBrandFilterDto>>.Success([]);

        var productsQuery = context.Products.AsNoTracking()
            .Where(p =>
                p.Status == ProductStatus.Approved &&
                p.StockQuantity > 0 &&
                p.CategoryAssignments.Any(a =>
                    a.Category != null &&
                    a.Category.ProductCategoryGroupId == query.CategoryGroupId));

        if (query.CategoryId.HasValue)
            productsQuery = productsQuery.Where(p =>
                p.CategoryAssignments.Any(a => a.ProductCategoryId == query.CategoryId.Value));

        var items = await context.Brands.AsNoTracking()
            .Where(b => productsQuery.Any(p => p.BrandId == b.BrandId))
            .OrderBy(b => b.Title)
            .Select(b => new PublicBrandFilterDto
            {
                BrandId = b.BrandId,
                Title = b.Title,
            })
            .ToListAsync();

        return OperationResult<List<PublicBrandFilterDto>>.Success(items);
    }

    public async Task<OperationResult> RecordProductViewAsync(Guid productId, string? clientIp)
    {
        if (string.IsNullOrWhiteSpace(clientIp))
            return OperationResult.SuccessResult();

        var productExists = await context.Products.AsNoTracking()
            .AnyAsync(p => p.ProductId == productId && p.Status == ProductStatus.Approved);

        if (!productExists)
            return OperationResult.Failure("محصول یافت نشد");

        var ipHash = ClientIpHelper.HashIp(clientIp, IpHashPepper);
        var alreadyViewed = await context.ProductViews.AsNoTracking()
            .AnyAsync(v => v.ProductId == productId && v.IpHash == ipHash);

        if (alreadyViewed)
            return OperationResult.SuccessResult();

        context.ProductViews.Add(new ProductView
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            IpHash = ipHash,
            ViewedAt = DateTime.UtcNow,
        });

        var product = await context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
        if (product == null)
            return OperationResult.Failure("محصول یافت نشد");

        product.ViewCount++;

        try
        {
            await context.SaveChangesAsync();
            return OperationResult.SuccessResult();
        }
        catch (DbUpdateException)
        {
            return OperationResult.SuccessResult();
        }
    }

    private static List<PublicFilterAttributeDto> DeduplicateFilterAttributes(
        IEnumerable<PublicFilterAttributeDto> items)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<PublicFilterAttributeDto>();

        foreach (var item in items)
        {
            var key = NormalizeAttributeTitle(item.Title);
            if (key.Length == 0 || key == "استان" || key == "برند" || !seen.Add(key))
                continue;

            result.Add(item);
        }

        return result;
    }

    private static string NormalizeAttributeTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        return string.Join(
            " ",
            title.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
