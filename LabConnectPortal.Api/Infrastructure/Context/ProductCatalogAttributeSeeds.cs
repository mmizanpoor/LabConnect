using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.Context;

internal static class ProductCatalogAttributeSeeds
{
    internal sealed record AttributeSeed(string Title, AttributeFieldType? FieldType = null);

    private static readonly AttributeSeed[] EmploymentCommon =
    [
        new("استان"),
        new("شهر"),
        new("نوع مرکز آزمایشگاهی"),
        new("نوع همکاری"),
        new("شیفت کاری"),
        new("جنسیت"),
        new("حداقل مدرک تحصیلی"),
        new("رشته/گرایش تحصیلی"),
        new("حداقل سابقه کار (سال)"),
        new("وضعیت طرح"),
        new("وضعیت نظام وظیفه"),
        new("حقوق پایه"),
        new("بیمه تأمین اجتماعی"),
        new("بیمه تکمیلی"),
        new("صبحانه/ناهار"),
        new("کارانه و پاداش"),
        new("ساعات کاری"),
        new("محل فعالیت"),
        new("آشنایی با LIS"),
        new("مهارت کنترل کیفی (IQC/EQA)"),
    ];

    private static readonly AttributeSeed[] LabTechnicalManagerExtra =
    [
        new("نوع مجوز مسئول فنی"),
        new("بخش/گرایش تخصصی"),
        new("آشنایی با ISO 15189"),
        new("تجربه مدیریت CAPA و NCR"),
        new("مهارت تفسیر نتایج"),
    ];

    private static readonly AttributeSeed[] CompanyTechnicalManagerExtra =
    [
        new("حوزه فعالیت شرکت"),
        new("نوع مجوز فنی شرکت"),
        new("آشنایی با GMP"),
        new("آشنایی با ISO 13485"),
        new("آشنایی با CE-IVD"),
    ];

    private static readonly AttributeSeed[] HumanResourceExtra =
    [
        new("سمت شغلی"),
        new("بخش فنی"),
        new("مهارت کار با دستگاه"),
        new("مهارت نمونه‌گیری"),
        new("مهارت پذیرش/جوابدهی"),
    ];

    private static readonly AttributeSeed[] EquipmentCommon =
    [
        new("برند"),
        new("مدل"),
        new("کشور سازنده"),
        new("سال ساخت"),
        new("وضعیت (نو/کارکرده/بازسازی‌شده)"),
        new("گواهی کالیبراسیون"),
        new("تاریخ اعتبار کالیبراسیون", AttributeFieldType.Date),
        new("گارانتی (ماه)"),
        new("خدمات پس از فروش"),
        new("استان"),
        new("شهر"),
    ];

