// src/Models/PurchaseOrderItem.cs

namespace Inventory_Tracker.Models
{
    public class PurchaseOrderItem
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public int ItemId { get; set; }
        public int QuantityOrdered { get; set; }

        // Navigation Properties
        public virtual PurchaseOrder PurchaseOrder { get; set; }
        public virtual Item Item { get; set; }
    }
}