using Inventory_Tracker.Models; // <-- Add this
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Inventory_Tracker.Services
{
    public static class AdminSeeder
    {
        private const string AdminEmail = "admin@inventory.com";
        private const string AdminPassword = "Admin@123";
        private const string AdminRole = "Admin";
        private const string UserRole = "Employee";

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(); // <-- Changed

            // --- Ensure roles exist ---
            if (!await roleManager.RoleExistsAsync(AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(AdminRole));
            }

            if (!await roleManager.RoleExistsAsync(UserRole))
            {
                await roleManager.CreateAsync(new IdentityRole(UserRole));
            }

            // --- Ensure admin user exists ---
            var adminUser = await userManager.FindByEmailAsync(AdminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = AdminEmail,
                    Email = AdminEmail,
                    EmailConfirmed = true,
                    Location = "HQ" ,// or any default location
                    Name = "Default Admin", // <-- ADD THIS
                    ShopName = "Default Shop"
                };

                var createResult = await userManager.CreateAsync(adminUser, AdminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, AdminRole);
                }
                else
                {
                    throw new Exception("Failed to create admin user: " +
                        string.Join(", ", createResult.Errors));
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
                {
                    await userManager.AddToRoleAsync(adminUser, AdminRole);
                }
            }
        }
    }
}
