namespace VendingMachines.API.DTOs.Devices
{
    public class DeviceUpdateDto
    {
        public int Id { get; set; }
        public int? DeviceModelId { get; set; }
        public int? CompanyId { get; set; }
        public int? ModemId { get; set; }
        public int? DeviceStatusId { get; set; }
        public DateOnly InstallationDate { get; set; }
        public DateOnly? LastServiceDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public LocationDto? Location { get; set; }
    }
}
