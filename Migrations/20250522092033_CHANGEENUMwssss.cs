using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stageproject_ATS_AP2025Q2.Migrations
{
    /// <inheritdoc />
    public partial class CHANGEENUMwssss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StatusHistories_ApplicationId",
                table: "StatusHistories",
                column: "ApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_StatusHistories_Applications_ApplicationId",
                table: "StatusHistories",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatusHistories_Applications_ApplicationId",
                table: "StatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_StatusHistories_ApplicationId",
                table: "StatusHistories");
        }
    }
}
