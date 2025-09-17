// In a new file, perhaps in a DTOs folder.
using System.ComponentModel.DataAnnotations;

namespace Inventory_Tracker.DTOs
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name cannot be longer than 100 characters.")]
        public string Name { get; set; }
    }
}