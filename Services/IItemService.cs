using Inventory_Tracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory_Tracker.Services
{
    public interface IItemService
    {
        /// <summary>
        /// Gets all items that belong to a specific user.
        /// </summary>
        Task<IEnumerable<Item>> GetAllItemsAsync(string userId);

        /// <summary>
        /// Gets a single item by its ID, ensuring it belongs to the specified user.
        /// </summary>
        Task<Item?> GetItemByIdAsync(int id, string userId);

        /// <summary>
        /// Creates a new item. The UserId must be set on the item object before calling.
        /// </summary>
        Task<Item> CreateItemAsync(Item item);

        /// <summary>
        /// Updates an item and creates a history record, ensuring it belongs to the user.
        /// </summary>
        /// <returns>True if the update was successful.</returns>
        Task<bool> UpdateItemWithHistoryAsync(Item updatedItem, string userId, string? userName);

        /// <summary>
        /// Deletes an item, ensuring it belongs to the user.
        /// </summary>
        /// <returns>True if the deletion was successful.</returns>
        Task<bool> DeleteItemAsync(int id, string userId);

        /// <summary>
        /// Checks if an item exists for the specified user.
        /// </summary>
        Task<bool> ItemExistsAsync(int id, string userId);
    }
}