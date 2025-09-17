using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Inventory_Tracker.Models;
using Inventory_Tracker.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // CHANGE: Add this using
using System.Security.Claims;
using Inventory_Tracker.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Inventory_Tracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger, UserManager<ApplicationUser> userManager)
        {
            _categoryService = categoryService;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            _logger.LogInformation("Attempting to get all categories.");
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return Unauthorized();
                var ownerId = currentUser.OwnerId ?? currentUser.Id;
                var categories = await _categoryService.GetAllCategoriesAsync(ownerId);
                _logger.LogInformation("Successfully retrieved all categories.");
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all categories.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            _logger.LogInformation("Attempting to get category with ID: {CategoryId}", id);
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return Unauthorized();
                var ownerId = currentUser.OwnerId ?? currentUser.Id;
                var category = await _categoryService.GetCategoryByIdAsync(id, ownerId);
                if (category == null)
                {
                    _logger.LogWarning("Category with ID: {CategoryId} was not found.", id);
                    return NotFound();
                }
                _logger.LogInformation("Successfully found category {CategoryId}.", id);
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting category {CategoryId}", id);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] CreateCategoryDto categoryDto)
        {
            _logger.LogInformation("Attempting to create a new category with name: {CategoryName}", categoryDto.Name);
            try
            {
                // 1. Get the user ID from the token
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return Unauthorized();
                var ownerId = currentUser.OwnerId ?? currentUser.Id;

                // 2. Create a new Category database entity from the DTO
                var category = new Category
                {
                    Name = categoryDto.Name,
                    UserId = ownerId // 3. Assign ownership securely on the server
                };
                
                var createdCategory = await _categoryService.CreateCategoryAsync(category);

                _logger.LogInformation("Successfully created new category with ID {CategoryId}", createdCategory.Id);
                return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.Id }, createdCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a category.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            _logger.LogInformation("Attempting to delete category {CategoryId}", id);
            try
            {
                // 1. Get the logged-in user's ID from their token.
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return Unauthorized();
                var ownerId = currentUser.OwnerId ?? currentUser.Id;

                // 2. Call the updated service method, passing both IDs.
                var success = await _categoryService.DeleteCategoryAndItemsAsync(id, ownerId);

                // 3. Check if the service returned 'false'.
                if (!success)
                {
                    // This means the category was either not found or didn't belong to the user.
                    _logger.LogWarning("Delete failed: Category {CategoryId} not found for the current user.", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully deleted category {CategoryId}", id);
                return NoContent(); // Standard response for a successful DELETE.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting category {CategoryId}", id);
                return StatusCode(500, "An internal server error occurred.");
            }
        }
    }
}