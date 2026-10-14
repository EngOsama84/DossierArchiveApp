using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DossierArchiveApp.Models;

[Table("TB_SystemSettings")]
public class SystemSetting
{
    [Key]
    [Column("SettingID")]
    public int SettingId { get; set; }

    [Column("BaseFolderPath")]
    public string BaseFolderPath { get; set; } = string.Empty;

    [Column("EmployeeDisplayMode")]
    public string EmployeeDisplayMode { get; set; } = "Active";

    [Column("DefaultLanguage")]
    public string DefaultLanguage { get; set; } = "ar";

    [Column("BackupFolderPath")]
    public string BackupFolderPath { get; set; } = string.Empty;

    [Column("AutoBackupEnabled")]
    public int AutoBackupEnabled { get; set; } = 0;

    [Column("AutoBackupInterval")]
    public int AutoBackupInterval { get; set; } = 24;

    [Column("PDFViewerMode")]
    public string PDFViewerMode { get; set; } = "internal";

    [Column("MaxFileSize")]
    public int MaxFileSize { get; set; } = 52428800;

    [Column("DefaultSearchLimit")]
    public int DefaultSearchLimit { get; set; } = 200;

    [Column("KeepAuditLog")]
    public int KeepAuditLog { get; set; } = 1;
}

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
    public string CreatedAt { get; set; } = string.Empty;
}