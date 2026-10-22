using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DossierArchiveApp.Models;

[Table("TB_Employees")]
public class Employee
{
    [Key]
    [Column("EmployeeID")]
    public int EmployeeId { get; set; }

    [Column("EmployeeName")]
    [Required]
    public string EmployeeName { get; set; } = string.Empty;

    [Column("FileNumber")]
    [Required]
    public string FileNumber { get; set; } = string.Empty;

    [Column("IsActive")]
    public int IsActive { get; set; } = 1;

    [Column("EmployeeFolderPath")]
    public string EmployeeFolderPath { get; set; } = string.Empty;

    [Column("CreatedAt")]
    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    [Column("UpdatedAt")]
    public DateTime? UpdatedAt { get; set; } = DateTime.Now;

    [Column("CreatedBy")]
    public int? CreatedBy { get; set; }

    [Column("UpdatedBy")]
    public int? UpdatedBy { get; set; }

    [Column("IsDeleted")]
    public int IsDeleted { get; set; } = 0;

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [Column("DeletedBy")]
    public int? DeletedBy { get; set; }

    [Column("DeleteReason")]
    public string? DeleteReason { get; set; }
    // خاصية مساعدة لواجهة المستخدم
    [NotMapped]
    public bool ActiveStatus
    {
        get => IsActive == 1;
        set => IsActive = value ? 1 : 0;
    }
}