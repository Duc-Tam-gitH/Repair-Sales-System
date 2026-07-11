using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R_SS.Models.Entities;

namespace R_SS.DAL.Configurations
{
    public class SystemActivityLogCF : IEntityTypeConfiguration<SystemActivityLog>
    {
        public void Configure(EntityTypeBuilder<SystemActivityLog> builder)
        {
            builder.ToTable("SystemActivityLogs");
            builder.HasKey(log => log.SystemActivityLogId);
            builder.Property(log => log.ActorUsername).HasMaxLength(100);
            builder.Property(log => log.FunctionName).HasMaxLength(100).IsRequired();
            builder.Property(log => log.OperationType).HasMaxLength(50).IsRequired();
            builder.Property(log => log.AffectedData).HasMaxLength(255);
            builder.Property(log => log.ExecutionResult).HasMaxLength(50);
            builder.Property(log => log.IpAddress).HasMaxLength(50);
            builder.Property(log => log.Details).HasMaxLength(1000);
            builder.HasIndex(log => log.ExecutedAtUtc);
        }
    }
}
