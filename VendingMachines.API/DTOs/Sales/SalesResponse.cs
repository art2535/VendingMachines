using VendingMachines.API.DTOs.Devices;
using VendingMachines.API.DTOs.Products;

namespace VendingMachines.API.DTOs.Sales
{
    public class SalesResponse
    {
        public DeviceResponse Device { get; set; } = new();
        public ProductResponse Product { get; set; } = new();
        public DateTime SaleDateTime { get; set; }
        public PaymentMethodResponse PaymentMethod { get; set; } = new();
    }
}
