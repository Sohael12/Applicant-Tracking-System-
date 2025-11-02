using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stageproject_ATS_AP2025Q2.Migrations
{
    /// <inheritdoc />
    public partial class addnewfieldtoappliction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vacancies_AspNetUsers_AppUserId",
                table: "Vacancies");

            migrationBuilder.DropIndex(
                name: "IX_Vacancies_AppUserId",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "Vacancies");

            migrationBuilder.AddColumn<string>(
                name: "StatusHistory",
                table: "Applications",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusHistory",
                table: "Applications");

            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "Vacancies",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_AppUserId",
                table: "Vacancies",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vacancies_AspNetUsers_AppUserId",
                table: "Vacancies",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
