// src/DTOs/CreatePurchaseOrderDto.cs

using System.Collections.Generic;

namespace Inventory_Tracker.DTOs
{
    public class CreatePurchaseOrderDto
    {
        public int SupplierId { get; set; }
        public List<PurchaseOrderItemDto> Items { get; set; } = new List<PurchaseOrderItemDto>();
    }
}