using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class SystemConfiguration
    {
        [Key]
        public int SystemConfigurationId { get; set; }
        [Required, MaxLength(100)]
        public string ConfigurationKey { get; set; } = string.Empty;
        [Required, MaxLength(255)]
        public string ConfigurationValue { get; set; } = string.Empty;
        [MaxLength(100)]
        public string GroupName { get; set; } = "General";
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public int? UpdatedByUserId { get; set; }
    }
}
