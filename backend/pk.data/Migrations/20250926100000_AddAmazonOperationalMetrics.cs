using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace pk.data.Migrations
{
    /// <inheritdoc />
    public partial class AddAmazonOperationalMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AmazonOperationalSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceSnapshotId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmazonOperationalSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AmazonProductOperationalMetrics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OperationalSnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InventoryQuantity = table.Column<int>(type: "integer", nullable: true),
                    InventoryDays = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    UnitsSold7d = table.Column<int>(type: "integer", nullable: true),
                    IsStockout = table.Column<bool>(type: "boolean", nullable: true),
                    NegativeReviewCount = table.Column<int>(type: "integer", nullable: false),
                    LatestNegativeReviewAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LatestNegativeReviewExcerpt = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LatestNegativeReviewUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LatestPriceUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BuyBoxPrice = table.Column<decimal>(type: "numeric(12,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmazonProductOperationalMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AmazonProductOperationalMetrics_AmazonOperationalSnapshots_OperationalSnapshotId",
                        column: x => x.OperationalSnapshotId,
                        principalTable: "AmazonOperationalSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AmazonProductOperationalMetrics_AmazonProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AmazonProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AmazonOperationalSnapshots_CapturedAt",
                table: "AmazonOperationalSnapshots",
                column: "CapturedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AmazonProductOperationalMetrics_OperationalSnapshotId",
                table: "AmazonProductOperationalMetrics",
                column: "OperationalSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_AmazonProductOperationalMetrics_ProductId_CapturedAt",
                table: "AmazonProductOperationalMetrics",
                columns: new[] { "ProductId", "CapturedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmazonProductOperationalMetrics");

            migrationBuilder.DropTable(
                name: "AmazonOperationalSnapshots");
        }
    }
}
