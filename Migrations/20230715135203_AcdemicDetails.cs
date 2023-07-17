using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMPLOYEE_MANAGEMENT.Migrations
{
    /// <inheritdoc />
    public partial class AcdemicDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartmentHead",
                table: "Departments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AcademicDetails",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartYear = table.Column<double>(type: "float", nullable: false),
                    EndYear = table.Column<double>(type: "float", nullable: false),
                    proof = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicDetails", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_AcademicDetails_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Experience",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    proof = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experience", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Experience_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcademicDetails");

            migrationBuilder.DropTable(
                name: "Experience");

            migrationBuilder.DropColumn(
                name: "DepartmentHead",
                table: "Departments");
        }
    }
}
