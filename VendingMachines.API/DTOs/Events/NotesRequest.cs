using VendingMachines.API.DTOs.Events.Enums;

namespace VendingMachines.API.DTOs.Events
{
    public class NotesRequest
    {
        public int? DeviceId { get; set; }
        public EventTypeEnum EventType { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? EventDate { get; set; }
    }
}
