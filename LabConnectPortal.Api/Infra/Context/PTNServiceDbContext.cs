using Microsoft.EntityFrameworkCore;
using LabConnectPortal.Domain;

namespace LabConnectPortal.Infra.Context
{
    public class PTNServiceDbContext : DbContext
    {
        public PTNServiceDbContext(DbContextOptions<PTNServiceDbContext> options) : base(options)
        {     
        }

        public DbSet<TestECL> TestECL { get; set; }
        public DbSet<TestJoze3> TestJoze3 { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("dbo");
        }
    }
}
