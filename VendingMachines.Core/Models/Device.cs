using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("devices")]
[Index("CompanyId", Name = "idx_devices_company")]
[Index("DeviceStatusId", Name = "idx_devices_status")]
public partial class Device
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("device_model_id")]
    public int? DeviceModelId { get; set; }

    [Column("location_id")]
    public int? LocationId { get; set; }

    [Column("modem_id")]
    public int? ModemId { get; set; }

    [Column("device_status_id")]
    public int? DeviceStatusId { get; set; }

    [Column("company_id")]
    public int? CompanyId { get; set; }

    [Column("installation_date")]
    public DateOnly InstallationDate { get; set; }

    [Column("last_service_date")]
    public DateOnly? LastServiceDate { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Device")]
    [JsonIgnore]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    [ForeignKey("CompanyId")]
    [InverseProperty("Devices")]
    [JsonIgnore]
    public virtual Company? Company { get; set; }

    [ForeignKey("DeviceModelId")]
    [InverseProperty("Devices")]
    [JsonIgnore]
    public virtual DeviceModel? DeviceModel { get; set; }

    [ForeignKey("DeviceStatusId")]
    [InverseProperty("Devices")]
    [JsonIgnore]
    public virtual DeviceStatus? DeviceStatus { get; set; }

    [InverseProperty("Device")]
    [JsonIgnore]
    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    [InverseProperty("Device")]
    [JsonIgnore]
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    [ForeignKey("LocationId")]
    [InverseProperty("Devices")]
    [JsonIgnore]
    public virtual Location? Location { get; set; }

    [ForeignKey("ModemId")]
    [InverseProperty("Devices")]
    [JsonIgnore]
    public virtual Modem? Modem { get; set; }

    [InverseProperty("Device")]
    [JsonIgnore]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("Device")]
    [JsonIgnore]
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    [InverseProperty("Device")]
    [JsonIgnore]
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
