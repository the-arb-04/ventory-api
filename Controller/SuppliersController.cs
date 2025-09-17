using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventory_Tracker.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity; // CHANGE: Add this using

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly InventoryDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager; // CHANGE: Add UserManager

    // CHANGE: Inject UserManager in the constructor
    public SuppliersController(InventoryDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /api/suppliers
    [HttpGet]
    public async Task<IActionResult> GetAllSuppliers()
    {
        // CHANGE: Implement the new shop-level ownership pattern
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();
        var ownerId = currentUser.OwnerId ?? currentUser.Id;

        var suppliers = await _context.Suppliers
                                      .Where(s => s.UserId == ownerId)
                                      .ToListAsync();
        return Ok(suppliers);
    }
    
    // GET: /api/suppliers/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSupplierById(int id)
    {
        // CHANGE: Implement the new shop-level ownership pattern
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();
        var ownerId = currentUser.OwnerId ?? currentUser.Id;

        var supplier = await _context.Suppliers
                                     .FirstOrDefaultAsync(s => s.Id == id && s.UserId == ownerId);
        if (supplier == null)
        {
            return NotFound();
        }
        return Ok(supplier);
    }

    // POST: /api/suppliers
    [HttpPost]
    public async Task<IActionResult> AddSupplier([FromBody] SupplierDto supplierDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // CHANGE: Implement the new shop-level ownership pattern
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();
        var ownerId = currentUser.OwnerId ?? currentUser.Id;

        var supplier = new Supplier
        {
            Name = supplierDto.Name,
            PhoneNumber = supplierDto.PhoneNumber,
            Website = supplierDto.Website,
            Email = supplierDto.Email,
            ContactPerson = supplierDto.ContactPerson,
            WhatsAppNumber = supplierDto.WhatsAppNumber,
            UserId = ownerId // CHANGE: Stamp with the owner's ID
        };

        await _context.Suppliers.AddAsync(supplier);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetSupplierById), new { id = supplier.Id }, supplier);
    }

    // PUT: /api/suppliers/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] SupplierDto supplierDto)
    {
        // CHANGE: Implement the new shop-level ownership pattern
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();
        var ownerId = currentUser.OwnerId ?? currentUser.Id;

        var supplierToUpdate = await _context.Suppliers
                                             .FirstOrDefaultAsync(s => s.Id == id && s.UserId == ownerId);
        if (supplierToUpdate == null)
        {
            return NotFound();
        }
        
        supplierToUpdate.Name = supplierDto.Name;
        supplierToUpdate.PhoneNumber = supplierDto.PhoneNumber;
        supplierToUpdate.Website = supplierDto.Website;
        supplierToUpdate.Email = supplierDto.Email;
        supplierToUpdate.ContactPerson = supplierDto.ContactPerson;
        supplierToUpdate.WhatsAppNumber = supplierDto.WhatsAppNumber; 

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: /api/suppliers/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        // CHANGE: Implement the new shop-level ownership pattern
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();
        var ownerId = currentUser.OwnerId ?? currentUser.Id;
        
        var supplierToDelete = await _context.Suppliers
                                             .FirstOrDefaultAsync(s => s.Id == id && s.UserId == ownerId);
        if (supplierToDelete == null)
        {
            return NotFound();
        }

        _context.Suppliers.Remove(supplierToDelete);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}