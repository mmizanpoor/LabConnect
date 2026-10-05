using LabConnectPortal.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LabConnectPortal.Api.Infrastructure.Audit;

public sealed class AuditableEntityRegistry : IAuditableEntityRegistry
{
    private static readonly HashSet<string> DefaultIgnored = new(StringComparer.Ordinal)
    {
        "CreatedAt",
        "UpdatedAt",
        "ViewCount",
    };

    private static readonly HashSet<string> UserIgnored = new(StringComparer.Ordinal)
    {
        "CreatedAt",
        "UpdatedAt",
        "ViewCount",
        "PasswordHash",
        "RefreshToken",
        "RefreshTokenExpiryTime",
        "EmailConfirmationToken",
        "LastSeenAt",
    };

    private readonly Dictionary<Type, AuditableEntityDescriptor> _byType;

    public AuditableEntityRegistry()
    {
        _byType = new Dictionary<Type, AuditableEntityDescriptor>
        {
            [typeof(Product)] = Create(typeof(Product), nameof(Product), e => ((Product)e).ProductId.ToString(), NoCenter),
            [typeof(Brand)] = Create(typeof(Brand), nameof(Brand), e => ((Brand)e).BrandId.ToString(), NoCenter),
            [typeof(ProductCategory)] = Create(typeof(ProductCategory), nameof(ProductCategory), e => ((ProductCategory)e).ProductCategoryId.ToString(), NoCenter),
            [typeof(ProductCategoryGroup)] = Create(typeof(ProductCategoryGroup), nameof(ProductCategoryGroup), e => ((ProductCategoryGroup)e).ProductCategoryGroupId.ToString(), NoCenter, e => ((ProductCategoryGroup)e).Name),
            [typeof(ProductAttribute)] = Create(typeof(ProductAttribute), nameof(ProductAttribute), e => ((ProductAttribute)e).ProductAttributeId.ToString(), NoCenter),
            [typeof(ProductImage)] = Create(typeof(ProductImage), nameof(ProductImage), e => ((ProductImage)e).Id.ToString(), NoCenter),
            [typeof(ProductExpertReview)] = Create(typeof(ProductExpertReview), nameof(ProductExpertReview), e => ((ProductExpertReview)e).Id.ToString(), NoCenter),
            [typeof(ProductCategoryAssignment)] = Create(typeof(ProductCategoryAssignment), nameof(ProductCategoryAssignment), e => ((ProductCategoryAssignment)e).Id.ToString(), NoCenter),
            [typeof(ProductAttributeValue)] = Create(typeof(ProductAttributeValue), nameof(ProductAttributeValue), e => ((ProductAttributeValue)e).Id.ToString(), NoCenter),
            [typeof(ContentGroup)] = Create(typeof(ContentGroup), nameof(ContentGroup), e => ((ContentGroup)e).ContentGroupId.ToString(), NoCenter),
            [typeof(ContentPost)] = Create(typeof(ContentPost), nameof(ContentPost), e => ((ContentPost)e).ContentPostId.ToString(), NoCenter),
            [typeof(SliderGroup)] = Create(typeof(SliderGroup), nameof(SliderGroup), e => ((SliderGroup)e).Id.ToString(), NoCenter),
            [typeof(SliderSlide)] = Create(typeof(SliderSlide), nameof(SliderSlide), e => ((SliderSlide)e).Id.ToString(), NoCenter),
            [typeof(SiteUsefulLink)] = Create(typeof(SiteUsefulLink), nameof(SiteUsefulLink), e => ((SiteUsefulLink)e).Id.ToString(), NoCenter),
            [typeof(SiteSettings)] = Create(typeof(SiteSettings), nameof(SiteSettings), e => ((SiteSettings)e).Id.ToString(), NoCenter, e => ((SiteSettings)e).SiteTitle),
            [typeof(SiteUserPermission)] = Create(typeof(SiteUserPermission), nameof(SiteUserPermission), e => ((SiteUserPermission)e).Id.ToString(), NoCenter),
            [typeof(ProductUserReview)] = Create(typeof(ProductUserReview), nameof(ProductUserReview), e => ((ProductUserReview)e).Id.ToString(), NoCenter),
            [typeof(ProductReviewReply)] = Create(typeof(ProductReviewReply), nameof(ProductReviewReply), e => ((ProductReviewReply)e).Id.ToString(), NoCenter),

            [typeof(CenterProfile)] = Create(typeof(CenterProfile), nameof(CenterProfile), e => ((CenterProfile)e).Id.ToString(), (entry, _) => GuidProp(entry, nameof(CenterProfile.Id))),
            [typeof(SpecialOffer)] = Create(typeof(SpecialOffer), nameof(SpecialOffer), e => ((SpecialOffer)e).Id.ToString(), (entry, ctx) => ResolveByLabCode(entry, ctx, nameof(SpecialOffer.LabCodeNew))),
            [typeof(SpecialOfferTest)] = Create(typeof(SpecialOfferTest), nameof(SpecialOfferTest), e => ((SpecialOfferTest)e).Id.ToString(), ResolveSpecialOfferTestCenter),
            [typeof(SpecialOfferRequest)] = Create(typeof(SpecialOfferRequest), nameof(SpecialOfferRequest), e => ((SpecialOfferRequest)e).Id.ToString(), (entry, ctx) => ResolveByLabCode(entry, ctx, nameof(SpecialOfferRequest.RequesterLabCodeNew))),
            [typeof(DeviceGroup)] = Create(typeof(DeviceGroup), nameof(DeviceGroup), e => ((DeviceGroup)e).Id.ToString(), (entry, ctx) => ResolveByLabCode(entry, ctx, nameof(DeviceGroup.LabCodeNew))),
            [typeof(KitGroup)] = Create(typeof(KitGroup), nameof(KitGroup), e => ((KitGroup)e).Id.ToString(), (entry, ctx) => ResolveByLabCode(entry, ctx, nameof(KitGroup.LabCodeNew))),
            [typeof(TestInfo)] = Create(typeof(TestInfo), nameof(TestInfo), e => ((TestInfo)e).Id.ToString(), (entry, ctx) => ResolveByLabCode(entry, ctx, nameof(TestInfo.LabCodeNew)), e => ((TestInfo)e).FullName ?? ((TestInfo)e).ShortName),
            [typeof(LabAgreement)] = Create(typeof(LabAgreement), nameof(LabAgreement), e => ((LabAgreement)e).Id.ToString(), (entry, ctx) => ResolveByLabCode(entry, ctx, nameof(LabAgreement.PrimaryAgreementLabCodeNew))),
            [typeof(LabAgreementAttachment)] = Create(typeof(LabAgreementAttachment), nameof(LabAgreementAttachment), e => ((LabAgreementAttachment)e).Id.ToString(), ResolveLabAgreementAttachmentCenter, e => ((LabAgreementAttachment)e).FileName),
            [typeof(LabAgreementTestPrice)] = Create(typeof(LabAgreementTestPrice), nameof(LabAgreementTestPrice), e => ((LabAgreementTestPrice)e).Id.ToString(), ResolveLabAgreementTestPriceCenter, e => ((LabAgreementTestPrice)e).TestName),
            [typeof(ProductOrder)] = Create(typeof(ProductOrder), nameof(ProductOrder), e => ((ProductOrder)e).Id.ToString(), (entry, _) => GuidProp(entry, nameof(ProductOrder.CenterProfileId))),
            [typeof(ProductOrderItem)] = Create(typeof(ProductOrderItem), nameof(ProductOrderItem), e => ((ProductOrderItem)e).Id.ToString(), ResolveOrderItemCenter),
            [typeof(User)] = Create(
                typeof(User),
                nameof(User),
                e => ((User)e).Id.ToString(),
                (entry, _) => GuidProp(entry, nameof(User.CenterProfileId)),
                e =>
                {
                    var u = (User)e;
                    var name = $"{u.FirstName} {u.LastName}".Trim();
                    return string.IsNullOrWhiteSpace(name) ? u.Username : name;
                },
                UserIgnored),
            [typeof(LabUserPermission)] = Create(typeof(LabUserPermission), nameof(LabUserPermission), e => ((LabUserPermission)e).Id.ToString(), ResolveLabUserPermissionCenter),
            [typeof(OrganizationLocation)] = Create(typeof(OrganizationLocation), nameof(OrganizationLocation), e => ((OrganizationLocation)e).LocationId.ToString(), ResolveOrgLocationCenter, e => ((OrganizationLocation)e).LocationName),
            [typeof(JobPostingRequest)] = Create(typeof(JobPostingRequest), nameof(JobPostingRequest), e => ((JobPostingRequest)e).JobPostingId.ToString(), ResolveJobPostingCenter),
            [typeof(JobPostingBenefit)] = Create(typeof(JobPostingBenefit), nameof(JobPostingBenefit), e => ((JobPostingBenefit)e).Id.ToString(), (entry, ctx) => ResolveJobPostingChildByPostingId(ctx, GuidProp(entry, nameof(JobPostingBenefit.JobPostingId))), e => ((JobPostingBenefit)e).BenefitText),
            [typeof(JobPostingContractType)] = Create(typeof(JobPostingContractType), nameof(JobPostingContractType), e => ((JobPostingContractType)e).Id.ToString(), (entry, ctx) => ResolveJobPostingChildByPostingId(ctx, GuidProp(entry, nameof(JobPostingContractType.JobPostingId)))),
            [typeof(JobPostingEssentialSkill)] = Create(typeof(JobPostingEssentialSkill), nameof(JobPostingEssentialSkill), e => ((JobPostingEssentialSkill)e).Id.ToString(), (entry, ctx) => ResolveJobPostingChildByPostingId(ctx, GuidProp(entry, nameof(JobPostingEssentialSkill.JobPostingId)))),
            [typeof(JobPostingPersonalTrait)] = Create(typeof(JobPostingPersonalTrait), nameof(JobPostingPersonalTrait), e => ((JobPostingPersonalTrait)e).Id.ToString(), (entry, ctx) => ResolveJobPostingChildByPostingId(ctx, GuidProp(entry, nameof(JobPostingPersonalTrait.JobPostingId))), e => ((JobPostingPersonalTrait)e).TraitText),
            [typeof(JobApplication)] = Create(typeof(JobApplication), nameof(JobApplication), e => ((JobApplication)e).JobApplicationId.ToString(), (entry, ctx) => ResolveJobPostingChildByPostingId(ctx, GuidProp(entry, nameof(JobApplication.JobPostingId)))),
            [typeof(ProductResumeApplication)] = Create(typeof(ProductResumeApplication), nameof(ProductResumeApplication), e => ((ProductResumeApplication)e).ProductResumeApplicationId.ToString(), ResolveProductResumeCenter),
            [typeof(UserProfile)] = Create(typeof(UserProfile), nameof(UserProfile), e => ((UserProfile)e).UserId.ToString(), (entry, ctx) => ResolveByUserId(ctx, GuidProp(entry, nameof(UserProfile.UserId))), e => ((UserProfile)e).JobTitle),
            [typeof(WorkExperience)] = Create(typeof(WorkExperience), nameof(WorkExperience), e => ((WorkExperience)e).WorkExperienceId.ToString(), (entry, ctx) => ResolveByUserId(ctx, GuidProp(entry, nameof(WorkExperience.UserId))), e =>
            {
                var w = (WorkExperience)e;
                return string.IsNullOrWhiteSpace(w.JobTitle) ? w.CompanyName : w.JobTitle;
            }),
            [typeof(EducationalBackground)] = Create(typeof(EducationalBackground), nameof(EducationalBackground), e => ((EducationalBackground)e).EducationId.ToString(), (entry, ctx) => ResolveByUserId(ctx, GuidProp(entry, nameof(EducationalBackground.UserId))), e => ((EducationalBackground)e).InstitutionName),
            [typeof(UserSkill)] = Create(typeof(UserSkill), nameof(UserSkill), e => ((UserSkill)e).UserSkillId.ToString(), (entry, ctx) => ResolveByUserId(ctx, GuidProp(entry, nameof(UserSkill.UserId)))),
            [typeof(UserLanguage)] = Create(typeof(UserLanguage), nameof(UserLanguage), e => ((UserLanguage)e).UserLanguageId.ToString(), (entry, ctx) => ResolveByUserId(ctx, GuidProp(entry, nameof(UserLanguage.UserId)))),
            [typeof(JobPreference)] = Create(typeof(JobPreference), nameof(JobPreference), e => ((JobPreference)e).UserId.ToString(), (entry, ctx) => ResolveByUserId(ctx, GuidProp(entry, nameof(JobPreference.UserId)))),
            [typeof(JobPreferenceProvince)] = Create(typeof(JobPreferenceProvince), nameof(JobPreferenceProvince), e => ((JobPreferenceProvince)e).Id.ToString(), (entry, ctx) => ResolveByUserId(ctx, GuidProp(entry, nameof(JobPreferenceProvince.UserId)))),
            [typeof(JobPreferenceJobCategory)] = Create(typeof(JobPreferenceJobCategory), nameof(JobPreferenceJobCategory), e => ((JobPreferenceJobCategory)e).Id.ToString(), (entry, ctx) => ResolveByUserId(ctx, GuidProp(entry, nameof(JobPreferenceJobCategory.UserId)))),
            [typeof(JobPreferenceSeniorityLevel)] = Create(typeof(JobPreferenceSeniorityLevel), nameof(JobPreferenceSeniorityLevel), e => ((JobPreferenceSeniorityLevel)e).Id.ToString(), (entry, ctx) => ResolveByUserId(ctx, GuidProp(entry, nameof(JobPreferenceSeniorityLevel.UserId)))),
            [typeof(JobPreferenceContractType)] = Create(typeof(JobPreferenceContractType), nameof(JobPreferenceContractType), e => ((JobPreferenceContractType)e).Id.ToString(), (entry, ctx) => ResolveByUserId(ctx, GuidProp(entry, nameof(JobPreferenceContractType.UserId)))),
            [typeof(Skill)] = Create(typeof(Skill), nameof(Skill), e => ((Skill)e).SkillId.ToString(), NoCenter, e => ((Skill)e).SkillName),
        };
    }

