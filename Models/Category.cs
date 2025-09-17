using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // This line is crucial for validation
using System.Text.Json.Serialization; // Required for IgnoreCycles

namespace Inventory_Tracker.Models;

public partial class Category
{
    // Add these two lines
    public string UserId { get; set; }
    public virtual ApplicationUser User { get; set; }
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, ErrorMessage = "Category name cannot be longer than 100 characters.")]
    public string Name { get; set; } = null!;


    [JsonIgnore]
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
    
    public string? TestColumn123 { get; set; }
}
