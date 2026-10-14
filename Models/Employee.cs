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
    public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    [Column("UpdatedAt")]
    public string UpdatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    [Column("CreatedBy")]
    public int? CreatedBy { get; set; }

    [Column("UpdatedBy")]
    public int? UpdatedBy { get; set; }

    // خاصية مساعدة لواجهة المستخدم
    [NotMapped]
    public bool ActiveStatus
    {
        get => IsActive == 1;
        set => IsActive = value ? 1 : 0;
    }
}