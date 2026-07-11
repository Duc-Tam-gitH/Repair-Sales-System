using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class RolePermissionCF : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermissions");
            builder.HasKey(x => x.RolePermissionId);
            builder.Property(x => x.UseCaseId).HasMaxLength(20).IsRequired();
            builder.Property(x => x.FunctionName).HasMaxLength(150).IsRequired();
            builder.HasIndex(x => new { x.RoleId, x.UseCaseId }).IsUnique();
        }
    }
}
