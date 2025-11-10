namespace VendingMachines.DTOs.Events;

public class NotesRequest
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public DateTime? EventDate { get; set; }
    public DeviceRequest? Device { get; set; } = new();
}