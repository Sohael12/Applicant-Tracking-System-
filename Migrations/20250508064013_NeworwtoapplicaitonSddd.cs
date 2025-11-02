using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stageproject_ATS_AP2025Q2.Migrations
{
    /// <inheritdoc />
    public partial class NeworwtoapplicaitonSddd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApprovedPersonFullName",
                table: "InterviewNotes",
                newName: "FullName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "InterviewNotes",
                newName: "ApprovedPersonFullName");
        }
    }
}
