using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LabConnectPortal.Domain;

namespace LabConnectPortal.Infra.Configurations
{
    public class LabReceiverRangeDetailConfiguration : IEntityTypeConfiguration<LabReceiverRangeDetail>
    {
        public void Configure(EntityTypeBuilder<LabReceiverRangeDetail> builder)
        {
            builder.HasMany(rt => rt.ReceptTestNews)
                       .WithOne(r => r.LabReceiverRangeDetail)
                       .HasForeignKey(l => l.LabReceiverRangeDetailId);
        }
    }
}
