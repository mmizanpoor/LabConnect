using Microsoft.EntityFrameworkCore;
using LabConnectPortal.Domain;
using LabConnectPortal.Infra.Configurations;

namespace LabConnectPortal.Infra.Context
{
    public class SamanehDbContext : DbContext
    {
        public SamanehDbContext(DbContextOptions<SamanehDbContext> options) : base(options)
        {
        }

        public DbSet<LabAgreement> LabAgreements { get; set; }
        public DbSet<LabAgreementAttachment> LabAgreementAttachments { get; set; }
        public DbSet<LabAgreementTestPrice> LabAgreementTestPrices { get; set; }

        public DbSet<ReceptionNew> ReceptionNew { get; set; }
        public DbSet<ReceptTestNew> ReceptTestNew { get; set; }
        public DbSet<ReceptTestP> ReceptTestP { get; set; }
        public DbSet<ReceptTestS> ReceptTestS { get; set; }
        public DbSet<LabReceiverRangeDetail> LabReceiverRangeDetail { get; set; }
        public DbSet<SRLabName> SRLabName { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.LogTo(Console.WriteLine).EnableDetailedErrors();
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.ApplyConfiguration(new ReceptionConfiguration());
            modelBuilder.ApplyConfiguration(new ReceptTestConfiguration());
            modelBuilder.ApplyConfiguration(new ReceptTestPConfiguration());
            modelBuilder.ApplyConfiguration(new ReceptTestSConfiguration());
            modelBuilder.ApplyConfiguration(new LabReceiverRangeDetailConfiguration());
        }
    }
}
