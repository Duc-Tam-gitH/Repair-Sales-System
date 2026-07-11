using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class NotificationTemplate
    {
        [Key]
        public int NotificationTemplateId { get; set; }

        [Required, MaxLength(50)]
        public string TemplateCode { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string TemplateType { get; set; } = "Email";

        [Required, MaxLength(150)]
        public string TemplateName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Subject { get; set; }

        [Required, MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        [Required, MaxLength(4000)]
        public string DefaultSubject { get; set; } = string.Empty;

        [Required, MaxLength(4000)]
        public string DefaultContent { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string AllowedVariables { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        public virtual ICollection<NotificationTemplateHistory> Histories { get; set; } = new List<NotificationTemplateHistory>();
    }
}
