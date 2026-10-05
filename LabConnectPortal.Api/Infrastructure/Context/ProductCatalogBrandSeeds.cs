namespace LabConnectPortal.Api.Infrastructure.Context;

internal static class ProductCatalogBrandSeeds
{
    internal sealed record BrandSeed(string Title, string SeedFileName, bool ShowOnHomePage = true);

    internal static readonly BrandSeed[] All =
    [
        // کیت و مواد مصرفی — ایرانی
        new("پارس پیوند", "pars-peyvand.png"),
        new("حنان طب پارس", "hannan-teb-pars.svg"),
        new("روناک طب ویدا", "ronak-teb-vida.png"),
        new("زیست شیمی", "zist-shimi.svg"),
        new("کیمیا پژوهان", "kimia-pazhouhan.svg"),

        // کیت و مواد مصرفی — بین‌المللی
        new("Merck", "merck.svg"),
        new("Thermo Fisher", "thermo-fisher.svg"),

        // تجهیزات آزمایشگاهی
        new("Sysmex", "sysmex.svg"),
        new("Mindray", "mindray.png"),
        new("Roche", "roche.svg"),
        new("Abbott", "abbott.svg"),
        new("Hitachi", "hitachi.svg"),
        new("Siemens", "siemens.svg"),
        new("Beckman Coulter", "beckman-coulter.svg"),
        new("Erba", "erba.png"),
        new("Dirui", "dirui.svg"),
        new("Human", "human.svg"),

        // کامپیوتر و IT
        new("Dell", "dell.svg"),
        new("HP", "hp.svg"),
        new("Lenovo", "lenovo.svg"),
    ];
}
