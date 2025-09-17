using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Inventory_Tracker.DTOs;
using Inventory_Tracker.Models;
using Inventory_Tracker.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Required for .Any()

namespace Inventory_Tracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        // --- Public Registration for New Shop Admins ---
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ----- CHANGE 1: REMOVED a security block from here -----
            // The `if (await _userManager.Users.AnyAsync())` check has been deleted
            // to allow any new user to register their own shop.

            var user = new ApplicationUser
            {
                Name = model.Name,
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true, // Consider implementing email confirmation later
                ShopName = model.ShopName,
                Location = model.Location
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            // ----- CHANGE 2: IMPROVED error handling -----
            if (!result.Succeeded)
            {
                // Specifically check if the email is already in use
                if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
                {
                    return BadRequest("An account with this email already exists.");
                }
                // Return other registration errors if they occur
                return BadRequest(result.Errors);
            }

            // Every new user signing up here becomes an Admin of their shop
            await _userManager.AddToRoleAsync(user, "Admin");

            return Ok(new { message = "Admin account for new shop registered successfully!" });
        }

        // --- Login ---
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized("Invalid email or password");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid email or password");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Email,
                    user.Name,
                    user.ShopName,
                    user.Location,
                    Roles = roles
                }
            });
        }
    }
}