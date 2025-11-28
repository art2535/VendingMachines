using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("contracts")]
[Index("ContractNumber", Name = "contracts_contract_number_key", IsUnique = true)]
public partial class Contract
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("company_id")]
    public int? CompanyId { get; set; }

    [Column("contract_number")]
    [StringLength(50)]
    public string ContractNumber { get; set; } = null!;

    [Column("signing_date")]
    public DateOnly SigningDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = null!;

    [Column("signature_data")]
    public byte[]? SignatureData { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("Contracts")]
    [JsonIgnore]
    public virtual Company? Company { get; set; }
}
