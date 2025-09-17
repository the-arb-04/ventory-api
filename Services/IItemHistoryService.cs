using Inventory_Tracker.Models;

namespace Inventory_Tracker.Services
{
    public interface IItemHistoryService
    {
        Task<IEnumerable<ItemHistory>> GetHistoryByItemIdAsync(int id, string userId);
        Task AddHistoryAsync(ItemHistory history);
    }
}
