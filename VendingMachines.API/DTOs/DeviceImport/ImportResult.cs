namespace VendingMachines.API.DTOs.DeviceImport;

public class ImportResult
{
    public bool Success { get; set; }
    public int ImportedCount { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}