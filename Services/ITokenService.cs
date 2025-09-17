// In Services/ITokenService.cs
using Inventory_Tracker.Models; // <-- Add this using statement
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic; // <-- Add this using statement

namespace Inventory_Tracker.Services
{
    public interface ITokenService
    {
        // Change IdentityUser to ApplicationUser
        string CreateToken(ApplicationUser user, IList<string> roles);
    }
}