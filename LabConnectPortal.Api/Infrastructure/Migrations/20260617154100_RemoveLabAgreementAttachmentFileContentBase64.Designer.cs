using LabConnectPortal.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConnectPortal.Api.Infrastructure.Migrations
{
    [DbContext(typeof(LabConnectDbContext))]
    [Migration("20260617154100_RemoveLabAgreementAttachmentFileContentBase64")]
    partial class RemoveLabAgreementAttachmentFileContentBase64
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.16");

            modelBuilder.HasDefaultSchema("dbo");
#pragma warning restore 612, 618
        }
    }
}
