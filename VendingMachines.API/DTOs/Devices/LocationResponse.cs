namespace VendingMachines.API.DTOs.Devices
{
    public class LocationResponse
    {
        public int Id { get; set; }
        public string? InstallationAddress { get; set; }
        public string? PlaceDescription { get; set; }
    }
}
