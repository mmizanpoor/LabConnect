using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Helpers;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Product;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class CartService(
    LabConnectDbContext context) : ICartService
{
    public async Task<OperationResult<CartDto>> GetCartAsync(Guid userId)
    {
        var cart = await GetOrCreateCartAsync(userId, tracking: false);
        return OperationResult<CartDto>.Success(await MapCartAsync(cart.Id));
    }

    public async Task<OperationResult<CartDto>> AddToCartAsync(Guid userId, AddToCartCommand command)
    {
        if (command.Quantity < 1)
            return OperationResult<CartDto>.Failure("تعداد نامعتبر است");

        if (!command.ProductId.HasValue)
            return OperationResult<CartDto>.Failure("محصول نامعتبر است");

        return await AddProductToCartAsync(userId, command.ProductId.Value, command.Quantity);
    }

    private async Task<OperationResult<CartDto>> AddProductToCartAsync(Guid userId, Guid productId, int quantity)
    {
        var product = await context.Products.AsNoTracking()
            .Include(p => p.CreatedByCenterProfile)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p =>
                p.ProductId == productId
                && p.Status == ProductStatus.Approved
                && p.StockQuantity > 0);

        if (product == null)
            return OperationResult<CartDto>.Failure("محصول یافت نشد یا منتشر نشده است");

        if (!product.CreatedByCenterProfileId.HasValue)
            return OperationResult<CartDto>.Failure("فروشنده محصول مشخص نیست");

        if (product.IsNegotiablePrice)
            return OperationResult<CartDto>.Failure("این محصول قابل افزودن به سبد نیست");

        if (product.StockQuantity < quantity)
            return OperationResult<CartDto>.Failure("موجودی کافی نیست");

        var cart = await GetOrCreateCartAsync(userId, tracking: true);
        var existingItems = await LoadCartItemsAsync(cart.Id);

        var centerError = ValidateSameCenter(existingItems, product.CreatedByCenterProfileId.Value);
        if (centerError != null)
            return OperationResult<CartDto>.Failure(centerError);

        var existing = existingItems.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
        {
            var newQty = existing.Quantity + quantity;
            if (product.StockQuantity < newQty)
                return OperationResult<CartDto>.Failure("موجودی کافی نیست");
            existing.Quantity = newQty;
        }
        else
        {
            context.CartItems.Add(new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity,
            });
        }

        cart.UpdatedAt = DateTime.UtcNow.ToLocalTime();
        await context.SaveChangesAsync();
        return OperationResult<CartDto>.Success(await MapCartAsync(cart.Id));
    }

    public async Task<OperationResult<CartDto>> UpdateItemAsync(Guid userId, UpdateCartItemCommand command)
    {
        if (command.Quantity < 1)
            return OperationResult<CartDto>.Failure("تعداد نامعتبر است");

        var cart = await context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
            return OperationResult<CartDto>.Failure("سبد خرید یافت نشد");

        var item = await context.CartItems
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == command.CartItemId && i.CartId == cart.Id);

        if (item == null)
            return OperationResult<CartDto>.Failure("آیتم یافت نشد");

        if (item.Product != null)
        {
            if (item.Product.Status != ProductStatus.Approved || item.Product.StockQuantity <= 0)
                return OperationResult<CartDto>.Failure("این محصول دیگر در دسترس نیست");
            if (item.Product.StockQuantity < command.Quantity)
                return OperationResult<CartDto>.Failure("موجودی کافی نیست");
        }

        item.Quantity = command.Quantity;
        cart.UpdatedAt = DateTime.UtcNow.ToLocalTime();
        await context.SaveChangesAsync();
        return OperationResult<CartDto>.Success(await MapCartAsync(cart.Id));
    }

    public async Task<OperationResult<CartDto>> RemoveItemAsync(Guid userId, Guid cartItemId)
    {
        var cart = await context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
            return OperationResult<CartDto>.Failure("سبد خرید یافت نشد");

        var item = await context.CartItems.FirstOrDefaultAsync(i => i.Id == cartItemId && i.CartId == cart.Id);
        if (item == null)
            return OperationResult<CartDto>.Failure("آیتم یافت نشد");

        context.CartItems.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow.ToLocalTime();
        await context.SaveChangesAsync();
        return OperationResult<CartDto>.Success(await MapCartAsync(cart.Id));
    }

    public async Task<OperationResult<CartDto>> ClearCartAsync(Guid userId)
    {
        var cart = await context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
            return OperationResult<CartDto>.Success(new CartDto());

        context.CartItems.RemoveRange(cart.Items);
        cart.UpdatedAt = DateTime.UtcNow.ToLocalTime();
        await context.SaveChangesAsync();
        return OperationResult<CartDto>.Success(new CartDto());
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId, bool tracking)
    {
        var query = tracking ? context.Carts : context.Carts.AsNoTracking();
        var cart = await query.FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart != null)
            return cart;

        cart = new Cart { Id = Guid.NewGuid(), UserId = userId, UpdatedAt = DateTime.UtcNow };
        context.Carts.Add(cart);
        await context.SaveChangesAsync();
        return cart;
    }

    private async Task<List<CartItem>> LoadCartItemsAsync(Guid cartId)
        => await context.CartItems
            .Include(i => i.Product).ThenInclude(p => p!.CreatedByCenterProfile)
            .Where(i => i.CartId == cartId)
            .ToListAsync();

    private static string? ValidateSameCenter(List<CartItem> existingItems, Guid centerProfileId)
    {
        if (existingItems.Count == 0)
            return null;

        var currentCenterId = existingItems[0].Product?.CreatedByCenterProfileId;

        if (!currentCenterId.HasValue)
            return null;

        if (currentCenterId.Value != centerProfileId)
            return "سبد خرید فقط می‌تواند شامل کالاهای یک مرکز باشد. ابتدا سبد را خالی کنید.";

        return null;
    }

    private async Task<CartDto> MapCartAsync(Guid cartId)
    {
        var items = await context.CartItems.AsNoTracking()
            .Include(i => i.Product).ThenInclude(p => p!.Images)
            .Include(i => i.Product).ThenInclude(p => p!.CreatedByCenterProfile)
            .Where(i => i.CartId == cartId)
            .ToListAsync();

        if (items.Count == 0)
            return new CartDto();

        var mapped = items
            .Select(MapCartItem)
            .Where(i => i != null)
            .Cast<CartItemDto>()
            .ToList();

        if (mapped.Count == 0)
            return new CartDto();

        var dto = new CartDto
        {
            CenterProfileId = mapped[0].CenterProfileId,
            CenterName = mapped[0].CenterName,
            Items = mapped,
        };

        dto.DefaultShippingCost = 0;
        dto.TotalAmount = dto.Items.Sum(i => i.LineTotal);
        return dto;
    }

    internal static CartItemDto? MapCartItem(CartItem i)
    {
        if (i.Product == null || !i.ProductId.HasValue || !i.Product.CreatedByCenterProfileId.HasValue)
            return null;

        var finalPrice = CalculateFinalPrice(i.Product.Price, i.Product.DiscountPercent);
        return new CartItemDto
        {
            Id = i.Id,
            ItemType = CartItemType.Product,
            ProductId = i.ProductId.Value,
            ProductTitle = i.Product.Title,
            CenterName = i.Product.CreatedByCenterProfile?.Name ?? string.Empty,
            CenterProfileId = i.Product.CreatedByCenterProfileId.Value,
            Quantity = i.Quantity,
            UnitPrice = i.Product.Price,
            DiscountPercent = i.Product.DiscountPercent,
            LineTotal = finalPrice * i.Quantity,
            ImagePath = ProductListingImageHelper.ResolvePrimaryImagePath(i.Product),
            StockQuantity = i.Product.StockQuantity,
        };
    }

    internal static decimal CalculateFinalPrice(decimal price, decimal? discountPercent)
    {
        if (!discountPercent.HasValue || discountPercent.Value <= 0)
            return price;
        return Math.Round(price * (1 - discountPercent.Value / 100m), 0);
    }
}
