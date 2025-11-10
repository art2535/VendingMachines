using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("modems")]
[Index("SerialNumber", Name = "modems_serial_number_key", IsUnique = true)]
public partial class Modem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("brand")]
    [StringLength(20)]
    public string Brand { get; set; } = null!;

    [Column("serial_number")]
    [StringLength(15)]
    public string SerialNumber { get; set; } = null!;

    [Column("provider")]
    [StringLength(50)]
    public string? Provider { get; set; }

    [Column("balance")]
    [Precision(10, 2)]
    public decimal? Balance { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Modem")]
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
