using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Inventory_Tracker.Models;
using Inventory_Tracker.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Security.Claims; // Required for getting user claims
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Tracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // <-- Secures the entire controller
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("add-employee")]
        public async Task<IActionResult> AddEmployee([FromBody] AddEmployeeDto model)
        {
            var adminUser = await _userManager.GetUserAsync(User);
            if (adminUser == null)
            {
                return Unauthorized();
            }

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                return BadRequest("An account with this email already exists.");
            }

            var employee = new ApplicationUser
            {
                Name = model.Name,
                UserName = model.Email,
                Email = model.Email,
                // Inherit these details from the Admin who is creating the account
                ShopName = adminUser.ShopName,
                Location = adminUser.Location,
                OwnerId = adminUser.Id
            };

            var result = await _userManager.CreateAsync(employee, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(employee, "Employee");

            return Ok(new { message = "Employee account created successfully!" });
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            // 1. Get the currently logged-in admin
            var adminUser = await _userManager.GetUserAsync(User);
            if (adminUser == null)
            {
                return Unauthorized();
            }

            // 2. Find all users whose OwnerId matches the admin's Id
            var employees = await _userManager.Users
                .Where(user => user.OwnerId == adminUser.Id)
                .ToListAsync();

            // 3. Map the results to the safe EmployeeDto
            var employeeDtos = employees.Select(employee => new EmployeeDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email
            }).ToList();

            return Ok(employeeDtos);
        }
    }
}