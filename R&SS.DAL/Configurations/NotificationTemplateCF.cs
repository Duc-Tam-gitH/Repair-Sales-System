using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class NotificationTemplateCF : IEntityTypeConfiguration<NotificationTemplate>
    {
        public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
        {
            builder.ToTable("NotificationTemplates");
            builder.HasKey(x => x.NotificationTemplateId);
            builder.Property(x => x.TemplateCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.TemplateType).HasMaxLength(20).IsRequired();
            builder.Property(x => x.TemplateName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Subject).HasMaxLength(255);
            builder.Property(x => x.Content).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.DefaultSubject).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.DefaultContent).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.AllowedVariables).HasMaxLength(1000).IsRequired();
            builder.HasIndex(x => x.TemplateCode).IsUnique();
        }
    }
}
