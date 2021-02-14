using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArbitralSystem.PublicMarketInfoService.Persistence.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PairInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ExchangePairName = table.Column<string>(type: "varchar(32)", nullable: false),
                    UnificatedPairName = table.Column<string>(type: "varchar(32)", nullable: false),
                    BaseCurrency = table.Column<string>(type: "varchar(16)", nullable: false),
                    QuoteCurrency = table.Column<string>(type: "varchar(16)", nullable: false),
                    UtcCreatedAt = table.Column<DateTime>(type: "smalldatetime", nullable: false),
                    UtcDelistedAt = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    Exchange = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PairInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PairPrices",
                columns: table => new
                {
                    ExchangePairName = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(19,9)", nullable: true),
                    Exchange = table.Column<byte>(type: "tinyint", nullable: false),
                    UtcDate = table.Column<DateTime>(type: "smalldatetime", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_PairInfos_ExchangePairName_Exchange_UtcDelistedAt",
                table: "PairInfos",
                columns: new[] { "ExchangePairName", "Exchange", "UtcDelistedAt" },
                unique: true,
                filter: "[ExchangePairName] IS NOT NULL AND [UtcDelistedAt] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PairInfos");

            migrationBuilder.DropTable(
                name: "PairPrices");
        }
    }
}
