// DTOs/AuthDtos.cs
using System.ComponentModel.DataAnnotations;

namespace Inventory_Tracker.DTOs
{
    public record RegisterDto(
        [Required] string Name,
        [Required] string Email,
        [Required] string Password,
        [Required] string Location,     // ✅ Add this line
        [Required] string ShopName      // ✅ You can add this too if needed
    );

    public record LoginDto(
        [Required] string Email,
        [Required] string Password
    );
}
