using Inventory_Tracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory_Tracker.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly InventoryDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(InventoryDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync(string userId)
        {
            _logger.LogInformation("Service: Getting all categories for user {UserId}", userId);
            
            // CHANGE: Added .Where() clause to filter by the user's ID.
            return await _context.Categories
                                 .Where(c => c.UserId == userId)
                                 .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id, string userId)
        {
            _logger.LogInformation("Service: Getting category {CategoryId} for user {UserId}", id, userId);

            // CHANGE: Find category only if the ID and UserId both match.
            return await _context.Categories
                                 .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            // No change needed here. The UserId was already set in the controller.
            _logger.LogInformation("Service: Creating new category '{CategoryName}' for user {UserId}", category.Name, category.UserId);
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> UpdateCategoryAsync(Category category, string userId)
        {
            _logger.LogInformation("Service: Updating category {CategoryId} for user {UserId}", category.Id, userId);
            
            // CHANGE: First, verify that the category being updated actually belongs to the user.
            var existingCategory = await _context.Categories
                                                 .AsNoTracking() // Use AsNoTracking for existence checks
                                                 .FirstOrDefaultAsync(c => c.Id == category.Id && c.UserId == userId);

            if (existingCategory == null)
            {
                // The category does not exist or the user does not own it.
                return false;
            }

            // The user is authorized, so proceed with the update.
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryAndItemsAsync(int id, string userId)
        {
            _logger.LogInformation("Service: Deleting category {CategoryId} for user {UserId}", id, userId);

            // CHANGE: Find the category ensuring it belongs to the current user.
            var category = await _context.Categories
                                         .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
            {
                // If the category doesn't exist or doesn't belong to the user, do nothing and return false.
                return false;
            }

            var itemsToDelete = _context.Items.Where(i => i.CategoryId == id && i.UserId == userId);
            _context.Items.RemoveRange(itemsToDelete);

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}