using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMPLOYEE_MANAGEMENT.Migrations
{
    /// <inheritdoc />
    public partial class AcademicDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "AcademicDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GradeType",
                table: "AcademicDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "AcademicDetails");

            migrationBuilder.DropColumn(
                name: "GradeType",
                table: "AcademicDetails");
        }
    }
}
