using LabConnectPortal.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Context;

public class PtnServiceDbContext : DbContext
{
    public PtnServiceDbContext(DbContextOptions<PtnServiceDbContext> options) : base(options)
    {
    }

    public DbSet<TestECL> TestECL => Set<TestECL>();
    public DbSet<TestJoze3> TestJoze3 => Set<TestJoze3>();
    public DbSet<SmsAccount> SmsAccounts => Set<SmsAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("dbo");

        modelBuilder.Entity<TestECL>().HasNoKey().ToTable("TestsNew99");
        modelBuilder.Entity<TestJoze3>().HasNoKey().ToTable("TestsJoze3");
        modelBuilder.Entity<SmsAccount>().ToTable("SmsAccount");
    }
}
