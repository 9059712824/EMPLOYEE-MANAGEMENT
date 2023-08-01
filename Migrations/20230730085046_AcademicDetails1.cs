using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMPLOYEE_MANAGEMENT.Migrations
{
    /// <inheritdoc />
    public partial class AcademicDetails1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstitutionName",
                table: "AcademicDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstitutionName",
                table: "AcademicDetails");
        }
    }
}
