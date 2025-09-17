using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using InventoryTracker.DTOs;
using Inventory_Tracker.Models;
using Microsoft.Extensions.Configuration;

namespace InventoryTracker.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IDashboardService _dashboardService;
        

        public AiService(HttpClient httpClient, IDashboardService dashboardService)
        {
            _httpClient = httpClient;
            _dashboardService = dashboardService; // Assign the injected service
        }

        public async Task<string> GenerateProductDescriptionAsync(string productName)
        {
            // This is the prompt we will send to the AI.
            // It's crafted to be specific to a shop in your location.
            var prompt = $"Generate a short, engaging product description for a local shop in Pimpri-Chinchwad, India. The product is: '{productName}'. Keep it under 40 words.";

            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // IMPORTANT: Replace with your actual Gemini API URL and Key
            // For now, this uses a placeholder.
            var apiKey = "AIzaSyCuFbYFhQJ2ffsWNtac6IY28AoNP7yO3K4";
            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-05-20:generateContent?key={apiKey}";

            try
            {
                var response = await _httpClient.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();

                // This part carefully extracts the text from the AI's JSON response
                using (var jsonDoc = JsonDocument.Parse(responseBody))
                {
                    var text = jsonDoc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    return text?.Trim() ?? "Could not generate a description.";
                }
            }
            catch (Exception ex)
            {
                // Log the actual error for debugging
                Console.WriteLine($"Error calling Gemini API: {ex.Message}");
                return "Error: Could not generate description.";
            }
        }
        
        // Add this new method inside your AiService class
        public async Task<AiSummaryResponseDto> GetBusinessSummaryAsync(string userId)
        {
            // 1. Gather all the data, now including forecast and reorder info
            var snapshot = await _dashboardService.GetTodaySnapshotAsync(userId);
            var topItems = await _dashboardService.GetTopSellingItemsAsync(userId, "30d");
            var itemsToReorder = await _dashboardService.GetItemsToReorderAsync(userId);
            var salesForecast = await _dashboardService.GetOverallSalesForecastAsync(userId, 7);

            // 2. Create a new, more detailed prompt for the AI
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("You are a business analyst for a small shop. Analyze the following data and respond with a JSON object ONLY. The JSON object must have two keys: 'summary' (a string with a brief, insightful summary in Markdown format) and 'actions' (an array of objects).");
            promptBuilder.AppendLine("For the 'actions' array, look at the 'Items to Reorder' list. For each item, create an action object with 'type': 'ORDER_ITEM', 'label': 'Order [Item Name]', and 'targetEntity': '[Item Name]'. Do not suggest any other action types.");

            promptBuilder.AppendLine("\n--- DATA ---");
            promptBuilder.AppendLine($"Today's Revenue: {snapshot.Revenue:F2} INR");
            promptBuilder.AppendLine($"Today's Profit: {snapshot.GrossProfit:F2} INR");
            
            promptBuilder.AppendLine("\n--- Top Selling Items (Last 30 Days) ---");
            foreach (var item in topItems) { promptBuilder.AppendLine($"- {item.Name}"); }

            promptBuilder.AppendLine("\n--- Items to Reorder (Low Stock) ---");
            foreach (var item in itemsToReorder) { promptBuilder.AppendLine($"- {item.Name} (Current: {item.CurrentQuantity}, Min: {item.MinQuantity})"); }

            if (salesForecast != null && salesForecast.Any())
            {
                var totalForecastedSales = salesForecast.Sum(f => (decimal)f.Yhat);
                promptBuilder.AppendLine($"\n--- Sales Forecast (Next 7 Days) ---");
                promptBuilder.AppendLine($"Projected Revenue: {totalForecastedSales:F2} INR");
            }
            
            promptBuilder.AppendLine("\nRespond ONLY with a valid JSON object based on these instructions.");

            // 3. Call the Gemini API
            var payload = new { contents = new[] { new { parts = new[] { new { text = promptBuilder.ToString() } } } } };
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var apiKey = "AIzaSyCuFbYFhQJ2ffsWNtac6IY28AoNP7yO3K4"; // Get key from configuration
            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={apiKey}";

            try
            {
                var response = await _httpClient.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                // 4. Parse the response and deserialize the JSON into our DTO
                using (var jsonDoc = JsonDocument.Parse(responseBody))
                {
                    var text = jsonDoc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                    
                    var cleanJson = text.Trim().Replace("```json", "").Replace("```", "");

                    return JsonSerializer.Deserialize<AiSummaryResponseDto>(cleanJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing Gemini summary: {ex.Message}");
                // Return a default error object that matches the expected structure
                return new AiSummaryResponseDto { Summary = "Error: Could not generate a business summary.", Actions = new List<AiActionDto>() };
            }
        }
    }
}