using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("device_types")]
[Index("Name", Name = "device_types_name_key", IsUnique = true)]
public partial class DeviceType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column("description")]
    [StringLength(255)]
    public string? Description { get; set; }

    [InverseProperty("DeviceType")]
    [JsonIgnore]
    public virtual ICollection<DeviceModel> DeviceModels { get; set; } = new List<DeviceModel>();
}
