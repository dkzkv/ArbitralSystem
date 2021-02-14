using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArbitralSystem.Storage.MarketInfoStorageService.Persistence.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DistributerStates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "varchar(32)", nullable: false),
                    Exchange = table.Column<byte>(type: "tinyint", nullable: false),
                    UtcChangedAt = table.Column<DateTime>(nullable: false),
                    PreviousStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    CurrentStatus = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributerStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderbookPriceEntries",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "varchar(32)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(19,9)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(19,9)", nullable: false),
                    Exchange = table.Column<byte>(type: "tinyint", nullable: false),
                    OrderSide = table.Column<bool>(type: "bit", nullable: false),
                    UtcCatchAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistributerStates");

            migrationBuilder.DropTable(
                name: "OrderbookPriceEntries");
        }
    }
}
