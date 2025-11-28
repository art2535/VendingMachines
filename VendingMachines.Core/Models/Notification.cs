using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("notifications")]
[Index("DeviceId", Name = "idx_notifications_device")]
public partial class Notification
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("device_id")]
    public int? DeviceId { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("type")]
    [StringLength(20)]
    public string Type { get; set; } = null!;

    [Column("message")]
    public string Message { get; set; } = null!;

    [Column("priority")]
    public int? Priority { get; set; }

    [Column("date_time")]
    public DateTime? DateTime { get; set; }

    [Column("confirmed")]
    public bool? Confirmed { get; set; }

    [ForeignKey("DeviceId")]
    [InverseProperty("Notifications")]
    public virtual Device? Device { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Notifications")]
    [JsonIgnore]
    public virtual User? User { get; set; }
}
