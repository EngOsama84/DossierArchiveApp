using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DossierArchiveApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_AuditLog",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserID = table.Column<int>(type: "INTEGER", nullable: true),
                    ActionType = table.Column<string>(type: "TEXT", nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", nullable: true),
                    EntityID = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_AuditLog", x => x.LogID);
                });

            migrationBuilder.CreateTable(
                name: "TB_DocumentEmployees",
                columns: table => new
                {
                    DocumentEmployeeID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DocumentID = table.Column<int>(type: "INTEGER", nullable: false),
                    EmployeeID = table.Column<int>(type: "INTEGER", nullable: false),
                    EmployeeFilePath = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_DocumentEmployees", x => x.DocumentEmployeeID);
                });

            migrationBuilder.CreateTable(
                name: "TB_Documents",
                columns: table => new
                {
                    DocumentID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Subject = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentNumber = table.Column<string>(type: "TEXT", nullable: false),
                    DocumentDate = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalFileName = table.Column<string>(type: "TEXT", nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT", nullable: false),
                    FileExtension = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    FileHash = table.Column<string>(type: "TEXT", nullable: false),
                    MainCategoryID = table.Column<int>(type: "INTEGER", nullable: false),
                    PrimaryCategoryID = table.Column<int>(type: "INTEGER", nullable: false),
                    SecondaryCategoryID = table.Column<int>(type: "INTEGER", nullable: true),
                    TertiaryCategoryID = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<int>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_Documents", x => x.DocumentID);
                });

            migrationBuilder.CreateTable(
                name: "TB_Employees",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmployeeName = table.Column<string>(type: "TEXT", nullable: false),
                    FileNumber = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<int>(type: "INTEGER", nullable: false),
                    EmployeeFolderPath = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<int>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_Employees", x => x.EmployeeID);
                });

            migrationBuilder.CreateTable(
                name: "TB_MainCategories",
                columns: table => new
                {
                    MainCategoryID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_MainCategories", x => x.MainCategoryID);
                });

            migrationBuilder.CreateTable(
                name: "TB_PrimaryCategories",
                columns: table => new
                {
                    PrimaryCategoryID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MainCategoryID = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_PrimaryCategories", x => x.PrimaryCategoryID);
                });

            migrationBuilder.CreateTable(
                name: "TB_SecondaryCategories",
                columns: table => new
                {
                    SecondaryCategoryID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PrimaryCategoryID = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_SecondaryCategories", x => x.SecondaryCategoryID);
                });

            migrationBuilder.CreateTable(
                name: "TB_SystemSettings",
                columns: table => new
                {
                    SettingID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BaseFolderPath = table.Column<string>(type: "TEXT", nullable: false),
                    DirectorateName = table.Column<string>(type: "TEXT", nullable: false),
                    DepartmentName = table.Column<string>(type: "TEXT", nullable: false),
                    LogoPath = table.Column<string>(type: "TEXT", nullable: false),
                    FontFamily = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultScannerName = table.Column<string>(type: "TEXT", nullable: false),
                    ScanSource = table.Column<string>(type: "TEXT", nullable: false),
                    DuplexScan = table.Column<int>(type: "INTEGER", nullable: false),
                    ScannerResolutionDPI = table.Column<int>(type: "INTEGER", nullable: false),
                    ScannerColorMode = table.Column<string>(type: "TEXT", nullable: false),
                    PdfCompressionLevel = table.Column<string>(type: "TEXT", nullable: false),
                    EmployeeDisplayMode = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultLanguage = table.Column<string>(type: "TEXT", nullable: false),
                    BackupFolderPath = table.Column<string>(type: "TEXT", nullable: false),
                    AutoBackupEnabled = table.Column<int>(type: "INTEGER", nullable: false),
                    AutoBackupInterval = table.Column<int>(type: "INTEGER", nullable: false),
                    PDFViewerMode = table.Column<string>(type: "TEXT", nullable: false),
                    MaxFileSize = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultSearchLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    KeepAuditLog = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_SystemSettings", x => x.SettingID);
                });

            migrationBuilder.CreateTable(
                name: "TB_TertiaryCategories",
                columns: table => new
                {
                    TertiaryCategoryID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SecondaryCategoryID = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_TertiaryCategories", x => x.TertiaryCategoryID);
                });

            migrationBuilder.CreateTable(
                name: "TB_Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    IsAdmin = table.Column<int>(type: "INTEGER", nullable: false),
                    CanAdd = table.Column<int>(type: "INTEGER", nullable: false),
                    CanEdit = table.Column<int>(type: "INTEGER", nullable: false),
                    CanDelete = table.Column<int>(type: "INTEGER", nullable: false),
                    CanExport = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<int>(type: "INTEGER", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<string>(type: "TEXT", nullable: true),
                    LastLogin = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_Users", x => x.UserID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "TB_AuditLog",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentEmployees_DocumentId",
                table: "TB_DocumentEmployees",
                column: "DocumentID");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentEmployees_Employee_Document",
                table: "TB_DocumentEmployees",
                columns: new[] { "EmployeeID", "DocumentID" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Categories",
                table: "TB_Documents",
                columns: new[] { "MainCategoryID", "PrimaryCategoryID" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentDate",
                table: "TB_Documents",
                column: "DocumentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentNumber",
                table: "TB_Documents",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeName",
                table: "TB_Employees",
                column: "EmployeeName");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_FileNumber",
                table: "TB_Employees",
                column: "FileNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_AuditLog");

            migrationBuilder.DropTable(
                name: "TB_DocumentEmployees");

            migrationBuilder.DropTable(
                name: "TB_Documents");

            migrationBuilder.DropTable(
                name: "TB_Employees");

            migrationBuilder.DropTable(
                name: "TB_MainCategories");

            migrationBuilder.DropTable(
                name: "TB_PrimaryCategories");

            migrationBuilder.DropTable(
                name: "TB_SecondaryCategories");

            migrationBuilder.DropTable(
                name: "TB_SystemSettings");

            migrationBuilder.DropTable(
                name: "TB_TertiaryCategories");

            migrationBuilder.DropTable(
                name: "TB_Users");
        }
    }
}
