using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArelScoreWebUI.Migrations
{
    /// <inheritdoc />
    public partial class EditProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupNo",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupNo",
                table: "Projects");
        }
    }
}
