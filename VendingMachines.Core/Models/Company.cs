using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("companies")]
public partial class Company
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("contact_email")]
    [StringLength(50)]
    public string? ContactEmail { get; set; }

    [Column("contact_phone")]
    [StringLength(18)]
    public string? ContactPhone { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Company")]
    [JsonIgnore]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    [InverseProperty("Company")]
    [JsonIgnore]
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    [InverseProperty("Company")]
    [JsonIgnore]
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    [InverseProperty("Company")]
    [JsonIgnore]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
