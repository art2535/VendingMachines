namespace VendingMachines.API.DTOs.Devices
{
    public class DeviceListItem
    {
        private int? _modemId;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string Company { get; set; }

        public int? ModemId
        {
            get => _modemId ?? -1;
            set => _modemId = value;
        }

        public string Address { get; set; }
        public string Place { get; set; }
        public DateOnly InstallationDate { get; set; }
        public string OperatingMode { get; set; }
        public string TimeZone { get; set; }
        public int? ServicePriority { get; set; }
        public string Matrix { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
