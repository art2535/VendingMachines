using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("payment_methods")]
[Index("Name", Name = "payment_methods_name_key", IsUnique = true)]
public partial class PaymentMethod
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; } = null!;

    [InverseProperty("PaymentMethod")]
    [JsonIgnore]
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
