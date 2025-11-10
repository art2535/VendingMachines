using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("inventory")]
[Index("DeviceId", Name = "idx_inventory_device")]
[Index("DeviceId", "ProductId", Name = "inventory_device_id_product_id_key", IsUnique = true)]
public partial class Inventory
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("device_id")]
    public int? DeviceId { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Column("quantity")]
    public long Quantity { get; set; }

    [Column("minimum_stock")]
    public long MinimumStock { get; set; }

    [ForeignKey("DeviceId")]
    [InverseProperty("Inventories")]
    public virtual Device? Device { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("Inventories")]
    public virtual Product? Product { get; set; }
}
