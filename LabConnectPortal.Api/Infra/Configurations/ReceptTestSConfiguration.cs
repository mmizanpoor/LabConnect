using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LabConnectPortal.Domain;

namespace LabConnectPortal.Infra.Configurations
{
    public class ReceptTestSConfiguration : IEntityTypeConfiguration<ReceptTestS>
    {
        public void Configure(EntityTypeBuilder<ReceptTestS> builder)
        {

            builder.HasOne(typeof(ReceptionNew), "Reception")
               .WithMany("ReceptTestSs")
               .HasForeignKey("intSourceLabId", "chrSourceReceptId", "intTargetLabId");
        }
    }
}
