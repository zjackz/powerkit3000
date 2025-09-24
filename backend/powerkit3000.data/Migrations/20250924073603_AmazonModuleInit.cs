using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace powerkit3000.data.Migrations
{
    /// <inheritdoc />
    public partial class AmazonModuleInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AmazonCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AmazonCategoryId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParentCategoryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmazonCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AmazonCategories_AmazonCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "AmazonCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AmazonProducts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Brand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    ListingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmazonProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AmazonProducts_AmazonCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AmazonCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AmazonSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    BestsellerType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmazonSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AmazonSnapshots_AmazonCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AmazonCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AmazonProductDataPoints",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<string>(type: "character varying(10)", nullable: false),
                    SnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    Rating = table.Column<float>(type: "real", nullable: true),
                    ReviewsCount = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmazonProductDataPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AmazonProductDataPoints_AmazonProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AmazonProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AmazonProductDataPoints_AmazonSnapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "AmazonSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AmazonTrends",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<string>(type: "character varying(10)", nullable: false),
                    SnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    TrendType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmazonTrends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AmazonTrends_AmazonProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "AmazonProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AmazonTrends_AmazonSnapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "AmazonSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AmazonCategories_AmazonCategoryId",
                table: "AmazonCategories",
                column: "AmazonCategoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AmazonCategories_Name",
                table: "AmazonCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AmazonCategories_ParentCategoryId",
                table: "AmazonCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AmazonProductDataPoints_ProductId_CapturedAt",
                table: "AmazonProductDataPoints",
                columns: new[] { "ProductId", "CapturedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AmazonProductDataPoints_SnapshotId",
                table: "AmazonProductDataPoints",
                column: "SnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_AmazonProducts_CategoryId_Title",
                table: "AmazonProducts",
                columns: new[] { "CategoryId", "Title" });

            migrationBuilder.CreateIndex(
                name: "IX_AmazonSnapshots_CategoryId_BestsellerType_CapturedAt",
                table: "AmazonSnapshots",
                columns: new[] { "CategoryId", "BestsellerType", "CapturedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AmazonTrends_ProductId",
                table: "AmazonTrends",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AmazonTrends_SnapshotId_TrendType",
                table: "AmazonTrends",
                columns: new[] { "SnapshotId", "TrendType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmazonProductDataPoints");

            migrationBuilder.DropTable(
                name: "AmazonTrends");

            migrationBuilder.DropTable(
                name: "AmazonProducts");

            migrationBuilder.DropTable(
                name: "AmazonSnapshots");

            migrationBuilder.DropTable(
                name: "AmazonCategories");

        }
    }
}
