namespace VendingMachines.DTOs.Events;

public class DeviceRequest
{
    public int Id { get; set; }
    public string DeviceModel { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Modem { get; set; } = string.Empty;
    public string DeviceStatus { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public DateOnly? InstallationDate { get; set; }
    public DateOnly? LastServiceDate { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}