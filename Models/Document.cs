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
    public string DocumentDate { get; set; } = string.Empty;

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
    public string CreatedAt { get; set; } = string.Empty;

    [Column("UpdatedAt")]
    public string UpdatedAt { get; set; } = string.Empty;

    [Column("CreatedBy")]
    public int? CreatedBy { get; set; }

    [Column("UpdatedBy")]
    public int? UpdatedBy { get; set; }

    [Column("IsDeleted")]
    public int IsDeleted { get; set; } = 0;
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
    public string CreatedAt { get; set; } = string.Empty;
}