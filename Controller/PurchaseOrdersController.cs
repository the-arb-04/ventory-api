using Microsoft.AspNetCore.Mvc;
using Inventory_Tracker.Models;
using Inventory_Tracker.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims; // CHANGE: Add this using
using Microsoft.AspNetCore.Identity;

namespace Inventory_Tracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // CHANGE: Secure the entire controller
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly InventoryDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PurchaseOrdersController(InventoryDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        [HttpGet("items")]
        public async Task<IActionResult> GetAllOrderItems()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id; // CHANGE: Get user ID

            // CHANGE: Filter by the UserId on the parent PurchaseOrder
            var orderItems = await _context.PurchaseOrderItems
                .Where(poi => poi.PurchaseOrder.UserId == ownerId)
                .Include(poi => poi.Item)
                .Include(poi => poi.PurchaseOrder)
                    .ThenInclude(po => po.Supplier)
                .OrderByDescending(poi => poi.PurchaseOrder.OrderDate)
                .ToListAsync();

            // ... (rest of the method is fine)
            var result = orderItems.Select(poi => new {
                id = poi.Id,
                purchaseOrderId = poi.PurchaseOrderId,
                itemName = poi.Item.Name,
                supplierName = poi.PurchaseOrder.Supplier.Name,
                quantityOrdered = poi.QuantityOrdered,
                orderDate = poi.PurchaseOrder.OrderDate,
                status = poi.PurchaseOrder.Status
            });

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPurchaseOrders()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
            
            // CHANGE: Add .Where() clause to filter by owner
            var purchaseOrders = await _context.PurchaseOrders
                .Where(po => po.UserId == ownerId)
                .Include(po => po.Supplier)
                .Include(po => po.PurchaseOrderItems)
                    .ThenInclude(poi => poi.Item)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();

            return Ok(purchaseOrders);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto orderDto)
        {
            if (orderDto == null || !orderDto.Items.Any()) return BadRequest("Invalid order data.");

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;

            // CHANGE: Advanced security check to verify ownership of supplier and all items
            var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == orderDto.SupplierId && s.UserId == ownerId);
            if (!supplierExists) return BadRequest("Supplier not found for this user.");

            var itemIdsInOrder = orderDto.Items.Select(i => i.ItemId).ToList();
            var validUserItemCount = await _context.Items.CountAsync(i => itemIdsInOrder.Contains(i.Id) && i.UserId == ownerId);
            if (validUserItemCount != itemIdsInOrder.Count) return BadRequest("One or more items do not exist or belong to this user.");
            
            var purchaseOrder = new PurchaseOrder
            {
                SupplierId = orderDto.SupplierId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                UserId = ownerId // CHANGE: Stamp the order with the owner's ID
            };

            // ... (rest of the method is fine) ...
            foreach (var itemDto in orderDto.Items)
            {
                purchaseOrder.PurchaseOrderItems.Add(new PurchaseOrderItem
                {
                    ItemId = itemDto.ItemId,
                    QuantityOrdered = itemDto.QuantityOrdered
                });
            }
            await _context.PurchaseOrders.AddAsync(purchaseOrder);
await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPurchaseOrder), new { id = purchaseOrder.Id }, purchaseOrder);
        }
        
        [HttpPut("{id}/receive")]
        public async Task<IActionResult> MarkAsReceived(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // CHANGE: Get user ID

                // CHANGE: Find the PO ensuring it belongs to the current user
                var purchaseOrder = await _context.PurchaseOrders
                    .Include(po => po.PurchaseOrderItems)
                    .FirstOrDefaultAsync(po => po.Id == id && po.UserId == userId);

                if (purchaseOrder == null) return NotFound("Purchase Order not found.");
                if (purchaseOrder.Status == "Received") return BadRequest("This order has already been received.");
                
                purchaseOrder.Status = "Received";
                // ... (rest of the logic is fine) ...
                foreach (var orderItem in purchaseOrder.PurchaseOrderItems)
                {
                    // This is now also secure because we already verified the PO belongs to the user
                    var itemToUpdate = await _context.Items.FindAsync(orderItem.ItemId);
                    if (itemToUpdate != null)
                    {
                        itemToUpdate.Quantity += orderItem.QuantityOrdered;
                    }
                }
                await _context.SaveChangesAsync();
await transaction.CommitAsync();
                return Ok(new { message = "Order marked as received and stock updated." });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPurchaseOrder(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;


            // CHANGE: Find the PO ensuring it belongs to the current user
            var purchaseOrder = await _context.PurchaseOrders
                .Include(po => po.PurchaseOrderItems)
                    .ThenInclude(poi => poi.Item)
                .Include(po => po.Supplier)
                .FirstOrDefaultAsync(po => po.Id == id && po.UserId == ownerId);

            if (purchaseOrder == null) return NotFound();

            return Ok(purchaseOrder);
        }
    }
}