using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Inventory_Tracker.DTOs;
using Inventory_Tracker.Services;
using Microsoft.AspNetCore.Authorization; // CHANGE: Add this
using System.Security.Claims;          // CHANGE: Add this

namespace Inventory_Tracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // CHANGE: Secure the entire controller
    public class ForecastController : ControllerBase
    {
        private readonly ForecastingService _forecastingService;

        public ForecastController(ForecastingService forecastingService)
        {
            _forecastingService = forecastingService;
        }

        [HttpGet("item/{itemId}")]
        public async Task<ActionResult<IEnumerable<ProphetForecastDto>>> GetForecast(int itemId, [FromQuery] int horizon = 7)
        {
            if (horizon <= 0 || horizon > 90)
            {
                return BadRequest("Horizon must be between 1 and 90 days.");
            }

            // CHANGE: Get the logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // CHANGE: Pass the userId to the service for a secure lookup
            var prediction = await _forecastingService.GetForecastForItemAsync(itemId, userId, horizon);

            if (prediction == null)
            {
                // This generic message is more secure as it doesn't reveal *why* the forecast failed
                return NotFound("Forecast not available for this item.");
            }
            
            return Ok(prediction);
        }
    }
}