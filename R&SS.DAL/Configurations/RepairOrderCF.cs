using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class RepairOrderCF : IEntityTypeConfiguration<RepairOrder>
    {
        public void Configure(EntityTypeBuilder<RepairOrder> builder)
        {
            builder.ToTable("RepairOrders");

            builder.HasKey(x => x.RepairOrderId);

            builder.Property(x => x.RepairOrderCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.DeviceName)
                .HasMaxLength(150);

            builder.Property(x => x.DeviceType)
                .HasMaxLength(100);

            builder.Property(x => x.Brand)
                .HasMaxLength(100);

            builder.Property(x => x.DeviceModel)
                .HasMaxLength(150);

            builder.Property(x => x.SerialNumber)
                .HasMaxLength(150);

            builder.Property(x => x.IssueDescription)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.RequestType)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.DeviceCondition)
                .HasMaxLength(255);

            builder.Property(x => x.Diagnosis)
                .HasMaxLength(255);

            builder.Property(x => x.InspectionResult)
                .HasMaxLength(255);

            builder.Property(x => x.WorkPerformed)
                .HasMaxLength(255);

            builder.Property(x => x.RepairResult)
                .HasMaxLength(255);

            builder.Property(x => x.AccompanyingAccessories)
                .HasMaxLength(255);

            builder.Property(x => x.ServiceFee)
                .HasPrecision(18, 2);

            builder.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.Notes)
                .HasMaxLength(255);

            builder.Property(x => x.ReceivedDate)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.HasIndex(x => x.RepairOrderCode).IsUnique();

            builder.HasOne(x => x.Customer)
                .WithMany(x => x.RepairOrders)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReceivedByUser)
                .WithMany()
                .HasForeignKey(x => x.ReceivedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.AssignedTechnician)
                .WithMany()
                .HasForeignKey(x => x.AssignedTechnicianId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
