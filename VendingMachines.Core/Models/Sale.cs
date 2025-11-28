using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("sales")]
[Index("DeviceId", Name = "idx_sales_device")]
public partial class Sale
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("device_id")]
    public int? DeviceId { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Column("sale_date_time")]
    public DateTime? SaleDateTime { get; set; }

    [Column("payment_method_id")]
    public int? PaymentMethodId { get; set; }

    [ForeignKey("DeviceId")]
    [InverseProperty("Sales")]
    [JsonIgnore]
    public virtual Device? Device { get; set; }

    [ForeignKey("PaymentMethodId")]
    [InverseProperty("Sales")]
    [JsonIgnore]
    public virtual PaymentMethod? PaymentMethod { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("Sales")]
    [JsonIgnore]
    public virtual Product? Product { get; set; }
}
