using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DossierArchiveApp.Models;

[Table("TB_SystemSettings")]
public class SystemSetting
{
    [Key]
    [Column("SettingID")]
    public int SettingId { get; set; } = 1;

    [Column("BaseFolderPath")]
    public string BaseFolderPath { get; set; } = string.Empty;

    // الهوية المؤسسية
    [Column("DirectorateName")]
    public string DirectorateName { get; set; } = "المديرية العامة";

    [Column("DepartmentName")]
    public string DepartmentName { get; set; } = "شعبة تكنولوجيا المعلومات";

    [Column("LogoPath")]
    public string LogoPath { get; set; } = string.Empty;

    [Column("FontFamily")]
    public string FontFamily { get; set; } = "Dubai";

    // إعدادات الماسح الضوئي (Hardware & Quality)
    [Column("DefaultScannerName")]
    public string DefaultScannerName { get; set; } = string.Empty;

    [Column("ScanSource")]
    public string ScanSource { get; set; } = "Feeder"; // Feeder أو Flatbed

    [Column("DuplexScan")]
    public int DuplexScan { get; set; } = 0; // مسح الوجهين

    [Column("ScannerResolutionDPI")]
    public int ScannerResolutionDPI { get; set; } = 200;

    [Column("ScannerColorMode")]
    public string ScannerColorMode { get; set; } = "Grayscale";

    [Column("PdfCompressionLevel")]
    public string PdfCompressionLevel { get; set; } = "Medium";

    // باقي الإعدادات
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