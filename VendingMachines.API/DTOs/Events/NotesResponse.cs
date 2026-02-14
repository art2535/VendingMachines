using VendingMachines.API.DTOs.Devices;

namespace VendingMachines.API.DTOs.Events;

public class NotesResponse
{
    public int Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public DeviceResponse Device { get; set; } = new();
}