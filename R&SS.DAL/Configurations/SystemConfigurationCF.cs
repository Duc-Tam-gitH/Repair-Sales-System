using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class SystemConfigurationCF : IEntityTypeConfiguration<SystemConfiguration>
    {
        public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
        {
            builder.ToTable("SystemConfigurations");
            builder.HasKey(x => x.SystemConfigurationId);
            builder.Property(x => x.ConfigurationKey).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ConfigurationValue).HasMaxLength(255).IsRequired();
            builder.Property(x => x.GroupName).HasMaxLength(100);
            builder.HasIndex(x => x.ConfigurationKey).IsUnique();
        }
    }
}
