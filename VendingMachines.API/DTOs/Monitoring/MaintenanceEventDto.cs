namespace VendingMachines.API.DTOs.Monitoring;

public class MaintenanceEventDto
{
    public DateTime Date { get; set; }
    public int DeviceId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Franchisee { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = string.Empty;
    public string TextColor { get; set; } = string.Empty;
}