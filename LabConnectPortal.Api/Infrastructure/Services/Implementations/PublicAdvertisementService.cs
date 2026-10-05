using System.Text.RegularExpressions;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Advertisement;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public partial class PublicAdvertisementService(LabConnectDbContext context) : IPublicAdvertisementService
{
    public async Task<OperationResult<List<PublicAdvertisementCardDto>>> GetActiveForHomeAsync(int pageSize = 4)
    {
        var take = pageSize < 1 ? 4 : Math.Min(pageSize, 4);
        var now = DateTime.UtcNow.ToLocalTime();

        var rows = await context.Advertisements.AsNoTracking()
            .Where(a =>
                a.IsActive &&
                a.StartAt <= now &&
                a.EndAt >= now)
            .OrderByDescending(a => a.CreatedAt)
            .Take(take)
            .Select(a => new
            {
                a.AdvertisementId,
                a.Title,
                a.ShortDescription,
                a.ImagePath,
                a.LinkUrl,
            })
            .ToListAsync();

        var productIds = rows
            .Select(row => TryParseProductId(row.LinkUrl))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        Dictionary<Guid, string?> productImages = productIds.Count == 0
            ? new Dictionary<Guid, string?>()
            : await context.ProductImages.AsNoTracking()
                .Where(image => productIds.Contains(image.ProductId))
                .GroupBy(image => image.ProductId)
                .Select(group => new
                {
                    ProductId = group.Key,
                    ImagePath = group
                        .OrderBy(image => image.SortOrder)
                        .Select(image => image.ImagePath)
                        .FirstOrDefault(),
                })
                .ToDictionaryAsync(item => item.ProductId, item => item.ImagePath);

        var items = rows.Select(row =>
        {
            var productId = TryParseProductId(row.LinkUrl);
            string? productImagePath = null;
            if (productId.HasValue
                && productImages.TryGetValue(productId.Value, out var imagePath)
                && !string.IsNullOrWhiteSpace(imagePath))
                productImagePath = imagePath;

            return new PublicAdvertisementCardDto
            {
                AdvertisementId = row.AdvertisementId,
                Title = row.Title,
                ShortDescription = row.ShortDescription,
                ImagePath = row.ImagePath,
                ProductImagePath = productImagePath,
            };
        }).ToList();

        return OperationResult<List<PublicAdvertisementCardDto>>.Success(items);
    }

    private static Guid? TryParseProductId(string? linkUrl)
    {
        if (string.IsNullOrWhiteSpace(linkUrl))
            return null;

        var match = ProductLinkRegex().Match(linkUrl.Trim());
        return match.Success && Guid.TryParse(match.Groups[1].Value, out var productId)
            ? productId
            : null;
    }

    [GeneratedRegex(@"/products/([0-9a-fA-F-]{36})", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ProductLinkRegex();
}
