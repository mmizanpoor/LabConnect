using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Context;

public class SamanehDbContext : DbContext
{
    public SamanehDbContext(DbContextOptions<SamanehDbContext> options) : base(options)
    {
    }

    public DbSet<ReceptionNew> ReceptionNew => Set<ReceptionNew>();
    public DbSet<ReceptTestNew> ReceptTestNew => Set<ReceptTestNew>();
    public DbSet<ReceptTestP> ReceptTestP => Set<ReceptTestP>();
    public DbSet<ReceptTestS> ReceptTestS => Set<ReceptTestS>();
    public DbSet<LabReceiverRangeDetail> LabReceiverRangeDetail => Set<LabReceiverRangeDetail>();
    public DbSet<SRLabName> SRLabName => Set<SRLabName>();
    public DbSet<Security> Securities => Set<Security>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("dbo");

        modelBuilder.ApplyConfiguration(new ReceptionConfiguration());
        modelBuilder.ApplyConfiguration(new ReceptTestConfiguration());
        modelBuilder.ApplyConfiguration(new ReceptTestPConfiguration());
        modelBuilder.ApplyConfiguration(new ReceptTestSConfiguration());
        modelBuilder.ApplyConfiguration(new LabReceiverRangeDetailConfiguration());

        modelBuilder.Entity<SRLabName>().ToTable("SR-LabName");
        modelBuilder.Entity<Security>().ToTable("Security");
    }
}
