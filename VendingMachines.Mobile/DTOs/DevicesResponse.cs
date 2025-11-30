namespace VendingMachines.Mobile.DTOs;

public class DevicesResponse
{
    public int TotalCount { get; set; }
    public List<DeviceDto> Items { get; set; } = new();
}