    internal static IReadOnlyDictionary<string, AttributeSeed[]> ByCategory { get; } =
        new Dictionary<string, AttributeSeed[]>(StringComparer.Ordinal)
        {
            ["استخدام نیروی انسانی"] = [.. EmploymentCommon, .. HumanResourceExtra],
            ["کاریابی نیروی انسانی"] = [.. EmploymentCommon, .. HumanResourceExtra],
            ["استخدام مسئول فنی آزمایشگاه"] = [.. EmploymentCommon, .. LabTechnicalManagerExtra],
            ["کاریابی مسئول فنی آزمایشگاه"] = [.. EmploymentCommon, .. LabTechnicalManagerExtra],
            ["استخدام مسئول فنی شرکت"] = [.. EmploymentCommon, .. CompanyTechnicalManagerExtra],
            ["کاریابی مسئول فنی شرکت"] = [.. EmploymentCommon, .. CompanyTechnicalManagerExtra],

            ["کیت های آزمایشگاهی"] =
            [
                new("برند/سازنده"),
                new("شماره کاتالوگ (Cat No)"),
                new("پارامتر اندازه‌گیری (Analyte)"),
                new("روش آزمایش"),
                new("نوع نمونه"),
                new("محدوده اندازه‌گیری"),
                new("حساسیت (Sensitivity)"),
                new("تعداد تست"),
                new("شرایط نگهداری"),
                new("تاریخ انقضا", AttributeFieldType.ExpiryDate),
                new("گواهی CE/IVD"),
                new("سازگاری با دستگاه"),
                new("کنترل کیفی داخلی/خارجی"),
            ],

            ["مواد مصرفی"] =
            [
                new("برند"),
                new("شماره کاتالوگ (Cat No)"),
                new("شماره CAS"),
                new("درجه/گرید"),
                new("حجم/بسته‌بندی"),
                new("خلوص (%)"),
                new("شرایط نگهداری"),
                new("تاریخ انقضا", AttributeFieldType.ExpiryDate),
                new("MSDS موجود"),
                new("استاندارد (USP/EP/ISO)"),
                new("کاربرد در بخش"),
            ],

            ["تجهیزات عمومی"] =
            [
                .. EquipmentCommon,
                new("نوع تجهیز"),
                new("ولتاژ/برق"),
                new("ابعاد/وزن"),
                new("استاندارد CE-IVD"),
            ],

            ["تجهیزات سنجشی"] =
            [
                .. EquipmentCommon,
                new("محدوده اندازه‌گیری"),
                new("دقت (Accuracy)"),
                new("رزولوشن"),
                new("عدم قطعیت (Uncertainty)"),
                new("کالیبراسیون قابل ردیابی"),
            ],

            ["تجهیزات پایه آزمایشگاهی"] =
            [
                .. EquipmentCommon,
                new("نوع تجهیز (سانتریفuge/انکوباتور/...)"),
                new("ظرفیت/حجم کاری"),
                new("محدوده دما/سرعت"),
                new("کلاس ایمنی بیولوژیک"),
            ],

            ["قطعات و لوازم جانبی"] =
            [
                new("برند"),
                new("شماره پارت OEM"),
                new("سازگاری با مدل دستگاه"),
                new("نوع قطعه"),
                new("وضعیت (نو/کارکرده)"),
                new("گارانتی (ماه)"),
                new("استان"),
                new("شهر"),
            ],

            ["خدمات تجهیزات"] =
            [
                new("نوع خدمت"),
                new("برند/مدل پوشش‌داده‌شده"),
                new("گواهی ISO 17025"),
                new("محدوده جغرافیایی"),
                new("زمان پاسخ‌گویی"),
                new("دوره سرویس دوره‌ای (PM)"),
                new("ارائه گواهی کالیبراسیون"),
                new("استان"),
                new("شهر"),
            ],

            ["مشارکت در راه اندازی"] =
            [
                new("استان"),
                new("شهر"),
                new("نوع مشارکت"),
                new("نوع آزمایشگاه هدف"),
                new("سرمایه مورد نیاز"),
                new("سهم مشارکت"),
                new("مجوز مورد نیاز"),
                new("زمان‌بندی راه‌اندازی"),
            ],

            ["اجاره آزمایشگاه"] =
            [
                new("استان"),
                new("شهر"),
                new("متراژ (متر مربع)"),
                new("نوع مجوز فعالیت"),
                new("تجهیزات موجود"),
                new("ظرفیت روزانه نمونه"),
                new("مدت قرارداد"),
                new("ودیعه/اجاره ماهانه"),
            ],

            ["فروش آزمایشگاه"] =
            [
                new("استان"),
                new("شهر"),
                new("نوع آزمایشگاه"),
                new("متراژ (متر مربع)"),
                new("تعداد پرسنل"),
                new("ظرفیت سالانه نمونه"),
                new("مجوزهای فعال"),
                new("ارزش تجهیزات"),
                new("حجم قرارداد بیمه‌ای"),
            ],

            ["انجام تست های آزمایشگاهی"] =
            [
                new("نوع آزمایش/پنل"),
                new("استان"),
                new("شهر"),
                new("استاندارد (ISO 15189)"),
                new("زمان جوابدهی (TAT)"),
                new("نوع نمونه پذیرفته"),
                new("مشارکت در EQA"),
                new("محدوده تخصص"),
            ],

            ["مشاوره و راه اندازی"] =
            [
                new("نوع خدمت"),
                new("استان"),
                new("شهر"),
                new("حوزه تخصصی"),
                new("سابقه پروژه (سال)"),
                new("استاندارد هدف (ISO 15189/LIS/...)"),
                new("مدت قرارداد"),
            ],
        };

    internal static int ExpectedAttributeCount =>
        ByCategory.Values.Sum(v => v.Length);
}
