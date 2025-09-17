using Inventory_Tracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory_Tracker.Services
{
    public interface ICategoryService
    {
        /// <summary>
        /// Gets all categories that belong to a specific user.
        /// </summary>
        Task<IEnumerable<Category>> GetAllCategoriesAsync(string userId);

        /// <summary>
        /// Gets a single category by its ID, ensuring it belongs to the specified user.
        /// </summary>
        Task<Category?> GetCategoryByIdAsync(int id, string userId);

        /// <summary>
        /// Creates a new category. The UserId must be set on the category object before calling.
        /// </summary>
        Task<Category> CreateCategoryAsync(Category category);

        /// <summary>
        /// Updates a category, ensuring it belongs to the specified user. Returns true if successful.
        /// </summary>
        Task<bool> UpdateCategoryAsync(Category category, string userId);

        /// <summary>
        /// Deletes a category and its associated items, ensuring it belongs to the user. Returns true if successful.
        /// </summary>
        Task<bool> DeleteCategoryAndItemsAsync(int id, string userId);
    }
}