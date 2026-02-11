using VendingMachines.API.DTOs.Company;
using VendingMachines.API.DTOs.Devices;

namespace VendingMachines.API.DTOs.Bookings
{
    public class BookingsResponse
    {
        public DeviceResponse Device { get; set; } = new();
        public CompanyResponse Company { get; set; } = new();
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string OwnershipType { get; set; } = string.Empty;
        public bool? Insurance { get; set; }
        public decimal? MonthlyCost { get; set; }
        public decimal? AnnualCost { get; set; }
        public int? PaybackPeriod { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
