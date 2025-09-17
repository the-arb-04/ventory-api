// In a new file or your DTOs folder
public class SupplierDto
{
    public string Name { get; set; }
    public string? PhoneNumber { get; set; } // string? makes it optional
    public string? WhatsAppNumber { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
}