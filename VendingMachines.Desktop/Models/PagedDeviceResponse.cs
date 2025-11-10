using VendingMachines.API.DTOs.Devices;

namespace VendingMachines.Desktop.Models
{
    public class PagedDeviceResponse
    {
        public int TotalCount { get; set; }
        public List<DeviceListItem> Items { get; set; }
    }

}
