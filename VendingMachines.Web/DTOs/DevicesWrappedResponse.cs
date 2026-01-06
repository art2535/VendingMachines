using VendingMachines.API.DTOs.Devices;

namespace VendingMachines.Web.DTOs;

public class DevicesWrappedResponse
{
    public int TotalCount { get; set; }
    public List<DeviceListItem> Items { get; set; } = new();
}