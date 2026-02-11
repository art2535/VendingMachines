using VendingMachines.API.DTOs.Company;
namespace VendingMachines.API.DTOs.Contracts
{
    public class ContractResponse
    {
        public CompanyResponse Company { get; set; } = new();
        public string ContractNumber { get; set; } = string.Empty;
        public DateOnly SigningDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string ContactStatus { get; set; } = string.Empty;
        public byte[]? SignatureData { get; set; }
    }
}
