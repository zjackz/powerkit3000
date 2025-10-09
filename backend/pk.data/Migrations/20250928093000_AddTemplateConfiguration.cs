using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pk.data.Migrations
{
    /// <inheritdoc />
    public class AddTemplateConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TemplateConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ProjectName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ApiBaseUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ThemeColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AccessToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemplateDictionaryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateDictionaryEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateDictionaryEntries_Category_Key",
                table: "TemplateDictionaryEntries",
                columns: new[] { "Category", "Key" },
                unique: true);

            var seedTimestamp = new DateTime(2025, 9, 28, 0, 0, 0, DateTimeKind.Utc);

            migrationBuilder.InsertData(
                table: "TemplateConfigurations",
                columns: new[]
                {
                    "Id",
                    "ProjectName",
                    "LogoUrl",
                    "ContactEmail",
                    "ApiBaseUrl",
                    "ThemeColor",
                    "DisplayName",
                    "Email",
                    "AccessToken",
                    "CreatedAt",
                    "UpdatedAt"
                },
                values: new object[]
                {
                    1,
                    "PowerKit Template",
                    null,
                    null,
                    null,
                    "#177ddc",
                    "Owner",
                    "you@example.com",
                    null,
                    seedTimestamp,
                    seedTimestamp
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplateDictionaryEntries");

            migrationBuilder.DropTable(
                name: "TemplateConfigurations");
        }
    }
}
