using InventoryTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Inventory_Tracker.DTOs; // For CreateSaleDto
using Microsoft.AspNetCore.Identity; // CHANGE: Add this
using Inventory_Tracker.Models; // CHANGE: Add this
using System.Threading.Tasks;
using System;
using System.Collections.Generic; // CHANGE: Add this

namespace InventoryTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly ISalesService _salesService;
        private readonly UserManager<ApplicationUser> _userManager; // CHANGE: Add UserManager

        // CHANGE: Inject UserManager
        public SalesController(ISalesService salesService, UserManager<ApplicationUser> userManager)
        {
            _salesService = salesService;
            _userManager = userManager;
        }

        // CHANGE: ADDED THIS ENTIRE GET METHOD
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Sale>>> GetSales()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return Unauthorized();
                var ownerId = currentUser.OwnerId ?? currentUser.Id;

                var sales = await _salesService.GetSalesAsync(ownerId);
                return Ok(sales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleDto saleDto)
        {
            if (saleDto == null || saleDto.ItemId <= 0 || saleDto.QuantitySold <= 0)
            {
                return BadRequest("Invalid sale data.");
            }

            try
            {
                // CHANGE: Implement the new shop-level ownership pattern
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return Unauthorized();
                var ownerId = currentUser.OwnerId ?? currentUser.Id;
                
                // We can still pass the user's email for logging purposes
                var userEmail = currentUser.Email; 

                await _salesService.CreateSaleAsync(saleDto, ownerId, userEmail);
                return Ok(new { message = "Sale recorded successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}