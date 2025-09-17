using Dapper;
using Npgsql;
using InventoryTracker.DTOs;
using Inventory_Tracker.DTOs;
using Inventory_Tracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Inventory_Tracker.Services;

namespace InventoryTracker.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IConfiguration _configuration;
        private readonly InventoryDbContext _context;
        private readonly ForecastingService _forecastingService;
        private readonly IHttpClientFactory _httpClientFactory;

        public DashboardService(IConfiguration configuration, InventoryDbContext context, ForecastingService forecastingService, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _context = context;
            _forecastingService = forecastingService;
            _httpClientFactory = httpClientFactory;
        }

        // CHANGE: Method now accepts userId
        public async Task<int> GetLowStockCountAsync(string userId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            await using var connection = new NpgsqlConnection(connectionString);

            // CHANGE: Added WHERE clause to filter by the item's owner
            var sql = @"SELECT COUNT(*) FROM items WHERE quantity <= minquantity AND ""UserId"" = @UserId;";

            // CHANGE: Pass the userId to the Dapper query
            var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });

            return count;
        }

        // CHANGE: Method now accepts userId
        public async Task<InventorySummaryDto> GetInventorySummaryAsync(string userId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            await using var connection = new NpgsqlConnection(connectionString);

            // CHANGE: Added WHERE clauses for UserId in both queries
            var sql = @"
                SELECT COALESCE(SUM(quantity), 0) FROM items WHERE ""UserId"" = @UserId;
                SELECT COALESCE(SUM(poi.quantity_ordered), 0)
                FROM purchase_order_items poi
                JOIN purchase_orders po ON poi.purchase_order_id = po.id
                WHERE po.status = 'Pending' AND po.""UserId"" = @UserId;";

            using (var multi = await connection.QueryMultipleAsync(sql, new { UserId = userId }))
            {
                var quantityInHand = await multi.ReadSingleOrDefaultAsync<long?>();
                var quantityToBeReceived = await multi.ReadSingleOrDefaultAsync<long?>();

                return new InventorySummaryDto
                {
                    QuantityInHand = quantityInHand ?? 0,
                    QuantityToBeReceived = quantityToBeReceived ?? 0
                };
            }
        }

        // CHANGE: Method now accepts userId
        public async Task<TodaySnapshotDto> GetTodaySnapshotAsync(string userId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            await using var connection = new NpgsqlConnection(connectionString);

            // CHANGE: Added WHERE clause for UserId
            var sql = @"
                SELECT
                    COALESCE(SUM(sale_price_at_time_of_sale * quantity_sold), 0) AS Revenue,
                    COALESCE(SUM((sale_price_at_time_of_sale - cost_price_at_time_of_sale) * quantity_sold), 0) AS GrossProfit,
                    COALESCE(SUM(quantity_sold), 0)::int AS SalesCount
                FROM sales
                WHERE sale_timestamp >= CURRENT_DATE 
                AND sale_timestamp < CURRENT_DATE + INTERVAL '1 day'
                AND ""UserId"" = @UserId;";

            var snapshot = await connection.QuerySingleOrDefaultAsync<TodaySnapshotDto>(sql, new { UserId = userId });

            return snapshot ?? new TodaySnapshotDto();
        }


        public async Task<InventoryValueDto> GetInventoryValueAsync(string userId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            await using var connection = new NpgsqlConnection(connectionString);

            // CHANGE: Added WHERE clauses for UserId in both queries
            var sql = @"
                SELECT COALESCE(SUM(costprice * quantity), 0) FROM items WHERE ""UserId"" = @UserId;
                SELECT COALESCE(SUM(sellingprice * quantity), 0) FROM items WHERE ""UserId"" = @UserId;";

            // CHANGE: Pass the userId to the Dapper query
            using (var multi = await connection.QueryMultipleAsync(sql, new { UserId = userId }))
            {
                var costValue = await multi.ReadSingleOrDefaultAsync<decimal?>();
                var retailValue = await multi.ReadSingleOrDefaultAsync<decimal?>();

                return new InventoryValueDto
                {
                    CostValue = costValue ?? 0,
                    RetailValue = retailValue ?? 0
                };
            }
        }

        public async Task<IEnumerable<SalesDataPointDto>> GetSalesPerformanceAsync(string userId, string period)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            await using var connection = new NpgsqlConnection(connectionString);

            var interval = period switch
            {
                "7d" => "7 days",
                "90d" => "90 days",
                _ => "30 days",
            };

            // CHANGE: Added WHERE clause for UserId
            var sql = $@"
                SELECT
                    DATE_TRUNC('day', sale_timestamp)::date AS ""Date"",
                    SUM(sale_price_at_time_of_sale * quantity_sold) AS ""TotalRevenue""
                FROM sales
                WHERE sale_timestamp >= NOW() - INTERVAL '{interval}' AND ""UserId"" = @UserId
                GROUP BY DATE_TRUNC('day', sale_timestamp)
                ORDER BY ""Date"";";

            // CHANGE: Pass the userId to the Dapper query
            var salesData = await connection.QueryAsync<SalesDataPointDto>(sql, new { UserId = userId });

            return salesData;
        }

        public async Task<IEnumerable<TopSellingItemDto>> GetTopSellingItemsAsync(string userId, string period)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            await using var connection = new NpgsqlConnection(connectionString);

            var interval = period switch
            {
                "7d" => "7 days",
                "90d" => "90 days",
                _ => "30 days",
            };

            // CHANGE: Added WHERE clause for UserId on the sales table
            var sql = $@"
                SELECT
                    i.id AS ""ItemId"",
                    i.name AS ""Name"",
                    SUM(s.sale_price_at_time_of_sale * s.quantity_sold) AS ""TotalRevenue""
                FROM sales s
                JOIN items i ON s.item_id = i.id
                WHERE s.sale_timestamp >= NOW() - INTERVAL '{interval}' AND s.""UserId"" = @UserId
                GROUP BY i.id, i.name
                ORDER BY ""TotalRevenue"" DESC
                LIMIT 5;";

            // CHANGE: Pass the userId to the Dapper query
            var topItems = await connection.QueryAsync<TopSellingItemDto>(sql, new { UserId = userId });

            return topItems;
        }

        public async Task<IEnumerable<TopSellerForecastDto>> GetTopSellingItemsWithForecastAsync(string userId)
{
    // CHANGE: Pass the userId to the internal call
    var topSellers = await GetTopSellingItemsAsync(userId, "30d");

    var results = new List<TopSellerForecastDto>();

    foreach (var seller in topSellers)
    {
        
        var forecast = await _forecastingService.GetForecastForItemAsync(seller.ItemId,userId, 30);

        decimal projectedRevenue = 0;
        if (forecast != null)
        {
            // CHANGE: Securely find the item to verify ownership
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == seller.ItemId && i.UserId == userId);
            if (item != null)
            {
                var projectedUnits = forecast.Sum(p => (decimal)p.Yhat);
                projectedRevenue = projectedUnits * item.SellingPrice;
            }
        }

        results.Add(new TopSellerForecastDto
        {
            Name = seller.Name,
            HistoricalRevenue = seller.TotalRevenue,
            ProjectedRevenue = projectedRevenue
        });
    }

    return results;
}

