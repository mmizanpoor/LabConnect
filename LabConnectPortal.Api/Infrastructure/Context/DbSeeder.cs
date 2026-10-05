using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Context;

public static class DbSeeder
{
    public const string AdminMobile = "09037126316";
    public const string LabAdminMobile = "02166596699";
    public const int SampleLabCode = 21311;

    /// <summary>
    /// Data is now entered by hand and there is no backup, so seeders must never run:
    /// the catalog seeder in particular deletes every product when it detects a mismatch.
    /// Only enable this against a throwaway database.
    /// </summary>
    private static readonly bool EnableDataSeeding = false;

    public static async Task SeedAsync(LabConnectDbContext context, string contentRootPath)
    {
        await context.Database.MigrateAsync();

        if (!EnableDataSeeding)
            return;

        await ResumeReferenceDataSeeder.SeedAsync(context);
        await ProductCatalogReferenceDataSeeder.EnsureSeededAsync(context, contentRootPath);

        // One-time company listings seed. Already applied; keep commented so it never runs again.
        // await ProductCompanyListingSeeder.SeedOnceAsync(context, contentRootPath);

        if (await context.Users.AnyAsync())
            return;

        var adminId = Guid.NewGuid();
        var admin = new User
        {
            Id = adminId,
            UserType = UserType.Administrator,
            FirstName = "مدیر",
            LastName = "سیستم",
            Username = "admin",
            PasswordHash = PasswordHasher.Hash("Admin@123"),
            MobileNumber = AdminMobile,
            Email = "admin@labconnect.local",
            EmailConfirmed = true,
            MobileConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.Now,
        };

        var labProfileId = Guid.NewGuid();
        var labProfile = new CenterProfile
        {
            Id = labProfileId,
            CenterType = CenterType.Lab,
            Status = CenterProfileStatus.Active,
            LabCode = SampleLabCode,
            LabCodeNew = SampleLabCode,
            Name = "پیوند طب و نرم افزار",
        };

        var labAdminId = Guid.NewGuid();
        var labAdmin = new User
        {
            Id = labAdminId,
            UserType = UserType.AdminLab,
            CenterProfileId = labProfileId,
            FirstName = "پیوند طب و نرم افزار",
            LastName = "",
            Username = $"{LabAdminMobile}_{(int)UserType.AdminLab}",
            PasswordHash = string.Empty,
            MobileNumber = LabAdminMobile,
            MobileConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        //context.CenterProfiles.Add(labProfile);
        //context.Users.Add(admin);
        //context.Users.Add(labAdmin);
        //await context.SaveChangesAsync();
    }
}
