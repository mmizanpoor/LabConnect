using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LabConnectPortal.Api.Domain.Entities;

namespace LabConnectPortal.Api.Infrastructure.Configurations
{
    public class ReceptTestConfiguration : IEntityTypeConfiguration<ReceptTestNew>
    {
        public void Configure(EntityTypeBuilder<ReceptTestNew> builder)
        {

            builder.HasOne(typeof(ReceptionNew), "Reception")
               .WithMany("ReceptTests")
               .HasForeignKey("intSourceLabId", "chrSourceReceptId", "intTargetLabId");
        }
    }
}

