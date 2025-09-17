using InventoryTracker.DTOs;

namespace InventoryTracker.Services
{
    public class GenerateDescriptionRequest
    {
        public string ProductName { get; set; }
    }


    public interface IAiService
    {
        Task<string> GenerateProductDescriptionAsync(string productName);
        Task<AiSummaryResponseDto> GetBusinessSummaryAsync(string userId);
        
    }

}