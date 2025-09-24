using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace powerkit3000.data.Migrations
{
    /// <inheritdoc />
    public partial class AddChineseFieldsToKickstarterProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlurbCn",
                table: "KickstarterProjects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameCn",
                table: "KickstarterProjects",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlurbCn",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "NameCn",
                table: "KickstarterProjects");
        }
    }
}
