using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ProductCategoryPrice;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class ProductCategoryPriceService(LabConnectDbContext context) : IProductCategoryPriceService
{
    public async Task<OperationResult<List<ProductCategoryPriceItemDto>>> GetAllAsync()
    {
        var prices = await context.ProductCategoryPrices.AsNoTracking()
            .ToDictionaryAsync(x => x.ProductCategoryId, x => x);

        var categories = await context.ProductCategories.AsNoTracking()
            .Include(x => x.CategoryGroup)
            .OrderBy(x => x.CategoryGroup != null ? x.CategoryGroup.Name : "")
            .ThenBy(x => x.Title)
            .ToListAsync();

        var items = categories.Select(c =>
        {
            prices.TryGetValue(c.ProductCategoryId, out var price);
            return new ProductCategoryPriceItemDto
            {
                ProductCategoryId = c.ProductCategoryId,
                CategoryTitle = c.Title,
                ProductCategoryGroupId = c.ProductCategoryGroupId,
                GroupTitle = c.CategoryGroup?.Name ?? string.Empty,
                Price = price?.Price,
                UpdatedAt = price?.UpdatedAt,
            };
        }).ToList();

        return OperationResult<List<ProductCategoryPriceItemDto>>.Success(items);
    }

    public async Task<OperationResult<List<ProductCategoryPriceItemDto>>> SaveAllAsync(
        Guid userId,
        SaveProductCategoryPricesCommand command)
    {
        var items = command.Items ?? [];
        if (items.Any(x => x.Price < 0))
            return OperationResult<List<ProductCategoryPriceItemDto>>.Failure("قیمت نمی‌تواند منفی باشد");

        var categoryIds = items.Select(x => x.ProductCategoryId).Distinct().ToList();
        var existingCategoryIds = await context.ProductCategories
            .Where(x => categoryIds.Contains(x.ProductCategoryId))
            .Select(x => x.ProductCategoryId)
            .ToListAsync();

        if (existingCategoryIds.Count != categoryIds.Count)
            return OperationResult<List<ProductCategoryPriceItemDto>>.Failure("یکی از دسته‌بندی‌ها یافت نشد");

        var now = DateTime.UtcNow.ToLocalTime();
        var existingPrices = await context.ProductCategoryPrices
            .Where(x => categoryIds.Contains(x.ProductCategoryId))
            .ToDictionaryAsync(x => x.ProductCategoryId);

        foreach (var item in items)
        {
            if (existingPrices.TryGetValue(item.ProductCategoryId, out var entity))
            {
                entity.Price = item.Price;
                entity.UpdatedAt = now;
                entity.UpdatedByUserId = userId;
            }
            else
            {
                context.ProductCategoryPrices.Add(new ProductCategoryPrice
                {
                    ProductCategoryId = item.ProductCategoryId,
                    Price = item.Price,
                    UpdatedAt = now,
                    UpdatedByUserId = userId,
                });
            }
        }

        await context.SaveChangesAsync();
        return await GetAllAsync();
    }
}
