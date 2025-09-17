// Controllers/DashboardController.cs
using InventoryTracker.Services; // Add this using statement
using Microsoft.AspNetCore.Mvc;
using InventoryTracker.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity; // CHANGE: Add this
using Inventory_Tracker.Models;

namespace InventoryTracker.Controllers
{
    // This DTO can now be moved out or stay here, your choice.
    public class LowStockDto
    {
        public int Count { get; set; }
    }

    public class InventorySummaryDto
    {
        public long QuantityInHand { get; set; }
        public long QuantityToBeReceived { get; set; }
    }
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        // The service is "injected" here via the constructor
        public DashboardController(IDashboardService dashboardService, UserManager<ApplicationUser> userManager)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
        }

        // The endpoint logic is now just one clean line
        [HttpGet("low-stock-count")]
        public async Task<IActionResult> GetLowStockCount()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return Unauthorized();
                var ownerId = currentUser.OwnerId ?? currentUser.Id;
                var count = await _dashboardService.GetLowStockCountAsync(ownerId);
                var result = new LowStockDto { Count = count };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // Controllers/DashboardController.cs
        // ... (existing GetLowStockCount method) ...

        [HttpGet("inventory-summary")]
        public async Task<IActionResult> GetInventorySummary()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
                var summary = await _dashboardService.GetInventorySummaryAsync(ownerId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("today-snapshot")]
        public async Task<IActionResult> GetTodaySnapshot()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
                var snapshot = await _dashboardService.GetTodaySnapshotAsync(ownerId);
                return Ok(snapshot);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("inventory-value")]
        public async Task<IActionResult> GetInventoryValue()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
                var value = await _dashboardService.GetInventoryValueAsync(ownerId);
                return Ok(value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("sales-performance")]
        public async Task<IActionResult> GetSalesPerformance([FromQuery] string period = "30d") // Default to 30d if not provided
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
                var performanceData = await _dashboardService.GetSalesPerformanceAsync(ownerId ,period);
                return Ok(performanceData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("top-selling-items")]
        public async Task<IActionResult> GetTopSellingItems([FromQuery] string period = "30d")
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
                var topItems = await _dashboardService.GetTopSellingItemsAsync(ownerId ,period);
                return Ok(topItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("most-profitable-items")]
        public async Task<IActionResult> GetMostProfitableItems()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
                var items = await _dashboardService.GetMostProfitableItemsAsync(ownerId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("dead-stock")]
        public async Task<IActionResult> GetDeadStock()
        {
            try
            {
               var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
                var deadStock = await _dashboardService.GetDeadStockAsync(ownerId);
                return Ok(deadStock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("dead-stock-details")]
        public async Task<IActionResult> GetDeadStockDetails()
        {
            // You can add a try-catch block for consistency
            try
            {
               var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
                var deadStockItems = await _dashboardService.GetDeadStockDetailsAsync(ownerId);
                return Ok(deadStockItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("items-to-reorder")]
        public async Task<IActionResult> GetItemsToReorder()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
                var items = await _dashboardService.GetItemsToReorderAsync(ownerId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("slow-movers")]
        public async Task<IActionResult> GetSlowMovers()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
                var items = await _dashboardService.GetSlowMoversAsync(ownerId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        [HttpGet("top-selling-items-forecast")]
        public async Task<ActionResult<IEnumerable<TopSellerForecastDto>>> GetTopSellingItemsWithForecast()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
            var data = await _dashboardService.GetTopSellingItemsWithForecastAsync(ownerId);
            return Ok(data);
        }

        [HttpGet("sales-forecast")]
        public async Task<ActionResult<IEnumerable<ProphetForecastDto>>> GetOverallSalesForecast()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
            var forecast = await _dashboardService.GetOverallSalesForecastAsync(ownerId);
            if (forecast == null)
            {
                return Ok(new List<ProphetForecastDto>()); // Return empty list if no forecast
            }
            return Ok(forecast);
        }

        // Add this new endpoint to your DashboardController.cs
        [HttpGet("reorder-forecast")]
        public async Task<ActionResult<ReorderForecastDto>> GetReorderForecast()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
                var forecast = await _dashboardService.GetReorderForecastAsync(ownerId);
                return Ok(forecast);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

    }
}