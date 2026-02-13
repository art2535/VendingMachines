namespace VendingMachines.API.DTOs.Devices
{
    public class ModelResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DeviceTypeResponse DeviceType { get; set; } = new();
        public string Description { get; set; } = string.Empty;
    }
}
