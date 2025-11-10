namespace VendingMachines.Desktop.Models
{
    public class Summary
    {
        public decimal MoneyInTA { get; set; }
        public decimal ChangeInTA { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueYesterday { get; set; }
        public decimal CollectedToday { get; set; }
        public decimal CollectedYesterday { get; set; }
        public int ServicedToday { get; set; }
        public int ServicedYesterday { get; set; }
    }
}
