using InventoryTracker.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryTracker.Services
{
    public interface IDashboardService
    {
        Task<int> GetLowStockCountAsync(string userId);
        Task<InventorySummaryDto> GetInventorySummaryAsync(string userId);
        Task<TodaySnapshotDto> GetTodaySnapshotAsync(string userId);
        Task<InventoryValueDto> GetInventoryValueAsync(string userId);
        Task<IEnumerable<SalesDataPointDto>> GetSalesPerformanceAsync(string userId, string period);
        Task<IEnumerable<TopSellingItemDto>> GetTopSellingItemsAsync(string userId, string period);
        Task<IEnumerable<ProfitableItemDto>> GetMostProfitableItemsAsync(string userId);
        Task<DeadStockDto> GetDeadStockAsync(string userId);
        Task<IEnumerable<ReorderItemDto>> GetItemsToReorderAsync(string userId);
        Task<IEnumerable<SlowMoverDto>> GetSlowMoversAsync(string userId);
        Task<IEnumerable<DeadStockItemDetailDto>> GetDeadStockDetailsAsync(string userId);
        Task<IEnumerable<TopSellerForecastDto>> GetTopSellingItemsWithForecastAsync(string userId);
        Task<IEnumerable<ProphetForecastDto>> GetOverallSalesForecastAsync(string userId, int horizon = 14);
        Task<ReorderForecastDto> GetReorderForecastAsync(string userId);
    }
}