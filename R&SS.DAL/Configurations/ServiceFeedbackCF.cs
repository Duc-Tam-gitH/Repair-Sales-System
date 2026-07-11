using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class ServiceFeedbackCF : IEntityTypeConfiguration<ServiceFeedback>
    {
        public void Configure(EntityTypeBuilder<ServiceFeedback> builder)
        {
            builder.ToTable("ServiceFeedbacks");
            builder.HasKey(feedback => feedback.ServiceFeedbackId);
            builder.Property(feedback => feedback.Comment).HasMaxLength(1000);
            builder.HasIndex(feedback => feedback.SalesOrderId).IsUnique().HasFilter("[SalesOrderId] IS NOT NULL");
            builder.HasIndex(feedback => feedback.RepairOrderId).IsUnique().HasFilter("[RepairOrderId] IS NOT NULL");
        }
    }
}
