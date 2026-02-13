using VendingMachines.API.DTOs.Bookings.Enums;

namespace VendingMachines.API.DTOs.Bookings
{
    public class BookingsRequest
    {
        public int? DeviceId { get; set; }
        public int? CompanyId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public OwnershipTypeEnum OwnershipType { get; set; }
        public bool? Insurance { get; set; }
        public decimal? MonthlyCost { get; set; }
        public decimal? AnnualCost { get; set; }
        public int? PaybackPeriod { get; set; }
        public BookingStatusEnum Status { get; set; }
    }
}
