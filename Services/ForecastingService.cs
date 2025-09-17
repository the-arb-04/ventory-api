using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Inventory_Tracker.Models;
using Inventory_Tracker.DTOs;

namespace Inventory_Tracker.Services
{
    public class ForecastingService
    {
        private readonly InventoryDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public ForecastingService(InventoryDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // CHANGE: Method signature now accepts userId for security
        public async Task<IEnumerable<ProphetForecastDto>?> GetForecastForItemAsync(int itemId, string userId, int horizon)
        {
            // CHANGE: 1. Securely get sales data, filtering by both ItemId and UserId
            var allSalesForItem = await _context.Sales
                .Where(s => s.ItemId == itemId && s.UserId == userId)
                .OrderBy(s => s.SaleTimestamp)
                .ToListAsync();

            // The rest of the logic remains the same, but now operates on secure, pre-filtered data
            var dailySales = allSalesForItem
                .GroupBy(s => s.SaleTimestamp.Date)
                .Select(g => new 
                {
                    saleTimestamp = g.Key,
                    quantitySold = g.Sum(s => s.QuantitySold)
                })
                .ToList();

            if (dailySales.Count < 10)
            {
                return null;
            }

            var client = _httpClientFactory.CreateClient();
            try
            {
                var payload = new { salesData = dailySales, horizon = horizon };
                var response = await client.PostAsJsonAsync("http://127.0.0.1:5000/forecast", payload);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<IEnumerable<ProphetForecastDto>>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error calling the forecast API: {ex.Message}");
                return null;
            }
        }
    }
}