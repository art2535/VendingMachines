namespace VendingMachines.API.DTOs.Devices
{
    public class DeviceStatusResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
    }
}
