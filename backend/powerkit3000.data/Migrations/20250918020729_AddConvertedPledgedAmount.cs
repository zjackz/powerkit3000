using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace powerkit3000.data.Migrations
{
    /// <inheritdoc />
    public partial class AddConvertedPledgedAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Locations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Locations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Locations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayableName",
                table: "Locations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Locations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Urls",
                table: "KickstarterProjects",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "KickstarterProjects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "KickstarterProjects",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "KickstarterProjects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "KickstarterProjects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "KickstarterProjects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Blurb",
                table: "KickstarterProjects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<decimal>(
                name: "ConvertedPledgedAmount",
                table: "KickstarterProjects",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CountryDisplayableName",
                table: "KickstarterProjects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrencySymbol",
                table: "KickstarterProjects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CurrencyTrailingCode",
                table: "KickstarterProjects",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentCurrency",
                table: "KickstarterProjects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DisableCommunication",
                table: "KickstarterProjects",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FxRate",
                table: "KickstarterProjects",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisliked",
                table: "KickstarterProjects",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInPostCampaignPledgingPhase",
                table: "KickstarterProjects",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLaunched",
                table: "KickstarterProjects",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLiked",
                table: "KickstarterProjects",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStarrable",
                table: "KickstarterProjects",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentFunded",
                table: "KickstarterProjects",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "PrelaunchActivated",
                table: "KickstarterProjects",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "KickstarterProjects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "KickstarterProjects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Spotlight",
                table: "KickstarterProjects",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StaffPick",
                table: "KickstarterProjects",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StateChangedAt",
                table: "KickstarterProjects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "StaticUsdRate",
                table: "KickstarterProjects",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UsdExchangeRate",
                table: "KickstarterProjects",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UsdType",
                table: "KickstarterProjects",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Creators",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ParentName",
                table: "Categories",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConvertedPledgedAmount",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "CountryDisplayableName",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "CurrencySymbol",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "CurrencyTrailingCode",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "CurrentCurrency",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "DisableCommunication",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "FxRate",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "IsDisliked",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "IsInPostCampaignPledgingPhase",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "IsLaunched",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "IsLiked",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "IsStarrable",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "PercentFunded",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "PrelaunchActivated",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "Spotlight",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "StaffPick",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "StateChangedAt",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "StaticUsdRate",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "UsdExchangeRate",
                table: "KickstarterProjects");

            migrationBuilder.DropColumn(
                name: "UsdType",
                table: "KickstarterProjects");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Locations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Locations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Locations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayableName",
                table: "Locations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Locations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Urls",
                table: "KickstarterProjects",
                type: "jsonb",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "KickstarterProjects",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "KickstarterProjects",
                type: "jsonb",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "KickstarterProjects",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "KickstarterProjects",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "KickstarterProjects",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Blurb",
                table: "KickstarterProjects",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Creators",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ParentName",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
