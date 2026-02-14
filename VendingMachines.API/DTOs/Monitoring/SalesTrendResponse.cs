namespace VendingMachines.API.DTOs.Monitoring
{
    public class SalesTrendResponse
    {
        public string Date { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }
}
