namespace VendingMachines.API.DTOs.DeviceImport;

public class DeviceImportDto
{
    public string ModelName { get; set; } = string.Empty;
    public string? ModelDescription { get; set; }

    public string? DeviceTypeName { get; set; }
    public string? DeviceTypeDescription { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyContactEmail { get; set; }
    public string? CompanyContactPhone { get; set; }
    public string? CompanyAddress { get; set; }

    public string Address { get; set; } = string.Empty;
    public string? PlaceDescription { get; set; }

    public string InstallationDate { get; set; } = string.Empty;
    public string? LastServiceDate { get; set; }

    public string? StatusName { get; set; }
    public string? ModemSerial { get; set; }
}