namespace VendingMachines.API.DTOs.Products
{
    public class ProductResponse
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? SalesPopularity { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
