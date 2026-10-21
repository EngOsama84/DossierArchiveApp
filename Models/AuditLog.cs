using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DossierArchiveApp.Models;

[Table("TB_AuditLog")]
public class AuditLog
{
    [Key]
    [Column("LogID")]
    public int LogId { get; set; }

    [Column("UserID")]
    public int? UserId { get; set; }

    [Column("ActionType")]
    public string ActionType { get; set; } = string.Empty;

    [Column("EntityType")]
    public string? EntityType { get; set; }

    [Column("EntityID")]
    public int? EntityId { get; set; }

    [Column("Description")]
    public string Description { get; set; } = string.Empty;

    [Column("CreatedAt")]
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
}