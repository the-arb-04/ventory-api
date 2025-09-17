// In PurchaseOrder.cs
using System.ComponentModel.DataAnnotations.Schema; // Add this using statement
using System.Collections.Generic;
using System;

namespace Inventory_Tracker.Models
{
    public class PurchaseOrder
    {
        // Add the [ForeignKey] attribute here
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public PurchaseOrder()
        {
            PurchaseOrderItems = new HashSet<PurchaseOrderItem>();
        }

        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDate { get; set; }

        public virtual Supplier Supplier { get; set; }
        public virtual ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    }
}