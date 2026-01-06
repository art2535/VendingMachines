namespace VendingMachines.API.DTOs.DeviceImport;

public class DeviceImportDto
{
    public string ModelName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string InstallationDateStr { get; set; } = string.Empty;
    public string? StatusName { get; set; }
    public string? ModemSerial { get; set; }
}