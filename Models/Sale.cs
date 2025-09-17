// In Models/Sale.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Tracker.Models;

public class Sale
{
    // Add these two lines
    public string UserId { get; set; }
    public virtual ApplicationUser User { get; set; }
    [Key]
    public int SaleId { get; set; }

    [Required]
    public int ItemId { get; set; }

    [Required]
    public int QuantitySold { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal SalePriceAtTimeOfSale { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal CostPriceAtTimeOfSale { get; set; }

    [Required]
    public DateTime SaleTimestamp { get; set; } = DateTime.UtcNow;

    // Navigation property to the Item
    public virtual Item? Item { get; set; }
}