using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("device_models")]
public partial class DeviceModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column("device_type_id")]
    public int? DeviceTypeId { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [ForeignKey("DeviceTypeId")]
    [InverseProperty("DeviceModels")]
    [JsonIgnore]
    public virtual DeviceType? DeviceType { get; set; }

    [InverseProperty("DeviceModel")]
    [JsonIgnore]
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
