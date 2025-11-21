namespace VendingMachines.Mobile.DTOs;

public class DeviceRequest
{
    public int Id { get; set; }
    public string? DeviceModel { get; set; }
    public string? Location { get; set; }
    public string? Modem { get; set; }
    public string? DeviceStatus { get; set; }
    public string? Company { get; set; }

    public DateOnly? InstallationDate { get; set; }
    public DateOnly? LastServiceDate { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}