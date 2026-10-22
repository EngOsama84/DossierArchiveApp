using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DossierArchiveApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToDocumentEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "TB_Employees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "TB_Employees",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "TB_Documents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "TB_Documents",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "TB_DocumentEmployees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedAt",
                table: "TB_DocumentEmployees",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "TB_DocumentEmployees",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "TB_DocumentEmployees",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "TB_Employees");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TB_Employees");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "TB_Documents");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TB_Documents");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "TB_DocumentEmployees");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TB_DocumentEmployees");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TB_DocumentEmployees");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TB_DocumentEmployees");
        }
    }
}
