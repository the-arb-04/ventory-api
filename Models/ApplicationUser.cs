// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }
    public string ShopName { get; set; }
    public string Location { get; set; }
    public string? OwnerId { get; set; } 
}
