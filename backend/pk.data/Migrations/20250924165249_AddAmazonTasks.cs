using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pk.data.Migrations
{
    /// <inheritdoc />
    public partial class AddAmazonTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AmazonTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Site = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CategoriesJson = table.Column<string>(type: "jsonb", nullable: false),
                    LeaderboardsJson = table.Column<string>(type: "jsonb", nullable: false),
                    PriceRangeJson = table.Column<string>(type: "jsonb", nullable: false),
                    KeywordsJson = table.Column<string>(type: "jsonb", nullable: false),
                    FiltersJson = table.Column<string>(type: "jsonb", nullable: false),
                    ScheduleJson = table.Column<string>(type: "jsonb", nullable: false),
                    LimitsJson = table.Column<string>(type: "jsonb", nullable: false),
                    ProxyPolicy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    LlmSummary = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmazonTasks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AmazonTasks_Name",
                table: "AmazonTasks",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmazonTasks");
        }
    }
}
