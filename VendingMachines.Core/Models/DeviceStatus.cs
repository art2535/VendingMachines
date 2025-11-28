using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("device_statuses")]
[Index("Name", Name = "device_statuses_name_key", IsUnique = true)]
public partial class DeviceStatus
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(20)]
    public string Name { get; set; } = null!;

    [Column("color_code")]
    [StringLength(7)]
    public string? ColorCode { get; set; }

    [InverseProperty("DeviceStatus")]
    [JsonIgnore]
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
