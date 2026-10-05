using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.ViewModels.Product;

namespace LabConnectPortal.Api.Infrastructure.Helpers;

public static class ProductListingImageHelper
{
    public static string? ResolvePrimaryImagePath(Product product)
    {
        if (!string.IsNullOrWhiteSpace(product.FeaturedImagePath))
            return product.FeaturedImagePath;

        return product.Images?.OrderBy(i => i.SortOrder).FirstOrDefault()?.ImagePath;
    }

    public static List<ProductImageDto> ResolveDisplayImages(Product product)
    {
        var images = (product.Images ?? [])
            .OrderBy(i => i.SortOrder)
            .Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImagePath = i.ImagePath,
                SortOrder = i.SortOrder,
            }).ToList();

        if (!string.IsNullOrWhiteSpace(product.FeaturedImagePath)
            && images.All(i => i.ImagePath != product.FeaturedImagePath))
        {
            images.Insert(0, new ProductImageDto
            {
                Id = Guid.Empty,
                ImagePath = product.FeaturedImagePath!,
                SortOrder = -1,
            });
        }

        return images;
    }
}