public async Task<IEnumerable<ProfitableItemDto>> GetMostProfitableItemsAsync(string userId)
{
    var connectionString = _configuration.GetConnectionString("DefaultConnection");
    await using var connection = new NpgsqlConnection(connectionString);

    // CHANGE: Added WHERE clause for UserId on the sales table
    var sql = @"
        SELECT
            i.name AS ""Name"",
            (SUM(s.sale_price_at_time_of_sale - s.cost_price_at_time_of_sale) / NULLIF(SUM(s.sale_price_at_time_of_sale), 0)) * 100 AS ""ProfitMargin""
        FROM sales s
        JOIN items i ON s.item_id = i.id
        WHERE s.sale_timestamp >= NOW() - INTERVAL '90 days' AND s.""UserId"" = @UserId
        GROUP BY i.id, i.name
        HAVING SUM(s.sale_price_at_time_of_sale) > 0
        ORDER BY ""ProfitMargin"" DESC
        LIMIT 3;";

    var profitableItems = await connection.QueryAsync<ProfitableItemDto>(sql, new { UserId = userId });

    return profitableItems;
}

public async Task<DeadStockDto> GetDeadStockAsync(string userId)
{
    var connectionString = _configuration.GetConnectionString("DefaultConnection");
    await using var connection = new NpgsqlConnection(connectionString);

    // CHANGE: Added WHERE clauses for UserId to both the outer and inner queries
    var sql = @"
        SELECT
            COUNT(*)::int AS ""ItemCount"",
            COALESCE(SUM(i.costprice * i.quantity), 0) AS ""TotalValue""
        FROM items i
        WHERE i.""UserId"" = @UserId AND NOT EXISTS (
            SELECT 1
            FROM sales s
            WHERE s.item_id = i.id
            AND s.""UserId"" = @UserId
            AND s.sale_timestamp >= NOW() - INTERVAL '90 days'
        );";

    var deadStock = await connection.QuerySingleOrDefaultAsync<DeadStockDto>(sql, new { UserId = userId });

    return deadStock ?? new DeadStockDto();
}

