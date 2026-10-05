using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Product;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class ProductOrderService(
    LabConnectDbContext context,
    ICenterProfileRepository centerProfileRepository,
    INotificationService notificationService) : IProductOrderService
{
    public async Task<OperationResult<ProductOrderDto>> CheckoutAsync(Guid userId, CheckoutCommand command)
    {
        var cart = await context.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product!).ThenInclude(p => p.CreatedByCenterProfile)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || cart.Items.Count == 0)
            return OperationResult<ProductOrderDto>.Failure("سبد خرید خالی است");

        var productItems = cart.Items.Where(i => i.ProductId.HasValue).ToList();
        if (productItems.Count == 0)
            return OperationResult<ProductOrderDto>.Failure("سبد خرید محصول خالی است");

        var centerId = productItems.First().Product!.CreatedByCenterProfileId;
        if (!centerId.HasValue || productItems.Any(i => i.Product!.CreatedByCenterProfileId != centerId))
            return OperationResult<ProductOrderDto>.Failure("سبد خرید باید فقط شامل محصولات یک مرکز باشد");

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            decimal total = 0;
            var orderItems = new List<ProductOrderItem>();

            foreach (var cartItem in productItems)
            {
                var product = await context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == cartItem.ProductId);

                if (product == null || product.Status != ProductStatus.Approved)
                {
                    await transaction.RollbackAsync();
                    return OperationResult<ProductOrderDto>.Failure("یک یا چند محصول دیگر در دسترس نیست");
                }

                if (product.StockQuantity < cartItem.Quantity)
                {
                    await transaction.RollbackAsync();
                    return OperationResult<ProductOrderDto>.Failure($"موجودی «{product.Title}» کافی نیست");
                }

                product.StockQuantity -= cartItem.Quantity;
                product.UpdatedAt = DateTime.UtcNow.ToLocalTime();

                var finalPrice = CartService.CalculateFinalPrice(product.Price, product.DiscountPercent);
                var lineTotal = finalPrice * cartItem.Quantity;
                total += lineTotal;

                orderItems.Add(new ProductOrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = product.Price,
                    DiscountPercent = product.DiscountPercent,
                    LineTotal = lineTotal,
                });
            }

            var shipping = await ResolveShippingSnapshotAsync(userId, command);
            if (shipping.Error != null)
            {
                await transaction.RollbackAsync();
                return OperationResult<ProductOrderDto>.Failure(shipping.Error);
            }

            var shippingCost = await ResolveShippingCostAsync(centerId.Value, command.ShippingMethod);
            total += shippingCost;

            var order = new ProductOrder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CenterProfileId = centerId.Value,
                Status = ProductOrderStatus.PendingPayment,
                TotalAmount = total,
                ShippingCost = shippingCost,
                ShippingMethod = command.ShippingMethod,
                StockReserved = true,
                ShippingRecipientName = shipping.RecipientName!,
                ShippingAddress = shipping.Address!,
                ShippingPhone = shipping.Phone!,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            foreach (var item in orderItems)
                item.OrderId = order.Id;

            context.ProductOrders.Add(order);
            context.ProductOrderItems.AddRange(orderItems);
            context.CartItems.RemoveRange(productItems);
            cart.UpdatedAt = DateTime.UtcNow.ToLocalTime();

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetMyOrderByIdAsync(userId, order.Id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OperationResult<CheckoutResultDto>> CheckoutCartAsync(Guid userId, CheckoutCommand command)
    {
        var cart = await context.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product!).ThenInclude(p => p.CreatedByCenterProfile)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || cart.Items.Count == 0)
            return OperationResult<CheckoutResultDto>.Failure("سبد خرید خالی است");

        var productItems = cart.Items.Where(i => i.ProductId.HasValue).ToList();
        if (productItems.Count == 0)
            return OperationResult<CheckoutResultDto>.Failure("سبد خرید محصول خالی است");

        if (productItems.Count != cart.Items.Count)
            return OperationResult<CheckoutResultDto>.Failure("آیتم نامعتبر در سبد خرید وجود دارد");

        var centerId = productItems[0].Product!.CreatedByCenterProfileId;
        if (!centerId.HasValue || productItems.Any(i => i.Product!.CreatedByCenterProfileId != centerId))
            return OperationResult<CheckoutResultDto>.Failure("سبد خرید باید فقط شامل کالاهای یک مرکز باشد");

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            decimal total = 0;
            var orderItems = new List<ProductOrderItem>();

            foreach (var cartItem in productItems)
            {
                var product = await context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == cartItem.ProductId);

                if (product == null || product.Status != ProductStatus.Approved)
                {
                    await transaction.RollbackAsync();
                    return OperationResult<CheckoutResultDto>.Failure("یک یا چند محصول دیگر در دسترس نیست");
                }

                if (product.StockQuantity < cartItem.Quantity)
                {
                    await transaction.RollbackAsync();
                    return OperationResult<CheckoutResultDto>.Failure($"موجودی «{product.Title}» کافی نیست");
                }

                product.StockQuantity -= cartItem.Quantity;
                product.UpdatedAt = DateTime.UtcNow.ToLocalTime();

                var finalPrice = CartService.CalculateFinalPrice(product.Price, product.DiscountPercent);
                var lineTotal = finalPrice * cartItem.Quantity;
                total += lineTotal;

                orderItems.Add(new ProductOrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = product.Price,
                    DiscountPercent = product.DiscountPercent,
                    LineTotal = lineTotal,
                });
            }

            var shipping = await ResolveShippingSnapshotAsync(userId, command);
            if (shipping.Error != null)
            {
                await transaction.RollbackAsync();
                return OperationResult<CheckoutResultDto>.Failure(shipping.Error);
            }

            var shippingCost = await ResolveShippingCostAsync(centerId.Value, command.ShippingMethod);
            total += shippingCost;

            var order = new ProductOrder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CenterProfileId = centerId.Value,
                Status = ProductOrderStatus.PendingPayment,
                TotalAmount = total,
                ShippingCost = shippingCost,
                ShippingMethod = command.ShippingMethod,
                StockReserved = true,
                ShippingRecipientName = shipping.RecipientName!,
                ShippingAddress = shipping.Address!,
                ShippingPhone = shipping.Phone!,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            foreach (var item in orderItems)
                item.OrderId = order.Id;

            context.ProductOrders.Add(order);
            context.ProductOrderItems.AddRange(orderItems);
            context.CartItems.RemoveRange(cart.Items);
            cart.UpdatedAt = DateTime.UtcNow.ToLocalTime();

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            var orderResult = await GetMyOrderByIdAsync(userId, order.Id);
            return OperationResult<CheckoutResultDto>.Success(new CheckoutResultDto
            {
                Order = orderResult.Data,
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OperationResult<ProductOrderDto>> ConfirmPaymentAsync(Guid userId, Guid orderId)
    {
        var order = await context.ProductOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
            return OperationResult<ProductOrderDto>.Failure("سفارش یافت نشد");

        if (order.Status != ProductOrderStatus.PendingPayment)
            return OperationResult<ProductOrderDto>.Failure("وضعیت سفارش برای پرداخت معتبر نیست");

        return await TransitionToPaidAsync(order);
    }

    public async Task<OperationResult<PagedResult<ProductOrderListItemDto>>> GetMyOrdersAsync(Guid userId, GetMyOrdersQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 100);

        var dbQuery = context.ProductOrders.AsNoTracking()
            .Include(o => o.CenterProfile)
            .Include(o => o.Items)
            .Where(o => o.UserId == userId);

        if (query.Status.HasValue)
            dbQuery = dbQuery.Where(o => o.Status == query.Status.Value);

        var totalCount = await dbQuery.CountAsync();
        var orders = await dbQuery
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return OperationResult<PagedResult<ProductOrderListItemDto>>.Success(new PagedResult<ProductOrderListItemDto>
        {
            Items = orders.Select(MapListItem).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    public async Task<OperationResult<ProductOrderDto>> GetMyOrderByIdAsync(Guid userId, Guid orderId)
    {
        var order = await LoadOrderAsync(orderId, userId, isCenterView: false);
        if (order == null)
            return OperationResult<ProductOrderDto>.Failure("سفارش یافت نشد");

        return OperationResult<ProductOrderDto>.Success(await MapOrderAsync(order));
    }

    public async Task<OperationResult> CancelOrderAsync(Guid userId, Guid orderId)
    {
        var order = await context.ProductOrders
            .Include(o => o.Items)
            .Include(o => o.CenterProfile)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
            return OperationResult.Failure("سفارش یافت نشد");

        if (order.Status != ProductOrderStatus.PendingPayment)
            return OperationResult.Failure("فقط سفارش در انتظار پرداخت قابل لغو است");

        if (order.StockReserved)
        {
            await RestoreStockAsync(order);
            order.StockReserved = false;
        }

        order.Status = ProductOrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow.ToLocalTime();
        order.UpdatedAt = DateTime.UtcNow.ToLocalTime();
        var save = await SaveChangesAsync();
        if (!save.Success)
            return save;

        if (order.CenterProfile.OwnerUserId.HasValue)
        {
            await notificationService.CreateAsync(
                order.CenterProfile.OwnerUserId.Value,
                "لغو سفارش",
                "خریدار سفارش در انتظار پرداخت را لغو کرد.",
                NotificationType.General);
        }

        return save;
    }

    public async Task<OperationResult<PagedResult<ProductOrderListItemDto>>> GetCenterOrdersAsync(Guid userId, GetCenterOrdersQuery query)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<PagedResult<ProductOrderListItemDto>>.Failure(access.Error);

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 100);

        var dbQuery = context.ProductOrders.AsNoTracking()
            .Include(o => o.CenterProfile)
            .Include(o => o.User)
            .Include(o => o.Items)
            .Where(o => o.CenterProfileId == access.Profile!.Id);

        if (query.Status.HasValue)
            dbQuery = dbQuery.Where(o => o.Status == query.Status.Value);

        var totalCount = await dbQuery.CountAsync();
        var orders = await dbQuery
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var buyerContacts = await ResolveBuyerContactsAsync(orders.Select(o => o.User).ToList());

        return OperationResult<PagedResult<ProductOrderListItemDto>>.Success(new PagedResult<ProductOrderListItemDto>
        {
            Items = orders.Select(o => MapCenterOrderListItem(o, buyerContacts)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    public async Task<OperationResult<ProductOrderDto>> GetCenterOrderByIdAsync(Guid userId, Guid orderId)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<ProductOrderDto>.Failure(access.Error);

        var order = await LoadOrderAsync(orderId, access.Profile!.Id, isCenterView: true);
        if (order == null)
            return OperationResult<ProductOrderDto>.Failure("سفارش یافت نشد");

        return OperationResult<ProductOrderDto>.Success(await MapOrderAsync(order));
    }

    public async Task<OperationResult<ProductOrderDto>> MarkPaidAsync(Guid userId, Guid orderId)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<ProductOrderDto>.Failure(access.Error);

        var order = await context.ProductOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CenterProfileId == access.Profile!.Id);

        if (order == null)
            return OperationResult<ProductOrderDto>.Failure("سفارش یافت نشد");

        if (order.Status != ProductOrderStatus.PendingPayment)
            return OperationResult<ProductOrderDto>.Failure("وضعیت سفارش برای پرداخت معتبر نیست");

        var result = await TransitionToPaidAsync(order);
        if (!result.Status)
            return result;

        return await GetCenterOrderByIdAsync(userId, orderId);
    }

    public async Task<OperationResult<ProductOrderDto>> CompleteAsync(Guid userId, Guid orderId)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<ProductOrderDto>.Failure(access.Error);

        var order = await context.ProductOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CenterProfileId == access.Profile!.Id);

        if (order == null)
            return OperationResult<ProductOrderDto>.Failure("سفارش یافت نشد");

        if (order.Status != ProductOrderStatus.Paid)
            return OperationResult<ProductOrderDto>.Failure("فقط سفارش پرداخت‌شده قابل تکمیل است");

        order.Status = ProductOrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow.ToLocalTime();
        order.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductOrderDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        await notificationService.CreateAsync(
            order.UserId,
            "تکمیل سفارش",
            "سفارش شما توسط فروشنده تکمیل شد و آماده ارسال است.",
            NotificationType.General);

        return await GetCenterOrderByIdAsync(userId, orderId);
    }

    public async Task<OperationResult<ProductOrderDto>> CancelCenterOrderAsync(Guid userId, Guid orderId)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<ProductOrderDto>.Failure(access.Error);

        var order = await context.ProductOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CenterProfileId == access.Profile!.Id);

        if (order == null)
            return OperationResult<ProductOrderDto>.Failure("سفارش یافت نشد");

        if (order.Status is ProductOrderStatus.Completed
            or ProductOrderStatus.Cancelled
            or ProductOrderStatus.Shipped
            or ProductOrderStatus.Delivered)
            return OperationResult<ProductOrderDto>.Failure("این سفارش قابل لغو نیست");

        if (order.Status == ProductOrderStatus.Paid || order.StockReserved)
        {
            await RestoreStockAsync(order);
            order.StockReserved = false;
        }

        var buyerUserId = order.UserId;
        order.Status = ProductOrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow.ToLocalTime();
        order.UpdatedAt = DateTime.UtcNow.ToLocalTime();

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductOrderDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        await notificationService.CreateAsync(
            buyerUserId,
            "لغو سفارش",
            "سفارش شما توسط فروشنده لغو شد.",
            NotificationType.General);

        return await GetCenterOrderByIdAsync(userId, orderId);
    }

    public async Task<OperationResult<PagedResult<ProductOrderShipmentItemDto>>> GetShipmentItemsAsync(
        Guid userId,
        GetShipmentItemsQuery query)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<PagedResult<ProductOrderShipmentItemDto>>.Failure(access.Error);

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 500);

        var dbQuery = context.ProductOrderItems.AsNoTracking()
            .Include(i => i.Product)
            .Include(i => i.Order).ThenInclude(o => o.User)
            .Where(i => i.Order.CenterProfileId == access.Profile!.Id
                && (i.Order.Status == ProductOrderStatus.Paid
                    || i.Order.Status == ProductOrderStatus.Completed
                    || i.Order.Status == ProductOrderStatus.Shipped
                    || i.Order.Status == ProductOrderStatus.Delivered)
                && i.ProductId != null);

        var shipmentStatus = (query.ShipmentStatus ?? "pending").Trim().ToLowerInvariant();
        if (shipmentStatus is "pending" or "awaiting")
            dbQuery = dbQuery.Where(i => i.ShippedAt == null);
        else if (shipmentStatus is "shipped" or "sent")
            dbQuery = dbQuery.Where(i => i.ShippedAt != null);
        // "all" (or any other value): no extra shipment filter

        var search = query.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            dbQuery = dbQuery.Where(i =>
                (i.Product != null && i.Product.Title.Contains(search))
                || i.Order.User.Username.Contains(search)
                || i.Order.User.MobileNumber.Contains(search)
                || context.CenterProfiles.Any(p =>
                    p.Id == i.Order.User.CenterProfileId && p.Name.Contains(search)));
        }

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .OrderByDescending(i => i.Order.PaidAt ?? i.Order.CreatedAt)
            .ThenBy(i => i.ShippedAt == null ? 0 : 1)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var profileIds = items
            .Select(i => i.Order.User.CenterProfileId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var centerProfiles = await context.CenterProfiles.AsNoTracking()
            .Where(p => profileIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var mapped = items.Select(i => MapShipmentItem(i, centerProfiles)).ToList();

        return OperationResult<PagedResult<ProductOrderShipmentItemDto>>.Success(
            new PagedResult<ProductOrderShipmentItemDto>
            {
                Items = mapped,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            });
    }

    public async Task<OperationResult<ProductOrderShipmentItemDto>> MarkOrderItemShippedAsync(Guid userId, Guid orderItemId)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<ProductOrderShipmentItemDto>.Failure(access.Error);

        var item = await context.ProductOrderItems
            .Include(i => i.Product)
            .Include(i => i.Order).ThenInclude(o => o.User)
            .FirstOrDefaultAsync(i => i.Id == orderItemId
                && i.Order.CenterProfileId == access.Profile!.Id
                && i.ProductId != null);

        if (item == null)
            return OperationResult<ProductOrderShipmentItemDto>.Failure("قلم سفارش یافت نشد");

        if (item.Order.Status != ProductOrderStatus.Completed)
            return OperationResult<ProductOrderShipmentItemDto>.Failure("فقط اقلام سفارش تکمیل‌شده قابل ارسال هستند");

        if (item.ShippedAt.HasValue)
            return OperationResult<ProductOrderShipmentItemDto>.Failure("این قلم قبلاً ارسال شده است");

        var productTitle = item.Product?.Title ?? "محصول";
        var buyerUserId = item.Order.UserId;
        var now = DateTime.UtcNow.ToLocalTime();

        item.ShippedAt = now;
        item.Order.UpdatedAt = now;

        var remainingUnshipped = await context.ProductOrderItems
            .AnyAsync(i => i.OrderId == item.OrderId
                && i.ProductId != null
                && i.Id != item.Id
                && i.ShippedAt == null);
        if (!remainingUnshipped)
        {
            item.Order.Status = ProductOrderStatus.Shipped;
            item.Order.ShippedAt ??= now;
        }

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductOrderShipmentItemDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        await notificationService.CreateAsync(
            buyerUserId,
            "ارسال محصول",
            $"محصول «{productTitle}» برای شما ارسال شد.",
            NotificationType.General);

        CenterProfile? profile = null;
        if (item.Order.User.CenterProfileId.HasValue)
            profile = await context.CenterProfiles.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == item.Order.User.CenterProfileId);

        return OperationResult<ProductOrderShipmentItemDto>.Success(MapShipmentItem(
            item,
            profile != null && item.Order.User.CenterProfileId.HasValue
                ? new Dictionary<Guid, CenterProfile> { [item.Order.User.CenterProfileId.Value] = profile }
                : new Dictionary<Guid, CenterProfile>()));
    }

    public async Task<OperationResult<ProductOrderDto>> MarkOrderShippedAsync(Guid userId, ShipOrderCommand command)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<ProductOrderDto>.Failure(access.Error);

        var order = await context.ProductOrders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId && o.CenterProfileId == access.Profile!.Id);

        if (order == null)
            return OperationResult<ProductOrderDto>.Failure("سفارش یافت نشد");

        if (order.Status != ProductOrderStatus.Completed)
            return OperationResult<ProductOrderDto>.Failure("فقط سفارش تکمیل‌شده قابل ارسال است");

        var pendingItems = order.Items
            .Where(i => !i.ShippedAt.HasValue)
            .ToList();

        var tracking = command.TrackingCode?.Trim();
        var company = command.ShippingCompany?.Trim();
        if (!string.IsNullOrEmpty(tracking) && tracking.Length > 100)
            return OperationResult<ProductOrderDto>.Failure("کد رهگیری بیش از حد طولانی است");
        if (!string.IsNullOrEmpty(company) && company.Length > 200)
            return OperationResult<ProductOrderDto>.Failure("نام شرکت حمل بیش از حد طولانی است");

        var now = DateTime.UtcNow.ToLocalTime();
        foreach (var item in pendingItems)
            item.ShippedAt = now;

        order.TrackingCode = string.IsNullOrEmpty(tracking) ? null : tracking;
        order.ShippingCompany = string.IsNullOrEmpty(company) ? null : company;
        order.Status = ProductOrderStatus.Shipped;
        order.ShippedAt = now;
        order.UpdatedAt = now;

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductOrderDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var titles = order.Items
            .Select(i => i.Product?.Title)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .Take(3)
            .ToList();
        var titleText = titles.Count > 0 ? string.Join("، ", titles) : "اقلام سفارش";
        if (order.Items.Count > titles.Count)
            titleText += " و سایر اقلام";

        var trackingText = !string.IsNullOrEmpty(order.TrackingCode)
            ? $" کد رهگیری: {order.TrackingCode}."
            : string.Empty;

        await notificationService.CreateAsync(
            order.UserId,
            "ارسال سفارش",
            $"سفارش شما ارسال شد ({titleText}).{trackingText}",
            NotificationType.General);

        return await GetCenterOrderByIdAsync(userId, command.OrderId);
    }

    public async Task<OperationResult<ProductOrderDto>> MarkOrderDeliveredAsync(Guid userId, DeliverOrderCommand command)
    {
        var access = await ValidateManagerAccessAsync(userId);
        if (access.Error != null)
            return OperationResult<ProductOrderDto>.Failure(access.Error);

        var order = await context.ProductOrders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId && o.CenterProfileId == access.Profile!.Id);

        if (order == null)
            return OperationResult<ProductOrderDto>.Failure("سفارش یافت نشد");

        if (order.Status != ProductOrderStatus.Shipped)
            return OperationResult<ProductOrderDto>.Failure("فقط سفارش ارسال‌شده قابل ثبت تحویل است");

        var notes = command.Notes?.Trim();
        if (!string.IsNullOrEmpty(notes) && notes.Length > 1000)
            return OperationResult<ProductOrderDto>.Failure("توضیحات تحویل بیش از حد طولانی است");

        var now = DateTime.UtcNow.ToLocalTime();
        order.DeliveryNotes = string.IsNullOrEmpty(notes) ? null : notes;
        order.Status = ProductOrderStatus.Delivered;
        order.DeliveredAt = now;
        order.UpdatedAt = now;

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductOrderDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        var notesText = !string.IsNullOrEmpty(order.DeliveryNotes)
            ? $" توضیح: {order.DeliveryNotes}"
            : string.Empty;

        await notificationService.CreateAsync(
            order.UserId,
            "تحویل سفارش",
            $"سفارش شما تحویل داده شد.{notesText}",
            NotificationType.General);

        return await GetCenterOrderByIdAsync(userId, command.OrderId);
    }

    public async Task<OperationResult<ProductOrderDto>> ConfirmDeliveryAsync(Guid userId, Guid orderId)
    {
        var order = await context.ProductOrders
            .Include(o => o.CenterProfile)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
            return OperationResult<ProductOrderDto>.Failure("سفارش یافت نشد");

        if (order.Status != ProductOrderStatus.Shipped)
            return OperationResult<ProductOrderDto>.Failure("فقط سفارش ارسال‌شده قابل تأیید دریافت است");

        var now = DateTime.UtcNow.ToLocalTime();
        order.Status = ProductOrderStatus.Delivered;
        order.DeliveredAt = now;
        order.UpdatedAt = now;

        var save = await SaveChangesAsync();
        if (!save.Success)
            return OperationResult<ProductOrderDto>.Failure(save.Message ?? "خطا در ذخیره‌سازی");

        if (order.CenterProfile.OwnerUserId.HasValue)
        {
            await notificationService.CreateAsync(
                order.CenterProfile.OwnerUserId.Value,
                "دریافت سفارش",
                "خریدار دریافت سفارش را تأیید کرد.",
                NotificationType.General);
        }

        return await GetMyOrderByIdAsync(userId, orderId);
    }

    private static ProductOrderShipmentItemDto MapShipmentItem(
        ProductOrderItem i,
        IReadOnlyDictionary<Guid, CenterProfile> centerProfiles)
    {
        var buyer = i.Order.User;
        CenterProfile? profile = null;
        if (buyer.CenterProfileId.HasValue)
            centerProfiles.TryGetValue(buyer.CenterProfileId.Value, out profile);

        var fallbackName = !string.IsNullOrWhiteSpace(buyer.Username)
            ? buyer.Username
            : buyer.MobileNumber;

        return new ProductOrderShipmentItemDto
        {
            Id = i.Id,
            ProductTitle = i.Product?.Title ?? string.Empty,
            BuyerCenterName = FirstNonEmpty(i.Order.ShippingRecipientName, profile?.Name, fallbackName),
            BuyerCenterAddress = FirstNonEmpty(i.Order.ShippingAddress, profile?.Address),
            BuyerPhone = FirstNonEmpty(i.Order.ShippingPhone, profile?.Phone, buyer.MobileNumber),
            Quantity = i.Quantity,
            PaidAt = i.Order.PaidAt,
            ShippedAt = i.ShippedAt,
            IsShipped = i.ShippedAt.HasValue,
        };
    }

    private async Task<OperationResult<ProductOrderDto>> TransitionToPaidAsync(ProductOrder order)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            if (!order.StockReserved)
            {
                var items = await context.ProductOrderItems
                    .Where(i => i.OrderId == order.Id)
                    .ToListAsync();

                foreach (var item in items)
                {
                    if (item.ProductId.HasValue)
                    {
                        var product = await context.Products
                            .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                        if (product == null || product.StockQuantity < item.Quantity)
                        {
                            await transaction.RollbackAsync();
                            return OperationResult<ProductOrderDto>.Failure("موجودی یک یا چند محصول کافی نیست");
                        }

                        product.StockQuantity -= item.Quantity;
                        product.UpdatedAt = DateTime.UtcNow.ToLocalTime();
                    }
                }

                order.StockReserved = true;
            }

            order.Status = ProductOrderStatus.Paid;
            order.PaidAt = DateTime.UtcNow.ToLocalTime();
            order.UpdatedAt = DateTime.UtcNow.ToLocalTime();

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            var ownerUserId = await context.CenterProfiles.AsNoTracking()
                .Where(p => p.Id == order.CenterProfileId)
                .Select(p => p.OwnerUserId)
                .FirstOrDefaultAsync();
            if (ownerUserId.HasValue)
            {
                await notificationService.CreateAsync(
                    ownerUserId.Value,
                    "پرداخت سفارش",
                    "یک سفارش جدید پرداخت شد.",
                    NotificationType.General);
            }

            return OperationResult<ProductOrderDto>.Success(await MapOrderAsync(
                (await LoadOrderAsync(order.Id, order.UserId, isCenterView: false))!));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task RestoreStockAsync(ProductOrder order)
    {
        foreach (var item in order.Items)
        {
            if (item.ProductId.HasValue)
            {
                var product = await context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow.ToLocalTime();
                }
            }
        }
    }

    private async Task<ProductOrder?> LoadOrderAsync(Guid orderId, Guid scopeId, bool isCenterView)
    {
        var query = context.ProductOrders.AsNoTracking()
            .Include(o => o.CenterProfile)
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.UserReview)
            .AsQueryable();

        if (isCenterView)
            return await query.FirstOrDefaultAsync(o => o.Id == orderId && o.CenterProfileId == scopeId);

        return await query.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == scopeId);
    }

    private async Task<ProductOrderDto> MapOrderAsync(ProductOrder order)
        => new()
        {
            Id = order.Id,
            CenterProfileId = order.CenterProfileId,
            CenterName = order.CenterProfile.Name,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            ShippingCost = order.ShippingCost,
            ShippingMethod = order.ShippingMethod,
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt,
            CompletedAt = order.CompletedAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            ShippingRecipientName = order.ShippingRecipientName,
            ShippingAddress = order.ShippingAddress,
            ShippingPhone = order.ShippingPhone,
            TrackingCode = order.TrackingCode,
            ShippingCompany = order.ShippingCompany,
            DeliveryNotes = order.DeliveryNotes,
            BuyerName = order.User != null
                ? FirstNonEmpty(
                    order.ShippingRecipientName,
                    $"{order.User.FirstName} {order.User.LastName}".Trim(),
                    order.User.Username,
                    order.User.MobileNumber)
                : order.ShippingRecipientName,
            BuyerPhone = FirstNonEmpty(order.ShippingPhone, order.User?.Phone, order.User?.MobileNumber),
            CanShip = order.Status == ProductOrderStatus.Completed,
            CanDeliver = order.Status == ProductOrderStatus.Shipped,
            CanConfirmDelivery = order.Status == ProductOrderStatus.Shipped,
            Items = order.Items.Select(i => new ProductOrderItemDto
            {
                Id = i.Id,
                ItemType = CartItemType.Product,
                ProductId = i.ProductId,
                ProductTitle = i.Product!.Title,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountPercent = i.DiscountPercent,
                LineTotal = i.LineTotal,
                HasReview = i.UserReview != null,
                CanReview = (order.Status is ProductOrderStatus.Completed
                        or ProductOrderStatus.Shipped
                        or ProductOrderStatus.Delivered)
                    && i.UserReview == null,
                ShippedAt = i.ShippedAt,
                IsShipped = i.ShippedAt.HasValue,
            }).ToList(),
        };

    private static ProductOrderListItemDto MapListItem(ProductOrder order)
        => new()
        {
            Id = order.Id,
            CenterName = order.CenterProfile?.Name ?? string.Empty,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            ShippingCost = order.ShippingCost,
            ShippingMethod = order.ShippingMethod,
            CreatedAt = order.CreatedAt,
            ItemCount = order.Items.Count,
            CanConfirmDelivery = order.Status == ProductOrderStatus.Shipped,
        };

    private sealed record BuyerContact(string Name, string Address, string Phone);

    private async Task<Dictionary<Guid, BuyerContact>> ResolveBuyerContactsAsync(IReadOnlyList<User> buyers)
    {
        var result = new Dictionary<Guid, BuyerContact>();
        if (buyers.Count == 0)
            return result;

        var distinctBuyers = buyers
            .GroupBy(b => b.Id)
            .Select(g => g.First())
            .ToList();

        var buyerIds = distinctBuyers.Select(b => b.Id).ToList();
        var profileIds = distinctBuyers
            .Where(b => b.CenterProfileId.HasValue)
            .Select(b => b.CenterProfileId!.Value)
            .Distinct()
            .ToList();

        var profilesById = await context.CenterProfiles.AsNoTracking()
            .Where(p => profileIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var profilesByOwnerList = await context.CenterProfiles.AsNoTracking()
            .Where(p => p.OwnerUserId.HasValue && buyerIds.Contains(p.OwnerUserId.Value))
            .ToListAsync();
        var profilesByOwner = profilesByOwnerList
            .GroupBy(p => p.OwnerUserId!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        var locationsList = await context.OrganizationLocations.AsNoTracking()
            .Where(l => buyerIds.Contains(l.UserId))
            .ToListAsync();
        var locationsByUser = locationsList
            .GroupBy(l => l.UserId)
            .ToDictionary(g => g.Key, g => g.OrderBy(l => l.LocationName).First());

        foreach (var buyer in distinctBuyers)
        {
            CenterProfile? profile = null;
            if (buyer.CenterProfileId.HasValue)
                profilesById.TryGetValue(buyer.CenterProfileId.Value, out profile);
            if (profile == null)
                profilesByOwner.TryGetValue(buyer.Id, out profile);

            locationsByUser.TryGetValue(buyer.Id, out var location);

            var name = FirstNonEmpty(
                profile?.Name,
                $"{buyer.FirstName} {buyer.LastName}".Trim(),
                buyer.Username,
                buyer.MobileNumber);

            var address = FirstNonEmpty(
                profile?.Address,
                location?.Address,
                buyer.Address);

            var phone = FirstNonEmpty(
                profile?.Phone,
                location?.PhoneNumber,
                buyer.Phone,
                buyer.MobileNumber);

            result[buyer.Id] = new BuyerContact(name, address, phone);
        }

        return result;
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }
        return string.Empty;
    }

    private static ProductOrderListItemDto MapCenterOrderListItem(
        ProductOrder order,
        IReadOnlyDictionary<Guid, BuyerContact> buyerContacts)
    {
        buyerContacts.TryGetValue(order.UserId, out var contact);
        var fallbackName = !string.IsNullOrWhiteSpace(order.User.Username)
            ? order.User.Username
            : order.User.MobileNumber;

        return new ProductOrderListItemDto
        {
            Id = order.Id,
            CenterName = FirstNonEmpty(order.ShippingRecipientName, contact?.Name, fallbackName),
            BuyerAddress = FirstNonEmpty(order.ShippingAddress, contact?.Address),
            BuyerPhone = FirstNonEmpty(order.ShippingPhone, contact?.Phone, order.User.MobileNumber),
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            ShippingCost = order.ShippingCost,
            ShippingMethod = order.ShippingMethod,
            CreatedAt = order.CreatedAt,
            ItemCount = order.Items.Count,
            CanShip = order.Status == ProductOrderStatus.Completed,
            CanDeliver = order.Status == ProductOrderStatus.Shipped,
        };
    }

    private static Task<decimal> ResolveShippingCostAsync(Guid centerProfileId, ShippingMethod method)
        => Task.FromResult(0m);

    private async Task<(string? RecipientName, string? Address, string? Phone, string? Error)> ResolveShippingSnapshotAsync(
        Guid userId,
        CheckoutCommand command)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return (null, null, null, "کاربر یافت نشد");

        CenterProfile? profile = null;
        if (user.CenterProfileId.HasValue)
            profile = await context.CenterProfiles.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == user.CenterProfileId.Value);
        if (profile == null)
            profile = await context.CenterProfiles.AsNoTracking()
                .FirstOrDefaultAsync(p => p.OwnerUserId == userId);

        var location = await context.OrganizationLocations.AsNoTracking()
            .Where(l => l.UserId == userId)
            .OrderBy(l => l.LocationName)
            .FirstOrDefaultAsync();

        var recipient = FirstNonEmpty(
            command.ShippingRecipientName,
            profile?.Name,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Username,
            user.MobileNumber);

        var address = FirstNonEmpty(
            command.ShippingAddress,
            profile?.Address,
            location?.Address,
            user.Address);

        var phone = FirstNonEmpty(
            command.ShippingPhone,
            profile?.Phone,
            location?.PhoneNumber,
            user.Phone,
            user.MobileNumber);

        if (string.IsNullOrWhiteSpace(address))
            return (null, null, null, "لطفاً آدرس تحویل را تکمیل کنید");

        if (string.IsNullOrWhiteSpace(phone))
            return (null, null, null, "لطفاً شماره تماس تحویل را تکمیل کنید");

        if (string.IsNullOrWhiteSpace(recipient))
            recipient = phone;

        return (recipient, address, phone, null);
    }

    public async Task<OperationResult<PagedResult<AdminProductOrderListItemDto>>> GetAdminOrdersAsync(GetAdminOrdersQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 100);

        var dbQuery = context.ProductOrders.AsNoTracking()
            .Include(o => o.CenterProfile)
            .Include(o => o.User)
            .Include(o => o.Items)
            .AsQueryable();

        if (query.Status.HasValue)
            dbQuery = dbQuery.Where(o => o.Status == query.Status.Value);

        var search = query.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            dbQuery = dbQuery.Where(o =>
                o.CenterProfile.Name.Contains(search)
                || o.ShippingRecipientName.Contains(search)
                || o.ShippingPhone.Contains(search)
                || o.User.Username.Contains(search)
                || o.User.MobileNumber.Contains(search)
                || (o.TrackingCode != null && o.TrackingCode.Contains(search)));
        }

        var totalCount = await dbQuery.CountAsync();
        var orders = await dbQuery
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return OperationResult<PagedResult<AdminProductOrderListItemDto>>.Success(
            new PagedResult<AdminProductOrderListItemDto>
            {
                Items = orders.Select(o => new AdminProductOrderListItemDto
                {
                    Id = o.Id,
                    CenterName = o.CenterProfile.Name,
                    BuyerName = FirstNonEmpty(
                        o.ShippingRecipientName,
                        $"{o.User.FirstName} {o.User.LastName}".Trim(),
                        o.User.Username,
                        o.User.MobileNumber),
                    BuyerPhone = FirstNonEmpty(o.ShippingPhone, o.User.Phone, o.User.MobileNumber),
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    ShippingCost = o.ShippingCost,
                    ShippingMethod = o.ShippingMethod,
                    TrackingCode = o.TrackingCode,
                    ShippingCompany = o.ShippingCompany,
                    CreatedAt = o.CreatedAt,
                    ItemCount = o.Items.Count,
                }).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            });
    }

    public async Task<OperationResult<ProductOrderDto>> GetAdminOrderByIdAsync(Guid orderId)
    {
        var order = await context.ProductOrders.AsNoTracking()
            .Include(o => o.CenterProfile)
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.UserReview)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return OperationResult<ProductOrderDto>.Failure("سفارش یافت نشد");

        return OperationResult<ProductOrderDto>.Success(await MapOrderAsync(order));
    }

    private async Task<(User? User, CenterProfile? Profile, string? Error)> ValidateManagerAccessAsync(Guid userId)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return (null, null, "کاربر یافت نشد");

        if (user.UserType is not (UserType.Store or UserType.AdminLab or UserType.UserLab))
            return (user, null, "دسترسی مجاز نیست");

        var profile = await ResolveCenterProfileAsync(user);
        if (profile == null)
            return (user, null, "پروفایل مرکز یافت نشد");

        return (user, profile, null);
    }

    private async Task<CenterProfile?> ResolveCenterProfileAsync(User user)
    {
        if (user.CenterProfileId.HasValue)
            return await centerProfileRepository.GetByIdAsync(user.CenterProfileId.Value);

        if (user.UserType == UserType.AdminLab && user.GetLabCode().HasValue)
            return await centerProfileRepository.GetByLabCodeAsync(user.GetLabCode()!.Value);
        if (user.UserType == UserType.Store)
            return await centerProfileRepository.GetByOwnerUserIdAsync(user.Id);
        return null;
    }

    private async Task<OperationResult> SaveChangesAsync()
    {
        try
        {
            await context.SaveChangesAsync();
            return OperationResult.SuccessResult();
        }
        catch (DbUpdateException ex)
        {
            return OperationResult.Failure(ex.InnerException?.Message ?? ex.Message);
        }
    }
}
