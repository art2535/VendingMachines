using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace VendingMachines.Core.Models;

[Table("users")]
[Index("Email", Name = "users_email_key", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("last_name")]
    [StringLength(50)]
    public string LastName { get; set; } = null!;

    [Column("first_name")]
    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [Column("middle_name")]
    [StringLength(50)]
    public string? MiddleName { get; set; }

    [Column("email")]
    [StringLength(50)]
    public string Email { get; set; } = null!;

    [Column("phone")]
    [StringLength(18)]
    public string? Phone { get; set; }

    [Column("hashed_password")]
    [StringLength(255)]
    public string HashedPassword { get; set; } = null!;

    [Column("role_id")]
    public int? RoleId { get; set; }

    [Column("company_id")]
    public int? CompanyId { get; set; }

    [Column("language")]
    [StringLength(5)]
    public string? Language { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("Users")]
    [JsonIgnore]
    public virtual Company? Company { get; set; }

    [InverseProperty("User")]
    [JsonIgnore]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    [JsonIgnore]
    public virtual Role? Role { get; set; }

    [InverseProperty("User")]
    [JsonIgnore]
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
