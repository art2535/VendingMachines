namespace VendingMachines.API.DTOs.Monitoring;

public class MaintenanceEventResponse
{
    public string Month { get; set; } = string.Empty;
    public List<MaintenanceEventDto> Events { get; set; } = new();
}