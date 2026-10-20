using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DossierArchiveApp.Models;

[Table("TB_Documents")]
public class Document
{
    [Key]
    [Column("DocumentID")]
    public int DocumentId { get; set; }

    [Column("Subject")]
    public string Subject { get; set; } = string.Empty;

    [Column("DocumentNumber")]
    public string DocumentNumber { get; set; } = string.Empty;

    [Column("DocumentDate")]
    public DateTime? DocumentDate { get; set; }

    [Column("OriginalFileName")]
    public string OriginalFileName { get; set; } = string.Empty;

    [Column("StoredFileName")]
    public string StoredFileName { get; set; } = string.Empty;

    [Column("FileExtension")]
    public string FileExtension { get; set; } = string.Empty;

    [Column("FilePath")]
    public string FilePath { get; set; } = string.Empty;

    [Column("FileHash")]
    public string FileHash { get; set; } = string.Empty;

    [Column("MainCategoryID")]
    public int MainCategoryId { get; set; }

    [Column("PrimaryCategoryID")]
    public int PrimaryCategoryId { get; set; }

    [Column("SecondaryCategoryID")]
    public int? SecondaryCategoryId { get; set; }

    [Column("TertiaryCategoryID")]
    public int? TertiaryCategoryId { get; set; }

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
}

[Table("TB_DocumentEmployees")]
public class DocumentEmployee
{
    [Key]
    [Column("DocumentEmployeeID")]
    public int DocumentEmployeeId { get; set; }

    [Column("DocumentID")]
    public int DocumentId { get; set; }

    [Column("EmployeeID")]
    public int EmployeeId { get; set; }

    [Column("EmployeeFilePath")]
    public string EmployeeFilePath { get; set; } = string.Empty;

    [Column("CreatedAt")]
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
}