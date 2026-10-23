using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DossierArchiveApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentToUsersAndEmployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "TB_Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IsSuperAdmin",
                table: "TB_Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "TB_Employees",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_Users_DepartmentId",
                table: "TB_Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TB_Employees_DepartmentId",
                table: "TB_Employees",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TB_Employees_Departments_DepartmentId",
                table: "TB_Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TB_Users_Departments_DepartmentId",
                table: "TB_Users",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TB_Employees_Departments_DepartmentId",
                table: "TB_Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_TB_Users_Departments_DepartmentId",
                table: "TB_Users");

            migrationBuilder.DropIndex(
                name: "IX_TB_Users_DepartmentId",
                table: "TB_Users");

            migrationBuilder.DropIndex(
                name: "IX_TB_Employees_DepartmentId",
                table: "TB_Employees");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "TB_Users");

            migrationBuilder.DropColumn(
                name: "IsSuperAdmin",
                table: "TB_Users");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "TB_Employees");
        }
    }
}
