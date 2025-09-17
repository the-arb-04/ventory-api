using Inventory_Tracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory_Tracker.Services
{
    public class ItemHistoryService : IItemHistoryService
    {
        private readonly InventoryDbContext _context;

        public ItemHistoryService(InventoryDbContext context)
        {
            _context = context;
        }

        // CHANGE: Updated method signature to accept userId
        public async Task<IEnumerable<ItemHistory>> GetHistoryByItemIdAsync(int itemId, string userId)
        {
            // CHANGE: Added a second condition to the Where clause to filter by the user.
            return await _context.ItemHistories
                .Where(h => h.ItemId == itemId && h.UserId == userId)
                .OrderByDescending(h => h.Timestamp)
                .ToListAsync();
        }

        public async Task AddHistoryAsync(ItemHistory history)
        {
            // No change is needed here, as the UserId is set before this method is called.
            await _context.ItemHistories.AddAsync(history);
            await _context.SaveChangesAsync();
        }
    }
}