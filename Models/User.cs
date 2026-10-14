using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DossierArchiveApp.Models;

[Table("TB_Users")]
public class User
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [Column("UserName")]
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Column("PasswordHash")]
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("UserTypeID")]
    public int UserTypeId { get; set; }

    [Column("IsActive")]
    public int IsActive { get; set; } = 1;

    [Column("CreatedAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [Column("UpdatedAt")]
    public string UpdatedAt { get; set; } = string.Empty;

    [Column("LastLogin")]
    public string? LastLogin { get; set; }
}