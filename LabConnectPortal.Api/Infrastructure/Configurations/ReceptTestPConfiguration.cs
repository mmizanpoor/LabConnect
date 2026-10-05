using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LabConnectPortal.Api.Domain.Entities;

namespace LabConnectPortal.Api.Infrastructure.Configurations
{
    public class ReceptTestPConfiguration : IEntityTypeConfiguration<ReceptTestP>
    {
        public void Configure(EntityTypeBuilder<ReceptTestP> builder)
        {
            builder.HasOne(typeof(ReceptionNew), "Reception")
               .WithMany("ReceptTestPs")
               .HasForeignKey("intSourceLabId", "chrSourceReceptId", "intTargetLabId");
        }
    }
}