public async Task<IEnumerable<DeadStockItemDetailDto>> GetDeadStockDetailsAsync(string userId)
{
    var connectionString = _configuration.GetConnectionString("DefaultConnection");
    await using var connection = new NpgsqlConnection(connectionString);

    // CHANGE: Added WHERE clause for UserId to filter items and sales subquery
    var sql = @"
        SELECT
            i.name AS ""Name"",
            i.sku AS ""Sku"",
            i.quantity AS ""Quantity"",
            (i.costprice * i.quantity) AS ""ValueAtCost"",
            latest_sales.last_sale_date AS ""LastSoldDate""
        FROM items i
        LEFT JOIN (
            SELECT
                item_id,
                MAX(sale_timestamp) AS last_sale_date
            FROM sales
            WHERE ""UserId"" = @UserId
            GROUP BY item_id
        ) AS latest_sales ON i.id = latest_sales.item_id
        WHERE
            i.quantity > 0 AND i.""UserId"" = @UserId AND
            (latest_sales.last_sale_date IS NULL OR latest_sales.last_sale_date < NOW() - INTERVAL '90 days');";

    var deadStockItems = await connection.QueryAsync<DeadStockItemDetailDto>(sql, new { UserId = userId });

    return deadStockItems;
}

        public async Task<IEnumerable<ReorderItemDto>> GetItemsToReorderAsync(string userId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            await using var connection = new NpgsqlConnection(connectionString);

            // FIXED: This query now joins with the suppliers table to get the name
             var sql = @"
        SELECT
            i.id AS ""ItemId"",
            i.name AS ""Name"",
            i.quantity AS ""CurrentQuantity"",
            i.minquantity AS ""MinQuantity"",
            s.name AS ""SupplierName""
        FROM items i
        LEFT JOIN suppliers s ON i.supplier_id = s.id
        WHERE i.quantity <= i.minquantity AND i.""UserId"" = @UserId;";

    // CHANGE: Pass the userId to the Dapper query
    var itemsToReorder = await connection.QueryAsync<ReorderItemDto>(sql, new { UserId = userId });

            return itemsToReorder;
        }

        public async Task<IEnumerable<SlowMoverDto>> GetSlowMoversAsync(string userId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            await using var connection = new NpgsqlConnection(connectionString);

            // This query uses a CTE (WITH clause) to find items sold, but not within the last 30 days.
           // CHANGE: Added WHERE clauses for UserId in both the CTE and the main query
    var sql = @"
        WITH LastSale AS (
            SELECT
                item_id,
                MAX(sale_timestamp) as last_sale_date
            FROM sales
            WHERE ""UserId"" = @UserId
            GROUP BY item_id
        )
        SELECT
            i.id AS ""ItemId"",
            i.name AS ""Name"",
            i.quantity AS ""QuantityOnHand"",
            EXTRACT(DAY FROM NOW() - ls.last_sale_date)::int AS ""DaysSinceLastSale""
        FROM items i
        JOIN LastSale ls ON i.id = ls.item_id
        WHERE ls.last_sale_date < NOW() - INTERVAL '30 days' AND i.""UserId"" = @UserId
        ORDER BY ls.last_sale_date ASC
        LIMIT 5;";

    // CHANGE: Pass the userId to the Dapper query
    var slowMovers = await connection.QueryAsync<SlowMoverDto>(sql, new { UserId = userId });
            return slowMovers;
        }

        public async Task<IEnumerable<ProphetForecastDto>> GetOverallSalesForecastAsync(string userId , int horizon = 14)
        {
            // 1. Get all sales from the last 90 days to build a history
            var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
            var allSales = await _context.Sales
                .Where(s => s.SaleTimestamp >= ninetyDaysAgo && s.UserId == userId)
                .ToListAsync();

            // 2. Aggregate total revenue by day
            var dailyRevenue = allSales
                .GroupBy(s => s.SaleTimestamp.Date)
                .Select(g => new
                {
                    saleTimestamp = g.Key,
                    // This is the key change: we send total revenue, not quantity
                    quantitySold = g.Sum(s => s.SalePriceAtTimeOfSale * s.QuantitySold)
                })
                .ToList();

            if (dailyRevenue.Count < 10)
            {
                return null; // Not enough data for a forecast
            }

            // 3. Call the Python service with the aggregated revenue data
            var client = _httpClientFactory.CreateClient();
            try
            {
                var payload = new
                {
                    salesData = dailyRevenue,
                    horizon = horizon
                };
                var response = await client.PostAsJsonAsync("http://127.0.0.1:5000/forecast", payload);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<ProphetForecastDto>>();
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        // Add this new method to your DashboardService.cs
        public async Task<ReorderForecastDto> GetReorderForecastAsync(string userId)
{
    var allItems = await _context.Items
        .Where(i => i.UserId == userId)
        .Include(i => i.Supplier)
        .ToListAsync();

    var urgentItems = new List<SmartReorderItemDto>();
    var watchlistItems = new List<SmartReorderItemDto>();

    foreach (var item in allItems)
    {
        // Get summed forecast values
        var forecast7dResult = await _forecastingService.GetForecastForItemAsync(item.Id,userId, 7);
        var forecast30dResult = await _forecastingService.GetForecastForItemAsync(item.Id,userId, 30);

        var forecast7d = (int)Math.Round(forecast7dResult?.Sum(f => f.Yhat) ?? 0);
        var forecast30d = (int)Math.Round(forecast30dResult?.Sum(f => f.Yhat) ?? 0);

        // Calculate days left based on average daily demand
        double dailyForecast = forecast30d / 30.0;
        double predictedDaysLeft = dailyForecast > 0 ? item.Quantity / dailyForecast : double.PositiveInfinity;

        string urgency = predictedDaysLeft switch
        {
            <= 7 => "Urgent",
            <= 30 => "Watchlist",
            _ => "Normal"
        };

        var smartDto = MapToSmartReorderDto(item, forecast7d, forecast30d, predictedDaysLeft, urgency);

        if (urgency == "Urgent") urgentItems.Add(smartDto);
        else if (urgency == "Watchlist") watchlistItems.Add(smartDto);
    }

    return new ReorderForecastDto
{
    UrgentItems = urgentItems,
    WatchlistItems = watchlistItems
};
}

private SmartReorderItemDto MapToSmartReorderDto(
    Inventory_Tracker.Models.Item item,
    int forecast7d,
    int forecast30d,
    double predictedDaysLeft,
    string urgency)
{
    return new SmartReorderItemDto
    {
        Id = item.Id,
        Name = item.Name,
        CurrentStock = item.Quantity,
        PredictedDaysLeft = predictedDaysLeft,
        ForecastedSales7d = forecast7d,
        ForecastedSales30d = forecast30d,
        SupplierName = item.Supplier?.Name ?? "N/A",
        Urgency = urgency
    };
}


    }
}
     
 