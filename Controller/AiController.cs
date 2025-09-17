using InventoryTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Inventory_Tracker.DTOs;
using Microsoft.AspNetCore.Identity; // CHANGE: Add this
using Inventory_Tracker.Models;    // CHANGE: Add this

namespace InventoryTracker.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly UserManager<ApplicationUser> _userManager; // CHANGE: Add UserManager

        // CHANGE: Inject UserManager
        public AiController(IAiService aiService, UserManager<ApplicationUser> userManager)
        {
            _aiService = aiService;
            _userManager = userManager;
        }

        [HttpPost("generate-description")]
        public async Task<IActionResult> GenerateDescription([FromBody] GenerateDescriptionRequest request)
        {
            // No changes needed here, this endpoint is already secure enough.
            if (string.IsNullOrWhiteSpace(request?.ProductName))
            {
                return BadRequest("Product name is required.");
            }
            try
            {
                var description = await _aiService.GenerateProductDescriptionAsync(request.ProductName);
                return Ok(new { description });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("business-summary")]
        public async Task<IActionResult> GetBusinessSummary()
        {
            try
            {
                // CHANGE: Implement the new shop-level ownership pattern
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return Unauthorized();
                var ownerId = currentUser.OwnerId ?? currentUser.Id;

                var summary = await _aiService.GetBusinessSummaryAsync(ownerId);
                return Ok(summary); // The service already returns the DTO
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}