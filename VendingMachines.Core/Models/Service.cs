using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("services")]
public partial class Service
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("device_id")]
    public int? DeviceId { get; set; }

    [Column("service_date")]
    public DateOnly ServiceDate { get; set; }

    [Column("work_description")]
    public string? WorkDescription { get; set; }

    [Column("issues")]
    public string? Issues { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("DeviceId")]
    [InverseProperty("Services")]
    public virtual Device? Device { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Services")]
    public virtual User? User { get; set; }
}
