using System.ComponentModel.DataAnnotations;

namespace Inventory_Tracker.DTOs
{
    public class CreateItemDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public decimal SellingPrice { get; set; }
        public decimal CostPrice { get; set; }
        public int Quantity { get; set; }
        public int MinQuantity { get; set; }
        public decimal TaxPercentage { get; set; } // Add this
        public bool IsActive { get; set; }

        // Optional fields
        public string? Sku { get; set; }
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public string? Brand { get; set; }
        public int? CategoryId { get; set; }
        public int? SupplierId { get; set; }
    }
}