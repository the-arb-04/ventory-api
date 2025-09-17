using System;

namespace InventoryTracker.DTOs
{
    public class GenerateDescriptionRequest
    {
        public string ProductName { get; set; }
    }

    public class LatestSummaryDto
    {
        public string SummaryText { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class AiActionDto
    {
        public string Type { get; set; }
        public string Label { get; set; }
        public string TargetEntity { get; set; }
    }

    // Represents the entire structured response from the AI
    public class AiSummaryResponseDto
    {
        public string Summary { get; set; }
        public List<AiActionDto> Actions { get; set; }
    }
}
