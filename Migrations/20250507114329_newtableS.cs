using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stageproject_ATS_AP2025Q2.Migrations
{
    /// <inheritdoc />
    public partial class newtableS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Applications",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Applications",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Applications");
        }
    }
}
