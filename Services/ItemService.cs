using Inventory_Tracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory_Tracker.Services
{
    public class ItemService : IItemService
    {
        private readonly InventoryDbContext _context;
        private readonly ILogger<ItemService> _logger;

        public ItemService(InventoryDbContext context, ILogger<ItemService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Item>> GetAllItemsAsync(string userId)
        {
            _logger.LogInformation("Service: Getting all items for user {UserId}", userId);
            
            // CHANGE: Added .Where() clause to filter items by the user's ID.
            return await _context.Items
                .Where(i => i.UserId == userId)
                .Include(i => i.Category)
                .Include(i => i.Supplier) 
                .ToListAsync();
        }

        public async Task<Item?> GetItemByIdAsync(int id, string userId)
        {
            _logger.LogInformation("Service: Getting item by ID {ItemId} for user {UserId}", id, userId);

            // CHANGE: Find item only if the ID and UserId both match.
            return await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Supplier)
                .Include(i => i.ItemHistories)
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
        }

        public async Task<Item> CreateItemAsync(Item item)
        {
            // No change to logic, as the UserId is set in the controller before this is called.
            _logger.LogInformation("Service: Creating new item '{ItemName}' for user {UserId}", item.Name, item.UserId);
            item.CreatedDate = DateTime.UtcNow;
            item.UpdatedDate = DateTime.UtcNow;
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateItemWithHistoryAsync(Item updatedItem, string userId, string? userName)
        {
            _logger.LogInformation("Service: Updating item {ItemId} for user {UserId}", updatedItem.Id, userId);

            // CHANGE: Securely find the existing item, verifying ownership.
            var existingItem = await _context.Items.FirstOrDefaultAsync(i => i.Id == updatedItem.Id && i.UserId == userId);
            if (existingItem == null)
            {
                _logger.LogWarning("Item with ID {ItemId} not found for user {UserId}.", updatedItem.Id, userId);
                return false; // Item not found or user doesn't own it.
            }

            int originalQuantity = existingItem.Quantity;
            
            // Map updated properties to the existing entity
            existingItem.Name = updatedItem.Name;
            // ... (rest of the properties)
            existingItem.SupplierId = updatedItem.SupplierId; 
            existingItem.Description = updatedItem.Description;
            existingItem.IsActive = updatedItem.IsActive;
            existingItem.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (originalQuantity != updatedItem.Quantity)
            {
                // ... (history logging logic)
                var history = new ItemHistory
                {
                    ItemId = updatedItem.Id,
                    // CHANGE: Stamp the history record with the owner's ID
                    UserId = userId,
                    // ... (rest of history properties)
                    UserName = userName ?? "System",
                };
                await _context.ItemHistories.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            
            return true; // Return true on success
        }

        public async Task<bool> DeleteItemAsync(int id, string userId)
        {
            _logger.LogInformation("Service: Deleting item {ItemId} for user {UserId}", id, userId);

            // CHANGE: Find the item while also verifying ownership.
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
            if (item == null)
            {
                return false; // Item not found or user doesn't own it.
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return true; // Return true on success.
        }

        public async Task<bool> ItemExistsAsync(int id, string userId)
        {
            // CHANGE: Check for existence based on both ID and owner.
            return await _context.Items.AnyAsync(e => e.Id == id && e.UserId == userId);
        }
    }
}