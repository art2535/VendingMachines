namespace VendingMachines.API.DTOs.Devices
{
    public class DeviceRequest
    {
        public int? DeviceModelId { get; set; }
        public int? LocationId { get; set; }
        public int? ModemId { get; set; }
        public int? DeviceStatusId { get; set; }
        public int? CompanyId { get; set; }
        public DateOnly InstallationDate { get; set; }
        public DateOnly? LastServiceDate { get; set; }
    }
}
