using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class NotificationTemplateHistory
    {
        [Key]
        public int NotificationTemplateHistoryId { get; set; }

        public int NotificationTemplateId { get; set; }
        public int EditedByUserId { get; set; }

        [MaxLength(255)]
        public string? PreviousSubject { get; set; }

        [Required, MaxLength(4000)]
        public string PreviousContent { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? NewSubject { get; set; }

        [Required, MaxLength(4000)]
        public string NewContent { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string Action { get; set; } = "Update";

        public DateTime EditedAtUtc { get; set; } = DateTime.UtcNow;

        public virtual NotificationTemplate? NotificationTemplate { get; set; }
    }
}
