// In Models/AiSummary.cs
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Tracker.Models
{
    [Table("aisummaries")] // This tells EF the exact table name
    public class AiSummary
    {
        public int Id { get; set; }

        [Column("summary_text")]
        public string SummaryText { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Add the ownership properties
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

    
}