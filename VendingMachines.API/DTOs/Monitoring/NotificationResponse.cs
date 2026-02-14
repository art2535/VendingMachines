namespace VendingMachines.API.DTOs.Monitoring
{
    public class NotificationResponse
    {
        public int Id { get; set; }
        public int? DeviceId { get; set; }
        public int? UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? Priority { get; set; }
        public DateTime? DateTime { get; set; }
        public bool? Confirmed { get; set; }
    }
}
