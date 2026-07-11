using System.ComponentModel.DataAnnotations;

namespace R_SS.Models.Entities
{
    public class TechnicianAssignmentHistory
    {
        [Key]
        public int TechnicianAssignmentHistoryId { get; set; }
        public int RepairOrderId { get; set; }
        public int AssignedByUserId { get; set; }
        public int? PreviousTechnicianId { get; set; }
        public int AssignedTechnicianId { get; set; }
        public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
        [MaxLength(255)]
        public string? Notes { get; set; }
        public virtual RepairOrder? RepairOrder { get; set; }
    }
}
