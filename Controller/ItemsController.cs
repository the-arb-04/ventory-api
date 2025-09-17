using Microsoft.AspNetCore.Mvc;
using Inventory_Tracker.Models;
using Inventory_Tracker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Inventory_Tracker.DTOs;
using Microsoft.AspNetCore.Identity; // CHANGE: Add this
using Microsoft.Extensions.Logging;   // CHANGE: Add this if not present
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Inventory_Tracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IItemHistoryService _historyService;
        private readonly ILogger<ItemsController> _logger;
        private readonly InventoryDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // CHANGE: Add UserManager

        // CHANGE: Inject UserManager
        public ItemsController(
            IItemService itemService,
            IItemHistoryService historyService,
            ILogger<ItemsController> logger,
            InventoryDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _itemService = itemService;
            _historyService = historyService;
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems()
        {
            // CHANGE: Implement the new shop-level ownership pattern
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;

            var items = await _itemService.GetAllItemsAsync(ownerId);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Item>> GetItem(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;

            var item = await _itemService.GetItemByIdAsync(id, ownerId);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        [HttpGet("{id}/history")]
        public async Task<ActionResult<IEnumerable<ItemHistory>>> GetItemHistory(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;

            var itemExists = await _itemService.GetItemByIdAsync(id, ownerId);
            if (itemExists == null)
            {
                return NotFound("Item not found or you do not have permission to view its history.");
            }

            var history = await _historyService.GetHistoryByItemIdAsync(id, ownerId);
            return Ok(history);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Item>> CreateItem([FromBody] CreateItemDto itemDto)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;

            var item = new Item
            {
                // ... (mapping properties from DTO)
                Name = itemDto.Name,
                SellingPrice = itemDto.SellingPrice,
                CostPrice = itemDto.CostPrice,
                Quantity = itemDto.Quantity,
                MinQuantity = itemDto.MinQuantity,
                Sku = itemDto.Sku,
                Barcode = itemDto.Barcode,
                Description = itemDto.Description,
                Brand = itemDto.Brand,
                CategoryId = itemDto.CategoryId,   // <-- This is the main fix for your bug
                SupplierId = itemDto.SupplierId,
                TaxPercentage = itemDto.TaxPercentage,
                IsActive = itemDto.IsActive,
                UserId = ownerId // CHANGE: Stamp with the owner's ID
            };

            var createdItem = await _itemService.CreateItemAsync(item);
            return CreatedAtAction(nameof(GetItem), new { id = createdItem.Id }, createdItem);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] Item item) // Note: Consider using a DTO here too
        {
            if (id != item.Id) return BadRequest("Item ID mismatch.");
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;
            var updatedBy = currentUser.Email;
            
            var success = await _itemService.UpdateItemWithHistoryAsync(item, ownerId, updatedBy);

            if (!success) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;

            var success = await _itemService.DeleteItemAsync(id, ownerId);

            if (!success) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}/sales")]
        public async Task<ActionResult<IEnumerable<Sale>>> GetItemSales(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();
            var ownerId = currentUser.OwnerId ?? currentUser.Id;

            var itemExists = await _context.Items.AnyAsync(i => i.Id == id && i.UserId == ownerId);
            if (!itemExists)
            {
                return NotFound("Item not found or you do not have permission to view its sales.");
            }
            
            var sales = await _context.Sales
                .Where(s => s.ItemId == id && s.UserId == ownerId)
                .OrderBy(s => s.SaleTimestamp)
                .ToListAsync();

            return Ok(sales);
        }
    }
}