    public bool TryGetDescriptor(Type clrType, out AuditableEntityDescriptor descriptor)
    {
        var type = clrType;
        while (type is not null && type != typeof(object))
        {
            if (_byType.TryGetValue(type, out descriptor!))
                return true;
            type = type.BaseType;
        }

        descriptor = null!;
        return false;
    }

    public bool IsIgnoredProperty(AuditableEntityDescriptor descriptor, string propertyName)
        => descriptor.IgnoredProperties.Contains(propertyName);

    private static AuditableEntityDescriptor Create(
        Type clrType,
        string entityName,
        Func<object, string?> getKey,
        Func<EntityEntry, DbContext, Guid?> getCenter,
        Func<object, string?>? getTitle = null,
        IReadOnlySet<string>? ignored = null)
        => new()
        {
            ClrType = clrType,
            EntityName = entityName,
            GetRecordKey = getKey,
            GetCenterProfileId = getCenter,
            GetRecordTitle = getTitle,
            IgnoredProperties = ignored ?? DefaultIgnored,
        };

    private static Guid? NoCenter(EntityEntry _, DbContext __) => null;

    private static Guid? GuidProp(EntityEntry entry, string propertyName)
    {
        var prop = entry.Property(propertyName);
        if (prop.CurrentValue is Guid current && current != Guid.Empty)
            return current;
        if (prop.OriginalValue is Guid original && original != Guid.Empty)
            return original;
        return null;
    }

