using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("locations")]
public partial class Location
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("installation_address")]
    public string InstallationAddress { get; set; } = null!;

    [Column("place_description")]
    [StringLength(50)]
    public string PlaceDescription { get; set; } = null!;

    [InverseProperty("Location")]
    [JsonIgnore]
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
