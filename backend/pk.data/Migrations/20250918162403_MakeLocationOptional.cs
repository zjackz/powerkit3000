using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pk.data.Migrations
{
    /// <inheritdoc />
    public partial class MakeLocationOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KickstarterProjects_Locations_LocationId",
                table: "KickstarterProjects");

            migrationBuilder.AlterColumn<long>(
                name: "LocationId",
                table: "KickstarterProjects",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_KickstarterProjects_Locations_LocationId",
                table: "KickstarterProjects",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KickstarterProjects_Locations_LocationId",
                table: "KickstarterProjects");

            migrationBuilder.AlterColumn<long>(
                name: "LocationId",
                table: "KickstarterProjects",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_KickstarterProjects_Locations_LocationId",
                table: "KickstarterProjects",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
