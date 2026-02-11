namespace VendingMachines.API.DTOs.Devices
{
    public class ModemResponse
    {
        public string Brand { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}
