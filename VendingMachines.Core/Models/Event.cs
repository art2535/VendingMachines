using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("events")]
public partial class Event
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("device_id")]
    public int? DeviceId { get; set; }

    [Column("event_type")]
    [StringLength(50)]
    public string EventType { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("date_time")]
    public DateTime? DateTime { get; set; }

    [Column("media_path")]
    public string? MediaPath { get; set; }

    [ForeignKey("DeviceId")]
    [InverseProperty("Events")]
    [JsonIgnore]
    public virtual Device? Device { get; set; }
}
