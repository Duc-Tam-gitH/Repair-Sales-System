using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class ServiceRequestCF : IEntityTypeConfiguration<ServiceRequest>
    {
        public void Configure(EntityTypeBuilder<ServiceRequest> builder)
        {
            builder.ToTable("ServiceRequests");
            builder.HasKey(x => x.ServiceRequestId);
            builder.Property(x => x.RequestCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ServiceType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.DeviceType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Brand).HasMaxLength(100).IsRequired();
            builder.Property(x => x.DeviceModel).HasMaxLength(150);
            builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ImageUrls).HasMaxLength(1000);
            builder.Property(x => x.CancelReason).HasMaxLength(255);
            builder.HasIndex(x => x.RequestCode).IsUnique();
        }
    }
}
