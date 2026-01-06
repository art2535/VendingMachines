using VendingMachines.API.DTOs.Monitoring;

namespace VendingMachines.Web.DTOs;

public class ApiResponse
{
    public string Month { get; set; } = string.Empty;
    public List<MaintenanceEventDto> Events { get; set; } = new();
}