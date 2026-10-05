using System.Text;
using LabConnectPortal.Api.Infrastructure.HostedServices;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Audit;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Auth.Email;
using LabConnectPortal.Api.Infrastructure.Auth.Sms;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Repositories.Implementations;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Implementations;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Storage;
using LabConnectPortal.Api.Infrastructure.ViewModels.Notification;
using LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;
using LabConnectPortal.Api.Infrastructure.ViewModels.Site;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using LabConnectPortal.Api.Infrastructure.Factories.PaymentFactory;

namespace LabConnectPortal.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLabConnectPortal(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));
        services.AddSingleton(EmailSettings.CreateHardcoded());
        services.Configure<SepidRadisanSettings>(configuration.GetSection("SepidRadisan"));
        services.Configure<ExternalNotificationSettings>(configuration.GetSection("ExternalNotification"));
        services.Configure<PortalSettings>(configuration.GetSection("Portal"));
        services.Configure<AgreementExpiryNotificationSettings>(configuration.GetSection("AgreementExpiryNotifications"));

        services.AddSingleton<IAuditableEntityRegistry, AuditableEntityRegistry>();
        services.AddScoped<ActivityLogInterceptor>();
        services.AddScoped<ISaveChangesInterceptor>(sp => sp.GetRequiredService<ActivityLogInterceptor>());
        services.AddDbContext<LabConnectDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("LabConnectConnection"), o => o.UseCompatibilityLevel(120));
            foreach (var interceptor in sp.GetServices<ISaveChangesInterceptor>())
                options.AddInterceptors(interceptor);
        });

        services.AddDbContext<SamanehDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("SamanehConnection"), o => o.UseCompatibilityLevel(120));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddDbContext<PtnServiceDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("sql2008Connection"), o => o.UseCompatibilityLevel(120));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddScoped(typeof(IRepository<>), typeof(LabConnectRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILabUserPermissionRepository, LabUserPermissionRepository>();
        services.AddScoped<ISiteUserPermissionRepository, SiteUserPermissionRepository>();
        services.AddScoped<ICenterProfileRepository, CenterProfileRepository>();
        services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddHttpClient<INotificationService, NotificationService>((_, client) =>
            {
                var baseUrl = configuration["ExternalNotification:BaseUrl"] ?? "https://localhost:7173/";
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            });
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IOtpCodeRepository, OtpCodeRepository>();
        services.AddScoped<ILabAgreementRepository, LabAgreementRepository>();
        services.AddScoped<IReceptionRepository, ReceptionRepository>();
        services.AddScoped<ISRLabRepository, SRLabRepository>();
        services.AddScoped<ITestECLRepository, TestECLRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();

        services.AddScoped<IPaymentFactory, PaymentFactory>();

        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<ILoginAttemptService, LoginAttemptService>();
        services.AddScoped<ISecuritySmsReportService, SecuritySmsReportService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ILabUserService, LabUserService>();
        services.AddScoped<ILabUserPermissionService, LabUserPermissionService>();
        services.AddScoped<ISiteUserService, SiteUserService>();
        services.AddScoped<ISiteUserPermissionService, SiteUserPermissionService>();
        services.AddScoped<ICenterProfileService, CenterProfileService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<ILaboratoryLookupService, LaboratoryLookupService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IOrganizationLocationService, OrganizationLocationService>();
        services.AddScoped<IJobPostingService, JobPostingService>();
        services.AddScoped<IPublicJobService, PublicJobService>();
        services.AddScoped<IJobApplicationService, JobApplicationService>();
        services.AddScoped<IProductResumeApplicationService, ProductResumeApplicationService>();
        services.AddScoped<IProductCatalogService, ProductCatalogService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IProductOrderService, ProductOrderService>();
        services.AddScoped<IProductReviewService, ProductReviewService>();
        services.AddScoped<IPublicProductService, PublicProductService>();
        services.AddScoped<ISiteAnalyticsService, SiteAnalyticsService>();
        services.AddScoped<ISiteSettingsService, SiteSettingsService>();
        services.AddScoped<ISiteServiceService, SiteServiceService>();
        services.AddScoped<IPublicSiteServiceService, SiteServiceService>();
        services.AddScoped<ISliderGroupService, SliderGroupService>();
        services.AddScoped<IContentService, ContentService>();
        services.AddScoped<IPublicContentService, PublicContentService>();
        services.AddScoped<ICompanyRegulationService, CompanyRegulationService>();
        services.AddScoped<IPublicCompanyRegulationService, PublicCompanyRegulationService>();
        services.AddScoped<IAdvertisementService, AdvertisementService>();
        services.AddScoped<IPublicAdvertisementService, PublicAdvertisementService>();
        services.AddScoped<IAdvertisementPositionService, AdvertisementPositionService>();
        services.AddScoped<IAdvertisementDurationService, AdvertisementDurationService>();
        services.AddScoped<IAdvertisementPriceService, AdvertisementPriceService>();
        services.AddScoped<IAdvertisementOrderService, AdvertisementOrderService>();
        services.AddScoped<IProductCategoryPriceService, ProductCategoryPriceService>();
        services.AddScoped<ISiteChargeServicePricingService, SiteChargeServicePricingService>();
        services.AddScoped<ISiteChargeServiceAdminService, SiteChargeServiceAdminService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddSingleton<IImageThumbnailService, ImageThumbnailService>();
        services.AddScoped<ILabAgreementService, LabAgreementService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<ILabAgreementSettingsService, LabAgreementSettingsService>();
        services.AddScoped<IReceptionService, ReceptionService>();
        services.AddScoped<ITestECLService, TestECLService>();
        services.AddScoped<IDeviceGroupService, DeviceGroupService>();
        services.AddScoped<IKitGroupService, KitGroupService>();
        services.AddScoped<ITestInfoService, TestInfoService>();
        services.AddScoped<ISpecialOfferService, SpecialOfferService>();
        services.AddScoped<IPublicSpecialOfferService, PublicSpecialOfferService>();
        services.AddScoped<ILabAgreementPortalService, LabAgreementPortalService>();
        services.AddScoped<IAgreementExpiryNotificationService, AgreementExpiryNotificationService>();
        services.AddHostedService<AgreementExpiryNotificationHostedService>();
        services.AddScoped<ISepidRadisanRepository, SepidRadisanRepository>();
        services.AddScoped<IBehPardakhtService, BehPardakhtService>();

        services.AddHttpClient<IRasaService, RasaService>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            });

        services.AddHttpClient<ISepidRadisanService, SepidRadisanService>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            });

        services.AddSingleton<JwtTokenService>();
        services.AddScoped<OtpService>();
        services.AddScoped<ISmsSender, SmsSender>();
        services.AddScoped<IEmailSender, EmailSender>();

        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    NameClaimType = System.Security.Claims.ClaimTypes.Name,
                };
            });

        services.AddSingleton<IAuthorizationHandler, UserTypeAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, UserTypeAuthorizationPolicyProvider>();
        services.AddAuthorization();
        services.AddCors(policyBuilder => policyBuilder.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod()));
        //services.AddCors(policyBuilder => policyBuilder.AddDefaultPolicy(policy =>
        //    policy.WithOrigins("http://localhost:4200", "http://localhost:4201", "http://localhost:4500","*")
        //        .AllowAnyHeader()
        //        .AllowAnyMethod()
        //        .AllowCredentials()));

        services.AddHttpContextAccessor();
        return services;
    }
}
