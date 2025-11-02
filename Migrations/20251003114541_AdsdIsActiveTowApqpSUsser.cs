using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stageproject_ATS_AP2025Q2.Migrations
{
    /// <inheritdoc />
    public partial class AdsdIsActiveTowApqpSUsser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyText",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Templates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyText",
                table: "Templates",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Templates",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
