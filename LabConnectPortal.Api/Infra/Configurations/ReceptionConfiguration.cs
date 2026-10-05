using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LabConnectPortal.Domain;

namespace LabConnectPortal.Infra.Configurations
{
    public class ReceptionConfiguration : IEntityTypeConfiguration<ReceptionNew>
    {
        public void Configure(EntityTypeBuilder<ReceptionNew> builder)
        {
            builder.HasKey(r => new { r.intSourceLabId, r.chrSourceReceptId, r.intTargetLabId });
        }
    }
}
