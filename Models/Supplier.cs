namespace Inventory_Tracker.Models
{
    public class Supplier
    {
        // Add these two lines
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Website { get; set; }
        public string? Email { get; set; }
        public string? ContactPerson { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<Item> Items { get; set; }

    }
}