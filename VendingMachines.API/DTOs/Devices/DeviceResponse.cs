namespace VendingMachines.API.DTOs.Devices
{
    public class DeviceResponse
    {
        public ModelResponse Model { get; set; } = new();
        public LocationResponse Location { get; set; } = new();
        public ModemResponse Modem { get; set; } = new();
        public DeviceStatusResponse DeviceStatus { get; set; } = new();
        public DateOnly InstallationDate { get; set; }
        public DateOnly? LastServiceDate { get; set; }
    }
}
