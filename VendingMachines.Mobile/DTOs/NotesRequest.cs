namespace VendingMachines.Mobile.DTOs;

public class NotesRequest
{
    public int Id { get; set; }
    public string? EventType { get; set; }
    public string? Description { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime? EventDate { get; set; }
    public DeviceRequest? Device { get; set; } = new();
}