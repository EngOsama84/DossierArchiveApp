using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DossierArchiveApp.Models;

[Table("TB_MainCategories")]
public class MainCategory
{
    [Key]
    [Column("MainCategoryID")]
    public int MainCategoryId { get; set; }

    [Column("Name")]
    [Required]
    public string Name { get; set; } = string.Empty;
}

[Table("TB_PrimaryCategories")]
public class PrimaryCategory
{
    [Key]
    [Column("PrimaryCategoryID")]
    public int PrimaryCategoryId { get; set; }

    [Column("MainCategoryID")]
    public int MainCategoryId { get; set; }

    [Column("Name")]
    [Required]
    public string Name { get; set; } = string.Empty;
}

[Table("TB_SecondaryCategories")]
public class SecondaryCategory
{
    [Key]
    [Column("SecondaryCategoryID")]
    public int SecondaryCategoryId { get; set; }

    [Column("PrimaryCategoryID")]
    public int PrimaryCategoryId { get; set; }

    [Column("Name")]
    [Required]
    public string Name { get; set; } = string.Empty;
}

[Table("TB_TertiaryCategories")]
public class TertiaryCategory
{
    [Key]
    [Column("TertiaryCategoryID")]
    public int TertiaryCategoryId { get; set; }

    [Column("SecondaryCategoryID")]
    public int SecondaryCategoryId { get; set; }

    [Column("Name")]
    [Required]
    public string Name { get; set; } = string.Empty;
}