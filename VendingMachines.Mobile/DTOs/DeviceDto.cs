using System.Text.Json.Serialization;

namespace VendingMachines.Mobile.DTOs;

public class DeviceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}