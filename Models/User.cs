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

    [Column("FullName")]
    public string FullName { get; set; } = string.Empty;

    [Column("PasswordHash")]
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    // --- الصلاحيات المباشرة (0 = لا، 1 = نعم) ---
    [Column("IsAdmin")]
    public int IsAdmin { get; set; } = 0;

    [Column("CanAdd")]
    public int CanAdd { get; set; } = 0;

    [Column("CanEdit")]
    public int CanEdit { get; set; } = 0;

    [Column("CanDelete")]
    public int CanDelete { get; set; } = 0;

    [Column("CanExport")]
    public int CanExport { get; set; } = 0;

    // --- حالة الحساب والأمان الإجباري ---
    [Column("IsActive")]
    public int IsActive { get; set; } = 1;

    [Column("MustChangePassword")]
    public bool MustChangePassword { get; set; } = false;

    // --- حقول التواريخ القياسية لضمان التوافق التام مع SQL Server و SQLite ---
    [Column("CreatedAt")]
    public DateTime? CreatedAt { get; set; }

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [Column("LastLogin")]
    public DateTime? LastLogin { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    // لتحديد ما إذا كان المستخدم مديراً عاماً يرى كل الشُعب (مثل حساب admin الافتراضي)
    public int IsSuperAdmin { get; set; } = 0;
}