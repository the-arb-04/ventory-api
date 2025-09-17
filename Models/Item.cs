using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Tracker.Models
{
    public partial class Item
    {
        // Add these two lines
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public string? Brand { get; set; }
        public int? SupplierId { get; set; } // The foreign key
        public virtual Supplier? Supplier { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal CostPrice { get; set; }
        public decimal TaxPercentage { get; set; }
        public int Quantity { get; set; }
        public int MinQuantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public int? CategoryId { get; set; }

        public virtual Category? Category { get; set; }

        // **Important for history**
        public virtual ICollection<ItemHistory> ItemHistories { get; set; } = new List<ItemHistory>();
    }
}
