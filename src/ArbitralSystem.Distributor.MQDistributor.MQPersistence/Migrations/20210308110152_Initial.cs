using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ArbitralSystem.Distributor.MQDistributor.MQPersistence.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "mqd");

            migrationBuilder.CreateTable(
                name: "Exchanges",
                schema: "mqd",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exchanges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                schema: "mqd",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    ServerType = table.Column<string>(unicode: false, nullable: false),
                    MaxWorkersCount = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    ModifyAt = table.Column<DateTimeOffset>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Distributors",
                schema: "mqd",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Type = table.Column<string>(unicode: false, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false),
                    ModifyAt = table.Column<DateTimeOffset>(nullable: true),
                    Status = table.Column<string>(unicode: false, nullable: false),
                    ServerId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distributors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Distributors_Servers_ServerId",
                        column: x => x.ServerId,
                        principalSchema: "mqd",
                        principalTable: "Servers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DistributorExchanges",
                schema: "mqd",
                columns: table => new
                {
                    DistributorId = table.Column<Guid>(nullable: false),
                    ExchangeId = table.Column<int>(nullable: false),
                    HeartBeat = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributorExchanges", x => new { x.DistributorId, x.ExchangeId });
                    table.ForeignKey(
                        name: "FK_DistributorExchanges_Distributors_DistributorId",
                        column: x => x.DistributorId,
                        principalSchema: "mqd",
                        principalTable: "Distributors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DistributorExchanges_Exchanges_ExchangeId",
                        column: x => x.ExchangeId,
                        principalSchema: "mqd",
                        principalTable: "Exchanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DistributorExchanges_ExchangeId",
                schema: "mqd",
                table: "DistributorExchanges",
                column: "ExchangeId");

            migrationBuilder.CreateIndex(
                name: "IX_Distributors_ServerId",
                schema: "mqd",
                table: "Distributors",
                column: "ServerId");

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_Name",
                schema: "mqd",
                table: "Exchanges",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistributorExchanges",
                schema: "mqd");

            migrationBuilder.DropTable(
                name: "Distributors",
                schema: "mqd");

            migrationBuilder.DropTable(
                name: "Exchanges",
                schema: "mqd");

            migrationBuilder.DropTable(
                name: "Servers",
                schema: "mqd");
        }
    }
}
