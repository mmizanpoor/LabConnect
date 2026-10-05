using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LabConnectPortal.Api.Domain.Entities;

namespace LabConnectPortal.Api.Infrastructure.Configurations
{
    public class ReceptionConfiguration : IEntityTypeConfiguration<ReceptionNew>
    {
        public void Configure(EntityTypeBuilder<ReceptionNew> builder)
        {
            builder.HasKey(r => new { r.intSourceLabId, r.chrSourceReceptId, r.intTargetLabId });
        }
    }
}

