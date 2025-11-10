using VendingMachines.Core.Models;

namespace VendingMachines.API.DTOs.Monitoring
{
    public class NetworkStatusResponse
    {
        public int Active { get; set; }
        public int Inactive { get; set; }
        public double Efficiency { get; set; }
        public List<Device> Devices { get; set; }
        public decimal TotalMoney { get; set; }
    }
}
