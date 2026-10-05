namespace LabConnectPortal.Api.Domain;

/// <summary>
/// Stable module identifiers used for future per-entity access control.
/// Values must stay in sync with the TypeScript SystemEntity constants.
/// Names are shared across admin/profile (not role-prefixed).
/// </summary>
public static class SystemEntity
{
    public const string HeaderName = "X-System-Entity";

    public static readonly Guid Profile = Guid.Parse("f4f47b06-f392-49ee-9c11-5e694b7fa345");
    public static readonly Guid Resume = Guid.Parse("d80d4c21-1e22-46ce-9ab8-da2e43977008");
    public static readonly Guid CenterProfile = Guid.Parse("95d6e23c-744a-48ef-bcdb-a4321b7c1152");
    public static readonly Guid Location = Guid.Parse("b9246a45-0941-42a9-9fa3-5fcc08f1fbff");
    public static readonly Guid JobPosting = Guid.Parse("eec0d6a3-edaf-46ba-b76a-5562b4390367");
    public static readonly Guid ProductListing = Guid.Parse("28a46134-e146-41a1-81c3-eadba93e6a4d");
    public static readonly Guid ProductOrder = Guid.Parse("d7af789a-4d3d-4dfa-95c8-b830b414c076");
    public static readonly Guid LabUser = Guid.Parse("0d11744c-c2b6-4039-bada-b830a37423e8");
    public static readonly Guid DeviceGroup = Guid.Parse("061610d2-cb2d-4dbd-8050-effaff7fe16e");
    public static readonly Guid KitGroup = Guid.Parse("2282c3b6-aebf-4825-a112-3197078602b8");
    public static readonly Guid TestInfo = Guid.Parse("70e98d14-1a84-4607-a4b1-97438ffe1226");
    public static readonly Guid SpecialOffer = Guid.Parse("2f3cf390-447d-443c-8d66-6096c82c4e6d");
    public static readonly Guid Reception = Guid.Parse("f9208770-3e4b-4261-9fad-6015f5050b9e");
    public static readonly Guid LabAgreement = Guid.Parse("a795fa46-a728-47e5-b103-5d4f0c157972");
    public static readonly Guid Dashboard = Guid.Parse("9b2dd53b-db50-42b3-a562-0828eb0fc19e");
    public static readonly Guid User = Guid.Parse("69abf749-85b9-4c6b-ae5f-5a9998a1767d");
    public static readonly Guid Shop = Guid.Parse("c8b60aa4-2b97-45bb-bcc9-2e64628d1a1a");
    public static readonly Guid Laboratory = Guid.Parse("1a30bfe0-5159-4fe3-bf9c-f0da657cabc0");
    public static readonly Guid ProductCategory = Guid.Parse("a2988d15-00e2-4b8c-b975-d8945e45ec8d");
    public static readonly Guid ProductAttribute = Guid.Parse("b1169934-8842-4f5c-8cf4-6b26ea086e55");
    public static readonly Guid Brand = Guid.Parse("e1b65f43-d142-4902-ad0c-70382dea535b");
    public static readonly Guid Product = Guid.Parse("504586dc-bd75-4f6f-972b-c7fd583a9f8f");
    public static readonly Guid ProductReview = Guid.Parse("90673da1-e1cb-42a2-82c4-08e2a2705822");
    public static readonly Guid ContentGroup = Guid.Parse("aecc303a-0a4e-4044-ab32-8cd99b7c4de4");
    public static readonly Guid Post = Guid.Parse("21543e8f-3250-4bcd-8f8a-722ac83301aa");
    public static readonly Guid ContentNews = Guid.Parse("c4f1a8e2-3b57-4d91-9e26-7a0b5c8d2f14");
    public static readonly Guid ContentArticles = Guid.Parse("d5a2b9f3-4c68-4e02-af37-8b1c6d9e3025");
    public static readonly Guid ContentDocuments = Guid.Parse("e6b3c0a4-5d79-4f13-b048-9c2d7e0f4136");
    public static readonly Guid ContentAds = Guid.Parse("f7c4d1b5-6e8a-4024-c159-ad3e8f105247");
    public static readonly Guid SliderGroup = Guid.Parse("82db59eb-1439-44f7-b4d8-eaed4880b804");
    public static readonly Guid Message = Guid.Parse("d6679b99-cf50-4474-ae8a-50889b800bcf");
    public static readonly Guid Settings = Guid.Parse("7e320a52-136c-43cb-a65c-6753e959475d");
    public static readonly Guid SiteUser = Guid.Parse("c3e8a1f2-5b74-4d9e-9a62-1f8e0d4c7b35");
    public static readonly Guid ActivityLog = Guid.Parse("e5a1c8b3-7d4f-4e2a-9c61-8b0f3a5d2e17");
    public static readonly Guid LoginReport = Guid.Parse("b7c4e2a1-9f83-4d56-8e10-2a6c5d9b4f71");
    public static readonly Guid CompanyRegulation = Guid.Parse("a1d8e4f2-6c39-4b7a-9e15-3f8d0c2a5b64");
    public static readonly Guid SiteService = Guid.Parse("b2e9f5a3-7d4c-4e8b-9f21-4a7c6e0d3b58");

    public static IReadOnlySet<Guid> All { get; } = new HashSet<Guid>
    {
        Profile,
        Resume,
        CenterProfile,
        Location,
        JobPosting,
        ProductListing,
        ProductOrder,
        LabUser,
        DeviceGroup,
        KitGroup,
        TestInfo,
        SpecialOffer,
        Reception,
        LabAgreement,
        Dashboard,
        User,
        Shop,
        Laboratory,
        ProductCategory,
        ProductAttribute,
        Brand,
        Product,
        ProductReview,
        ContentGroup,
        Post,
        ContentNews,
        ContentArticles,
        ContentDocuments,
        ContentAds,
        SliderGroup,
        Message,
        Settings,
        SiteUser,
        ActivityLog,
        LoginReport,
        CompanyRegulation,
        SiteService,
    };

    public static bool IsKnown(Guid id) => All.Contains(id);

    public static IReadOnlyList<Guid> LabPortalAll { get; } = new List<Guid>
    {
        CenterProfile,
        Location,
        JobPosting,
        Product,
        LabUser,
        DeviceGroup,
        KitGroup,
        TestInfo,
        SpecialOffer,
        Reception,
        LabAgreement,
        Message,
        ActivityLog,
    };

    public static bool IsLabPortalEntity(Guid id) => LabPortalAll.Contains(id);

    /// <summary>
    /// Entities assignable/enforceable for UserType.Admin (site staff).
    /// SiteUser management stays Administrator-only and is excluded.
    /// </summary>
    public static IReadOnlyList<Guid> SiteAdminAll { get; } = new List<Guid>
    {
        User,
        Shop,
        Laboratory,
        ProductCategory,
        ProductAttribute,
        Brand,
        Product,
        ContentGroup,
        Post,
        ContentNews,
        ContentArticles,
        ContentDocuments,
        ContentAds,
        SliderGroup,
        SpecialOffer,
        Message,
        Settings,
        ActivityLog,
        LoginReport,
        CompanyRegulation,
    };

    public static bool IsSiteAdminEntity(Guid id) => SiteAdminAll.Contains(id);
}
