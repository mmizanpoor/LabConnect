using LabConnectPortal.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Context;

public class LabConnectDbContext : DbContext
{
    public LabConnectDbContext(DbContextOptions<LabConnectDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<LabUserPermission> LabUserPermissions => Set<LabUserPermission>();
    public DbSet<SiteUserPermission> SiteUserPermissions => Set<SiteUserPermission>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<CenterProfile> CenterProfiles => Set<CenterProfile>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();

    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<JobCategory> JobCategories => Set<JobCategory>();
    public DbSet<SalaryRange> SalaryRanges => Set<SalaryRange>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<LanguageName> LanguageNames => Set<LanguageName>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
    public DbSet<EducationalBackground> EducationalBackgrounds => Set<EducationalBackground>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<UserLanguage> UserLanguages => Set<UserLanguage>();
    public DbSet<JobPreference> JobPreferences => Set<JobPreference>();
    public DbSet<JobPreferenceProvince> JobPreferenceProvinces => Set<JobPreferenceProvince>();
    public DbSet<JobPreferenceJobCategory> JobPreferenceJobCategories => Set<JobPreferenceJobCategory>();
    public DbSet<JobPreferenceSeniorityLevel> JobPreferenceSeniorityLevels => Set<JobPreferenceSeniorityLevel>();
    public DbSet<JobPreferenceContractType> JobPreferenceContractTypes => Set<JobPreferenceContractType>();
    public DbSet<OrganizationLocation> OrganizationLocations => Set<OrganizationLocation>();
    public DbSet<JobPostingRequest> JobPostingRequests => Set<JobPostingRequest>();
    public DbSet<JobPostingContractType> JobPostingContractTypes => Set<JobPostingContractType>();
    public DbSet<JobPostingEssentialSkill> JobPostingEssentialSkills => Set<JobPostingEssentialSkill>();
    public DbSet<JobPostingPersonalTrait> JobPostingPersonalTraits => Set<JobPostingPersonalTrait>();
    public DbSet<JobPostingBenefit> JobPostingBenefits => Set<JobPostingBenefit>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<ProductResumeApplication> ProductResumeApplications => Set<ProductResumeApplication>();

    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductCategoryGroup> ProductCategoryGroups => Set<ProductCategoryGroup>();
    public DbSet<ProductCategoryPrice> ProductCategoryPrices => Set<ProductCategoryPrice>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<SiteService> SiteServices => Set<SiteService>();
    public DbSet<SiteChargeService> SiteChargeServices => Set<SiteChargeService>();
    public DbSet<SiteChargeServicePrice> SiteChargeServicePrices => Set<SiteChargeServicePrice>();
    public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategoryAssignment> ProductCategoryAssignments => Set<ProductCategoryAssignment>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductExpertReview> ProductExpertReviews => Set<ProductExpertReview>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<ProductOrder> ProductOrders => Set<ProductOrder>();
    public DbSet<ProductOrderItem> ProductOrderItems => Set<ProductOrderItem>();
    public DbSet<ProductUserReview> ProductUserReviews => Set<ProductUserReview>();
    public DbSet<ProductReviewReply> ProductReviewReplies => Set<ProductReviewReply>();
    public DbSet<ProductView> ProductViews => Set<ProductView>();
    public DbSet<SiteVisit> SiteVisits => Set<SiteVisit>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<SiteUsefulLink> SiteUsefulLinks => Set<SiteUsefulLink>();
    public DbSet<SliderGroup> SliderGroups => Set<SliderGroup>();
    public DbSet<SliderSlide> SliderSlides => Set<SliderSlide>();
    public DbSet<ContentGroup> ContentGroups => Set<ContentGroup>();
    public DbSet<ContentPost> ContentPosts => Set<ContentPost>();
    public DbSet<CompanyRegulation> CompanyRegulations => Set<CompanyRegulation>();
    public DbSet<Advertisement> Advertisements => Set<Advertisement>();
    public DbSet<AdvertisementPosition> AdvertisementPositions => Set<AdvertisementPosition>();
    public DbSet<AdvertisementDuration> AdvertisementDurations => Set<AdvertisementDuration>();
    public DbSet<AdvertisementPrice> AdvertisementPrices => Set<AdvertisementPrice>();
    public DbSet<AdvertisementOrder> AdvertisementOrders => Set<AdvertisementOrder>();
    public DbSet<DeviceGroup> DeviceGroups => Set<DeviceGroup>();
    public DbSet<KitGroup> KitGroups => Set<KitGroup>();
    public DbSet<TestInfo> TestInfos => Set<TestInfo>();
    public DbSet<SpecialOffer> SpecialOffers => Set<SpecialOffer>();
    public DbSet<SpecialOfferTest> SpecialOfferTests => Set<SpecialOfferTest>();
    public DbSet<SpecialOfferRequest> SpecialOfferRequests => Set<SpecialOfferRequest>();
    public DbSet<LabAgreement> LabAgreements => Set<LabAgreement>();
    public DbSet<LabAgreementAttachment> LabAgreementAttachments => Set<LabAgreementAttachment>();
    public DbSet<LabAgreementTestPrice> LabAgreementTestPrices => Set<LabAgreementTestPrice>();
    public DbSet<LabAgreementStatusHistory> LabAgreementStatusHistories => Set<LabAgreementStatusHistory>();
    public DbSet<LabAgreementSettings> LabAgreementSettings => Set<LabAgreementSettings>();
    public DbSet<SsoAuth> SsoAuths => Set<SsoAuth>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<UserLoginLog> UserLoginLogs => Set<UserLoginLog>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("dbo");

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => new { u.MobileNumber, u.UserType }).IsUnique();
            entity.HasIndex(u => u.CenterProfileId);
            entity.HasIndex(u => u.Email);
            entity.HasIndex(u => u.LastSeenAt);
            entity.Property(u => u.Latitude).HasPrecision(9, 6);
            entity.Property(u => u.Longitude).HasPrecision(9, 6);
            entity.HasOne(u => u.CenterProfile)
                .WithMany()
                .HasForeignKey(u => u.CenterProfileId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<LabUserPermission>(entity =>
        {
            entity.HasIndex(p => new { p.UserId, p.SystemEntityId }).IsUnique();
            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SiteUserPermission>(entity =>
        {
            entity.HasIndex(p => new { p.UserId, p.SystemEntityId }).IsUnique();
            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OtpCode>(entity =>
        {
            entity.HasIndex(o => new { o.MobileNumber, o.Purpose, o.IsUsed });
        });

        modelBuilder.Entity<SsoAuth>(entity =>
        {
            entity.ToTable("SSOAuth");
            entity.HasIndex(s => s.Username).IsUnique();
            entity.Property(s => s.SessionId).HasMaxLength(2000);
        });

        modelBuilder.Entity<CenterProfile>(entity =>
        {
            entity.HasIndex(p => p.LabCode)
                .IsUnique()
                .HasFilter("[LabCode] IS NOT NULL AND [CenterType] = 0");
            entity.HasIndex(p => p.LabCodeNew)
                .IsUnique()
                .HasFilter("[LabCodeNew] IS NOT NULL AND [CenterType] = 0");
            entity.HasIndex(p => p.OwnerUserId)
                .IsUnique()
                .HasFilter("[OwnerUserId] IS NOT NULL AND [CenterType] = 1");
        });

        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasIndex(x => x.KeyName).IsUnique();
            entity.HasIndex(x => new { x.CenterProfileId, x.CreateDate });
            entity.HasOne(x => x.CenterProfile)
                .WithMany()
                .HasForeignKey(x => x.CenterProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.HasIndex(n => new { n.UserId, n.IsRead });
            entity.HasIndex(n => n.CreatedAt);
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.HasIndex(p => p.ProvinceName).IsUnique();
        });

        modelBuilder.Entity<JobCategory>(entity =>
        {
            entity.HasIndex(c => c.CategoryName).IsUnique();
        });

        modelBuilder.Entity<SalaryRange>(entity =>
        {
            entity.HasIndex(s => s.SalaryRangeDescription).IsUnique();
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasIndex(s => s.SkillName).IsUnique();
        });

        modelBuilder.Entity<LanguageName>(entity =>
        {
            entity.HasIndex(l => l.Name).IsUnique();
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.Province)
                .WithMany()
                .HasForeignKey(p => p.ProvinceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkExperience>(entity =>
        {
            entity.HasOne(w => w.User)
                .WithMany(u => u.WorkExperiences)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EducationalBackground>(entity =>
        {
            entity.HasOne(e => e.User)
                .WithMany(u => u.EducationalBackgrounds)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserSkill>(entity =>
        {
            entity.HasIndex(s => new { s.UserId, s.SkillId }).IsUnique();
            entity.HasOne(s => s.User)
                .WithMany(u => u.UserSkills)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.Skill)
                .WithMany()
                .HasForeignKey(s => s.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserLanguage>(entity =>
        {
            entity.HasIndex(l => new { l.UserId, l.LanguageNameId }).IsUnique();
            entity.HasOne(l => l.User)
                .WithMany(u => u.UserLanguages)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(l => l.LanguageName)
                .WithMany()
                .HasForeignKey(l => l.LanguageNameId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JobPreference>(entity =>
        {
            entity.HasOne(j => j.User)
                .WithOne(u => u.JobPreference)
                .HasForeignKey<JobPreference>(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(j => j.MinimumSalary)
                .WithMany()
                .HasForeignKey(j => j.MinimumSalaryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<JobPreferenceProvince>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.ProvinceId }).IsUnique();
            entity.HasOne(x => x.JobPreference)
                .WithMany(j => j.PreferredProvinces)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Province)
                .WithMany()
                .HasForeignKey(x => x.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JobPreferenceJobCategory>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.JobCategoryId }).IsUnique();
            entity.HasOne(x => x.JobPreference)
                .WithMany(j => j.JobCategories)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.JobCategory)
                .WithMany()
                .HasForeignKey(x => x.JobCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JobPreferenceSeniorityLevel>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.SeniorityLevel }).IsUnique();
            entity.HasOne(x => x.JobPreference)
                .WithMany(j => j.SeniorityLevels)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobPreferenceContractType>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.ContractType }).IsUnique();
            entity.HasOne(x => x.JobPreference)
                .WithMany(j => j.AcceptableContractTypes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrganizationLocation>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.LocationName }).IsUnique();
            entity.HasOne(x => x.User)
                .WithMany(u => u.OrganizationLocations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Province)
                .WithMany()
                .HasForeignKey(x => x.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JobPostingRequest>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.Status });
            entity.HasOne(x => x.User)
                .WithMany(u => u.JobPostings)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.JobCategory)
                .WithMany()
                .HasForeignKey(x => x.JobCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Location)
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SalaryRange)
                .WithMany()
                .HasForeignKey(x => x.SalaryRangeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JobPostingContractType>(entity =>
        {
            entity.HasIndex(x => new { x.JobPostingId, x.ContractType }).IsUnique();
            entity.HasOne(x => x.JobPosting)
                .WithMany(j => j.ContractTypes)
                .HasForeignKey(x => x.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobPostingEssentialSkill>(entity =>
        {
            entity.HasIndex(x => new { x.JobPostingId, x.SkillId }).IsUnique();
            entity.HasOne(x => x.JobPosting)
                .WithMany(j => j.EssentialSkills)
                .HasForeignKey(x => x.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Skill)
                .WithMany()
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JobPostingPersonalTrait>(entity =>
        {
            entity.HasOne(x => x.JobPosting)
                .WithMany(j => j.PersonalTraits)
                .HasForeignKey(x => x.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobPostingBenefit>(entity =>
        {
            entity.HasOne(x => x.JobPosting)
                .WithMany(j => j.Benefits)
                .HasForeignKey(x => x.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasIndex(x => new { x.JobPostingId, x.ApplicantUserId }).IsUnique();
            entity.HasOne(x => x.JobPosting)
                .WithMany(j => j.Applications)
                .HasForeignKey(x => x.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ApplicantUser)
                .WithMany(u => u.JobApplications)
                .HasForeignKey(x => x.ApplicantUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductResumeApplication>(entity =>
        {
            entity.HasIndex(x => new { x.ProductId, x.ApplicantUserId }).IsUnique();
            entity.HasOne(x => x.Product)
                .WithMany(p => p.ResumeApplications)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.ApplicantUser)
                .WithMany(u => u.ProductResumeApplications)
                .HasForeignKey(x => x.ApplicantUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasIndex(c => c.Title).IsUnique();
            entity.HasIndex(c => c.ProductCategoryGroupId);
            entity.HasOne(c => c.CategoryGroup)
                .WithMany(g => g.Categories)
                .HasForeignKey(c => c.ProductCategoryGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductCategoryPrice>(entity =>
        {
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.HasIndex(x => x.ProductCategoryId).IsUnique();
            entity.HasOne(x => x.ProductCategory)
                .WithMany()
                .HasForeignKey(x => x.ProductCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.UpdatedByUser)
                .WithMany()
                .HasForeignKey(x => x.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SiteChargeService>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasOne(x => x.UpdatedByUser)
                .WithMany()
                .HasForeignKey(x => x.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasData(new SiteChargeService
            {
                SiteChargeServiceId = 1,
                Code = Domain.Enums.SiteChargeServiceCode.Sms,
                Title = "شارژ پیامک",
                Description = "سرویس شارژ اعتبار پیامک مرکز",
                PricingMode = Domain.Enums.SiteChargePricingMode.Fixed,
                IsActive = true,
                UpdatedAt = new DateTime(2026, 9, 12, 0, 0, 0, DateTimeKind.Utc),
            });
        });

        modelBuilder.Entity<SiteChargeServicePrice>(entity =>
        {
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.HasIndex(x => x.SiteChargeServiceId);
            entity.HasOne(x => x.Service)
                .WithMany(s => s.Prices)
                .HasForeignKey(x => x.SiteChargeServiceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasData(new SiteChargeServicePrice
            {
                SiteChargeServicePriceId = 1,
                SiteChargeServiceId = 1,
                Title = null,
                MinQuantity = null,
                MaxQuantity = null,
                PackageQuantity = null,
                Price = 200m,
                SortOrder = 0,
            });
        });

        modelBuilder.Entity<PaymentOrder>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.SystemTransactionKey).HasMaxLength(100);
            entity.Property(x => x.BankOrderKey).HasMaxLength(200);
            entity.Property(x => x.StatusCode).HasMaxLength(50);
            entity.Property(x => x.RefId).HasMaxLength(100);
            entity.Property(x => x.CardNumber).HasMaxLength(50);
            entity.Property(x => x.CardHash).HasMaxLength(200);
            entity.Property(x => x.Message).HasMaxLength(1000);
        });

        modelBuilder.Entity<ProductCategoryGroup>(entity =>
        {
            entity.HasIndex(g => g.Name).IsUnique();
        });

        modelBuilder.Entity<ProductAttribute>(entity =>
        {
            entity.HasIndex(a => new { a.ProductCategoryId, a.Title }).IsUnique();
            entity.HasOne(a => a.Category)
                .WithMany(c => c.Attributes)
                .HasForeignKey(a => a.ProductCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasIndex(b => b.Title).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Title);
            entity.HasIndex(p => p.ViewCount);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.CreatedByCenterProfileId);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.DiscountPercent).HasPrecision(18, 2);
            entity.Property(p => p.Latitude).HasPrecision(9, 6);
            entity.Property(p => p.Longitude).HasPrecision(9, 6);
            entity.HasIndex(p => p.ProvinceId);
            entity.HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            entity.HasOne(p => p.Province)
                .WithMany()
                .HasForeignKey(p => p.ProvinceId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(p => p.CreatedByCenterProfile)
                .WithMany()
                .HasForeignKey(p => p.CreatedByCenterProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(p => p.ApprovedByUser)
                .WithMany()
                .HasForeignKey(p => p.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductView>(entity =>
        {
            entity.HasIndex(x => new { x.ProductId, x.IpHash }).IsUnique();
            entity.HasIndex(x => x.ProductId);
            entity.HasOne(x => x.Product)
                .WithMany(p => p.Views)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SiteVisit>(entity =>
        {
            entity.HasIndex(x => x.ViewedAt);
            entity.HasIndex(x => new { x.IpHash, x.ViewedAt });
        });

        modelBuilder.Entity<ProductCategoryAssignment>(entity =>
        {
            entity.HasIndex(x => new { x.ProductId, x.ProductCategoryId }).IsUnique();
            entity.HasOne(x => x.Product)
                .WithMany(p => p.CategoryAssignments)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Category)
                .WithMany(c => c.ProductAssignments)
                .HasForeignKey(x => x.ProductCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductAttributeValue>(entity =>
        {
            entity.HasIndex(x => new { x.ProductId, x.ProductAttributeId }).IsUnique();
            entity.HasOne(x => x.Product)
                .WithMany(p => p.AttributeValues)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Attribute)
                .WithMany(a => a.Values)
                .HasForeignKey(x => x.ProductAttributeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasOne(x => x.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductExpertReview>(entity =>
        {
            entity.HasOne(x => x.Product)
                .WithMany(p => p.ExpertReviews)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasIndex(c => c.UserId).IsUnique();
            entity.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(x => new { x.CartId, x.ProductId })
                .IsUnique()
                .HasFilter("[ProductId] IS NOT NULL");
            entity.HasOne(x => x.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(x => x.CartId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductOrder>(entity =>
        {
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.ShippingCost).HasPrecision(18, 2);
            entity.Property(x => x.ShippingRecipientName).HasMaxLength(200);
            entity.Property(x => x.ShippingAddress).HasMaxLength(500);
            entity.Property(x => x.ShippingPhone).HasMaxLength(20);
            entity.Property(x => x.TrackingCode).HasMaxLength(100);
            entity.Property(x => x.ShippingCompany).HasMaxLength(200);
            entity.Property(x => x.DeliveryNotes).HasMaxLength(1000);
            entity.HasIndex(x => new { x.UserId, x.Status });
            entity.HasIndex(x => new { x.CenterProfileId, x.Status });
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CenterProfile)
                .WithMany()
                .HasForeignKey(x => x.CenterProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductOrderItem>(entity =>
        {
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.DiscountPercent).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.HasOne(x => x.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductUserReview>(entity =>
        {
            entity.HasIndex(x => x.OrderItemId).IsUnique();
            entity.HasOne(x => x.OrderItem)
                .WithOne(i => i.UserReview)
                .HasForeignKey<ProductUserReview>(x => x.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductReviewReply>(entity =>
        {
            entity.HasOne(x => x.Review)
                .WithMany(r => r.Replies)
                .HasForeignKey(x => x.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SiteSettings>(entity =>
        {
            entity.HasData(new SiteSettings
            {
                Id = 1,
                SiteTitle = "LabConnect Portal",
                Tagline = string.Empty,
                UpdatedAt = new DateTime(2026, 6, 13, 0, 0, 0, DateTimeKind.Utc),
            });
        });

        modelBuilder.Entity<SiteUsefulLink>(entity =>
        {
            entity.HasIndex(x => x.SortOrder);
        });

        modelBuilder.Entity<SliderGroup>(entity =>
        {
            entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => new { x.StartDate, x.EndDate });
        });

        modelBuilder.Entity<SliderSlide>(entity =>
        {
            entity.HasIndex(x => new { x.SliderGroupId, x.SortOrder });
            entity.HasOne(x => x.SliderGroup)
                .WithMany(g => g.Slides)
                .HasForeignKey(x => x.SliderGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ContentGroup>(entity =>
        {
            entity.HasIndex(g => g.Title).IsUnique();
            entity.HasIndex(g => g.Code).IsUnique().HasFilter("[Code] IS NOT NULL");
        });

        modelBuilder.Entity<ContentPost>(entity =>
        {
            entity.HasIndex(p => p.Type);
            entity.HasIndex(p => p.IsActive);
            entity.HasIndex(p => p.ShowOnHomePage);
            entity.HasIndex(p => p.PublishedAt);
            entity.HasOne(p => p.Group)
                .WithMany(g => g.Posts)
                .HasForeignKey(p => p.ContentGroupId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CompanyRegulation>(entity =>
        {
            entity.HasIndex(x => x.Type).IsUnique();
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.ModifiedAt);
        });

        modelBuilder.Entity<Advertisement>(entity =>
        {
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => new { x.IsActive, x.StartAt, x.EndAt });
            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.AdvertisementPositionId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Order)
                .WithOne(x => x.Advertisement)
                .HasForeignKey<Advertisement>(x => x.AdvertisementOrderId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => x.AdvertisementOrderId).IsUnique().HasFilter("[AdvertisementOrderId] IS NOT NULL");
        });

        modelBuilder.Entity<AdvertisementPosition>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.IsActive);
        });

        modelBuilder.Entity<AdvertisementDuration>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.SortOrder);
        });

        modelBuilder.Entity<AdvertisementPrice>(entity =>
        {
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.HasIndex(x => new { x.AdvertisementPositionId, x.AdvertisementDurationId, x.ValidFrom });
            entity.HasOne(x => x.Position)
                .WithMany(x => x.Prices)
                .HasForeignKey(x => x.AdvertisementPositionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Duration)
                .WithMany(x => x.Prices)
                .HasForeignKey(x => x.AdvertisementDurationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AdvertisementOrder>(entity =>
        {
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => new { x.AdvertisementPositionId, x.Status, x.StartDate, x.EndDate });
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Position)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.AdvertisementPositionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Duration)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.AdvertisementDurationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PriceRecord)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.AdvertisementPriceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DeviceGroup>(entity =>
        {
            entity.HasIndex(g => new { g.LabCodeNew, g.Title }).IsUnique();
            entity.HasIndex(g => g.LabCodeNew);
        });

        modelBuilder.Entity<KitGroup>(entity =>
        {
            entity.HasIndex(g => new { g.LabCodeNew, g.Title }).IsUnique();
            entity.HasIndex(g => g.LabCodeNew);
        });

        modelBuilder.Entity<TestInfo>(entity =>
        {
            entity.Property(t => t.ApprovePrice).HasPrecision(18, 2);
            entity.HasIndex(t => t.LabCodeNew);
            entity.HasIndex(t => t.TestId);
            entity.HasOne(t => t.KitGroup)
                .WithMany()
                .HasForeignKey(t => t.KitGroupId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(t => t.DeviceGroup)
                .WithMany()
                .HasForeignKey(t => t.DeviceGroupId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SpecialOffer>(entity =>
        {
            entity.HasIndex(o => o.LabCodeNew);
            entity.HasIndex(o => new { o.IsActive, o.EndDate });
        });

        modelBuilder.Entity<SpecialOfferTest>(entity =>
        {
            entity.Property(t => t.Discount).HasPrecision(5, 2);
            entity.HasIndex(t => new { t.SpecialOfferId, t.TestInfoId }).IsUnique();
            entity.HasOne(t => t.SpecialOffer)
                .WithMany(o => o.Tests)
                .HasForeignKey(t => t.SpecialOfferId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(t => t.TestInfo)
                .WithMany()
                .HasForeignKey(t => t.TestInfoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SpecialOfferRequest>(entity =>
        {
            entity.HasIndex(r => new { r.SpecialOfferId, r.UserId }).IsUnique();
            entity.HasOne(r => r.SpecialOffer)
                .WithMany(o => o.Requests)
                .HasForeignKey(r => r.SpecialOfferId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.LabAgreement)
                .WithMany()
                .HasForeignKey(r => r.LabAgreementId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<LabAgreementTestPrice>(entity =>
        {
            entity.Property(x => x.Approved).HasPrecision(18, 2);
            entity.Property(x => x.BaseTariffApproved).HasPrecision(18, 2);
            entity.Property(x => x.FirstAdditions).HasPrecision(18, 2);
            entity.Property(x => x.SecondAdditions).HasPrecision(18, 2);
            entity.Property(x => x.UrgentAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<LabAgreementStatusHistory>(entity =>
        {
            entity.HasIndex(x => new { x.LabAgreementId, x.ActionDateTime });
            entity.Property(x => x.ActionUserName).HasMaxLength(500);
            entity.Property(x => x.Reason).HasMaxLength(2000);
            entity.HasOne(x => x.LabAgreement)
                .WithMany(a => a.StatusHistories)
                .HasForeignKey(x => x.LabAgreementId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LabAgreementSettings>(entity =>
        {
            entity.HasIndex(x => x.CenterProfileId).IsUnique();
            entity.Property(x => x.LabName).HasMaxLength(200);
            entity.Property(x => x.HeaderAddress).HasMaxLength(500);
            entity.Property(x => x.Description1).HasMaxLength(2000);
            entity.Property(x => x.HeaderImagePath).HasMaxLength(500);
            entity.Property(x => x.HeaderLogoPath).HasMaxLength(500);
            entity.HasOne(x => x.CenterProfile)
                .WithMany()
                .HasForeignKey(x => x.CenterProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasIndex(x => new { x.EntityName, x.RecordKey, x.CreatedAt });
            entity.HasIndex(x => new { x.CenterProfileId, x.CreatedAt });
            entity.HasIndex(x => x.BatchId);
            entity.HasIndex(x => new { x.UserId, x.CreatedAt });
            entity.Property(x => x.UserType).HasMaxLength(30);
            entity.Property(x => x.Action).HasMaxLength(20);
            entity.Property(x => x.EntityName).HasMaxLength(100);
            entity.Property(x => x.RecordKey).HasMaxLength(64);
            entity.Property(x => x.RecordTitle).HasMaxLength(300);
            entity.Property(x => x.FieldName).HasMaxLength(100);
            entity.Property(x => x.IpAddress).HasMaxLength(45);
            entity.Property(x => x.UserAgent).HasMaxLength(500);
        });

        modelBuilder.Entity<UserLoginLog>(entity =>
        {
            entity.ToTable("UserLoginLogs", "dbo");
            entity.HasIndex(x => new { x.UserId, x.CreatedAt });
            entity.HasIndex(x => new { x.Success, x.CreatedAt });
            entity.HasIndex(x => new { x.LoginMethod, x.CreatedAt });
            entity.HasIndex(x => x.CreatedAt);
            entity.Property(x => x.Username).HasMaxLength(100);
            entity.Property(x => x.MobileNumber).HasMaxLength(20);
            entity.Property(x => x.IpAddress).HasMaxLength(45);
            entity.Property(x => x.UserAgent).HasMaxLength(500);
            entity.Property(x => x.FailureReason).HasMaxLength(300);
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices", "dbo");
            entity.HasIndex(x => new { x.PrimaryLabCodeNew, x.TargetLabCodeNew });
            entity.HasIndex(x => x.CreateDateTime);
            entity.Property(x => x.Username).HasMaxLength(256);
            entity.Property(x => x.State).HasDefaultValue(1);
        });
    }
}
