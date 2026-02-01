using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortener.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClickAnalyticToLong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClickAnalytics_UrlMappings_UrlMappingId1",
                table: "ClickAnalytics");

            migrationBuilder.DropIndex(
                name: "IX_ClickAnalytics_UrlMappingId1",
                table: "ClickAnalytics");

            migrationBuilder.DropColumn(
                name: "UrlMappingId1",
                table: "ClickAnalytics");

            migrationBuilder.AlterColumn<long>(
                name: "UrlMappingId",
                table: "ClickAnalytics",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_ClickAnalytics_UrlMappingId",
                table: "ClickAnalytics",
                column: "UrlMappingId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClickAnalytics_UrlMappings_UrlMappingId",
                table: "ClickAnalytics",
                column: "UrlMappingId",
                principalTable: "UrlMappings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClickAnalytics_UrlMappings_UrlMappingId",
                table: "ClickAnalytics");

            migrationBuilder.DropIndex(
                name: "IX_ClickAnalytics_UrlMappingId",
                table: "ClickAnalytics");

            migrationBuilder.AlterColumn<int>(
                name: "UrlMappingId",
                table: "ClickAnalytics",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "UrlMappingId1",
                table: "ClickAnalytics",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_ClickAnalytics_UrlMappingId1",
                table: "ClickAnalytics",
                column: "UrlMappingId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ClickAnalytics_UrlMappings_UrlMappingId1",
                table: "ClickAnalytics",
                column: "UrlMappingId1",
                principalTable: "UrlMappings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
