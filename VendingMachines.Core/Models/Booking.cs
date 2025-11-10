using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("bookings")]
public partial class Booking
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("device_id")]
    public int? DeviceId { get; set; }

    [Column("company_id")]
    public int? CompanyId { get; set; }

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("ownership_type")]
    [StringLength(20)]
    public string OwnershipType { get; set; } = null!;

    [Column("insurance")]
    public bool? Insurance { get; set; }

    [Column("monthly_cost")]
    [Precision(10, 2)]
    public decimal? MonthlyCost { get; set; }

    [Column("annual_cost")]
    [Precision(10, 2)]
    public decimal? AnnualCost { get; set; }

    [Column("payback_period")]
    public int? PaybackPeriod { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string? Status { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("Bookings")]
    public virtual Company? Company { get; set; }

    [ForeignKey("DeviceId")]
    [InverseProperty("Bookings")]
    public virtual Device? Device { get; set; }
}
