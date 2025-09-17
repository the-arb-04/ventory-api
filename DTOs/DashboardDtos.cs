namespace InventoryTracker.DTOs
{
    public class LowStockDto
    {
        public int Count { get; set; }
    }

    public class InventorySummaryDto
    {
        public long QuantityInHand { get; set; }
        public long QuantityToBeReceived { get; set; }
    }

    public class TodaySnapshotDto
    {
        public decimal Revenue { get; set; }
        public decimal GrossProfit { get; set; }
        public int SalesCount { get; set; }
    }

    public class InventoryValueDto
    {
        public decimal CostValue { get; set; }
        public decimal RetailValue { get; set; }
    }

    public class SalesDataPointDto
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopSellingItemDto
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ProfitableItemDto
    {
        public string Name { get; set; }
        public decimal ProfitMargin { get; set; }
    }
    public class DeadStockDto
    {
        public int ItemCount { get; set; }
        public decimal TotalValue { get; set; }
    }
    public class ReorderItemDto
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int CurrentQuantity { get; set; }
        public int MinQuantity { get; set; }
        public string? SupplierName { get; set; } // Use nullable string for supplier
    }

    public class SlowMoverDto
    {
        public int ItemId { get; set; }
        public string? Name { get; set; }
        public int QuantityOnHand { get; set; }
        public int? DaysSinceLastSale { get; set; }


    }

    public class DeadStockItemDetailDto // <-- MOVED OUTSIDE
    {
        public string Name { get; set; }
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public decimal ValueAtCost { get; set; }
        public DateTime? LastSoldDate { get; set; }
    }
    
    public class SmartReorderItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CurrentStock { get; set; }
        public double PredictedDaysLeft { get; set; }
        public int ForecastedSales7d { get; set; }
        public int ForecastedSales30d { get; set; }
        public string SupplierName { get; set; }
        public string Urgency { get; set; }
    }

    // DTO for the final API response, containing both lists
    public class ReorderForecastDto
    {
        public List<SmartReorderItemDto> UrgentItems { get; set; }
        public List<SmartReorderItemDto> WatchlistItems { get; set; }
    }
}
