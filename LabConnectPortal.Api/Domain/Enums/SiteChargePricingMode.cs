namespace LabConnectPortal.Api.Domain.Enums;

/// <summary>
/// حالت قیمت‌گذاری سرویس شارژ:
/// Fixed = قیمت ثابت به ازای هر واحد (هر تعدادی کاربر بخواهد)،
/// Range = قیمت واحد بر اساس بازه تعداد،
/// Package = بسته با تعداد و مبلغ ثابت.
/// </summary>
public enum SiteChargePricingMode
{
    Fixed = 0,
    Range = 1,
    Package = 2,
}
