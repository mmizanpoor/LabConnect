using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.Utils;

public static class AdvertisementOrderStatusHelper
{
    public static string ToPersianTitle(AdvertisementOrderStatus status) => status switch
    {
        AdvertisementOrderStatus.Draft => "پیش‌نویس",
        AdvertisementOrderStatus.PendingPayment => "در انتظار پرداخت",
        AdvertisementOrderStatus.Paid => "پرداخت‌شده",
        AdvertisementOrderStatus.Active => "فعال",
        AdvertisementOrderStatus.Expired => "منقضی",
        AdvertisementOrderStatus.Cancelled => "لغو‌شده",
        _ => status.ToString(),
    };

    public static bool OccupiesSlot(AdvertisementOrderStatus status)
        => status is AdvertisementOrderStatus.PendingPayment
            or AdvertisementOrderStatus.Paid
            or AdvertisementOrderStatus.Active;
}
