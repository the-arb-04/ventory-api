using Dapper;
using Npgsql;
using Inventory_Tracker.Models;

namespace InventoryTracker.Services
{
    public class SalesService : ISalesService
    {
        private readonly IConfiguration _configuration;

        public SalesService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // CHANGE: Method signature updated to accept user IDs
        public async Task CreateSaleAsync(CreateSaleDto saleDto, string userId, string userEmail)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // CHANGE: 1. Securely get the item, ensuring it belongs to the current user
                var item = await connection.QuerySingleOrDefaultAsync<Item>(
                    @"SELECT * FROM items WHERE id = @ItemId AND ""UserId"" = @UserId",
                    new { ItemId = saleDto.ItemId, UserId = userId },
                    transaction);

                if (item == null || item.quantity < saleDto.QuantitySold)
                {
                    throw new Exception("Item not found for this user or insufficient stock.");
                }

                // CHANGE: 2. Insert the new sale record with the owner's UserId
                var saleSql = @"
                    INSERT INTO sales (item_id, quantity_sold, sale_price_at_time_of_sale, cost_price_at_time_of_sale, ""UserId"")
                    VALUES (@ItemId, @QuantitySold, @SellingPrice, @CostPrice, @UserId);";

                await connection.ExecuteAsync(saleSql, new
                {
                    ItemId = saleDto.ItemId,
                    QuantitySold = saleDto.QuantitySold,
                    SellingPrice = item.sellingprice,
                    CostPrice = item.costprice,
                    UserId = userId // Stamp the sale with the owner's ID
                }, transaction);

                // 3. Update the stock (this is already secure because we verified ownership in step 1)
                var updateStockSql = @"UPDATE items SET quantity = quantity - @QuantitySold WHERE id = @ItemId AND ""UserId"" = @UserId;";
                await connection.ExecuteAsync(updateStockSql, new
                {
                    QuantitySold = saleDto.QuantitySold,
                    ItemId = saleDto.ItemId,
                    UserId = userId
                }, transaction);

                // CHANGE: 4. Insert a history record with the owner's UserId
                var historySql = @"
                    INSERT INTO itemhistories (itemid, transactiontype, quantitychanged, remarks, username, ""UserId"", timestamp)
                    VALUES (@ItemId, 'Stock Out', -@QuantitySold, 'Sold via application', @UserName, @UserId, NOW());";

                await connection.ExecuteAsync(historySql, new
                {
                    ItemId = saleDto.ItemId,
                    QuantitySold = saleDto.QuantitySold,
                    UserName = userEmail, // Use email for better logging
                    UserId = userId // Stamp the history record with the owner's ID
                }, transaction);

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        public async Task<IEnumerable<Sale>> GetSalesAsync(string userId)
{
    var connectionString = _configuration.GetConnectionString("DefaultConnection");
    await using var connection = new NpgsqlConnection(connectionString);
    
    // Securely select sales that belong only to the specified user's shop
    var sql = @"SELECT * FROM sales WHERE ""UserId"" = @UserId ORDER BY ""SaleTimestamp"" DESC;";
    
    return await connection.QueryAsync<Sale>(sql, new { UserId = userId });
}
    }
    
    // A simple class for Dapper to map item data
    public class Item {
        public int id { get; set; }
        public decimal sellingprice { get; set; }
        public decimal costprice { get; set; }
        public int quantity { get; set; }
    }
}
