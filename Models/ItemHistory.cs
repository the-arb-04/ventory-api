using System;

namespace Inventory_Tracker.Models
{
    public partial class ItemHistory
    {
        // Add these two lines
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string TransactionType { get; set; } = null!;
        public int QuantityChanged { get; set; }
        public string? Remarks { get; set; }
        public DateTime Timestamp { get; set; }
        public string? UserName { get; set; }

        public virtual Item Item { get; set; } = null!;
    }
}
