using Inventory_Tracker.Models;

namespace InventoryTracker.Services
{
    // A simple DTO to receive the sale data from the frontend
    public class CreateSaleDto
    {
        public int ItemId { get; set; }
        public int QuantitySold { get; set; }
    }

    public interface ISalesService
    {
        Task CreateSaleAsync(CreateSaleDto saleDto, string userId, string userEmail);
        Task<IEnumerable<Sale>> GetSalesAsync(string userId);
    }
}