    private static int? IntProp(EntityEntry entry, string propertyName)
    {
        var prop = entry.Property(propertyName);
        if (prop.CurrentValue is int current)
            return current;
        if (prop.OriginalValue is int original)
            return original;
        return null;
    }

    private static long? LongProp(EntityEntry entry, string propertyName)
    {
        var prop = entry.Property(propertyName);
        if (prop.CurrentValue is long current)
            return current;
        if (prop.OriginalValue is long original)
            return original;
        return null;
    }

    private static Guid? ResolveByLabCode(EntityEntry entry, DbContext context, string propertyName)
    {
        var labCode = IntProp(entry, propertyName);
        if (labCode is null)
            return null;

        return context.Set<CenterProfile>()
            .AsNoTracking()
            .Where(c => c.LabCodeNew == labCode.Value)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefault();
    }

    private static Guid? ResolveByUserId(DbContext context, Guid? userId)
    {
        if (userId is null || userId == Guid.Empty)
            return null;

        return context.Set<User>()
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.CenterProfileId)
            .FirstOrDefault();
    }

    private static Guid? ResolveSpecialOfferTestCenter(EntityEntry entry, DbContext context)
    {
        var offerId = LongProp(entry, nameof(SpecialOfferTest.SpecialOfferId));
        if (offerId is null)
            return null;

        var labCode = context.Set<SpecialOffer>()
            .AsNoTracking()
            .Where(o => o.Id == offerId)
            .Select(o => (int?)o.LabCodeNew)
            .FirstOrDefault();
        if (labCode is null)
            return null;

        return context.Set<CenterProfile>()
            .AsNoTracking()
            .Where(c => c.LabCodeNew == labCode.Value)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefault();
    }

    private static Guid? ResolveLabAgreementAttachmentCenter(EntityEntry entry, DbContext context)
        => ResolveLabAgreementChildCenter(LongProp(entry, nameof(LabAgreementAttachment.LabAgreementId)), context);

    private static Guid? ResolveLabAgreementTestPriceCenter(EntityEntry entry, DbContext context)
        => ResolveLabAgreementChildCenter(LongProp(entry, nameof(LabAgreementTestPrice.LabAgreementId)), context);

    private static Guid? ResolveLabAgreementChildCenter(long? agreementId, DbContext context)
    {
        if (agreementId is null)
            return null;

        var labCode = context.Set<LabAgreement>()
            .AsNoTracking()
            .Where(a => a.Id == agreementId)
            .Select(a => (int?)a.PrimaryAgreementLabCodeNew)
            .FirstOrDefault();
        if (labCode is null)
            return null;

        return context.Set<CenterProfile>()
            .AsNoTracking()
            .Where(c => c.LabCodeNew == labCode.Value)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefault();
    }

    private static Guid? ResolveOrderItemCenter(EntityEntry entry, DbContext context)
    {
        var orderId = GuidProp(entry, nameof(ProductOrderItem.OrderId));
        if (orderId is null)
            return null;

        return context.Set<ProductOrder>()
            .AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => (Guid?)o.CenterProfileId)
            .FirstOrDefault();
    }

    private static Guid? ResolveLabUserPermissionCenter(EntityEntry entry, DbContext context)
        => ResolveByUserId(context, GuidProp(entry, nameof(LabUserPermission.UserId)));

    private static Guid? ResolveOrgLocationCenter(EntityEntry entry, DbContext context)
        => ResolveByUserId(context, GuidProp(entry, nameof(OrganizationLocation.UserId)));

    private static Guid? ResolveJobPostingCenter(EntityEntry entry, DbContext context)
        => ResolveByUserId(context, GuidProp(entry, nameof(JobPostingRequest.UserId)));

    private static Guid? ResolveJobPostingChildByPostingId(DbContext context, Guid? postingId)
    {
        if (postingId is null || postingId == Guid.Empty)
            return null;

        var userId = context.Set<JobPostingRequest>()
            .AsNoTracking()
            .Where(j => j.JobPostingId == postingId)
            .Select(j => (Guid?)j.UserId)
            .FirstOrDefault();
        return ResolveByUserId(context, userId);
    }

    private static Guid? ResolveProductResumeCenter(EntityEntry entry, DbContext context)
    {
        var productId = GuidProp(entry, nameof(ProductResumeApplication.ProductId));
        if (productId is null)
            return null;

        return context.Set<Product>()
            .AsNoTracking()
            .Where(p => p.ProductId == productId)
            .Select(p => p.CreatedByCenterProfileId)
            .FirstOrDefault();
    }
}
