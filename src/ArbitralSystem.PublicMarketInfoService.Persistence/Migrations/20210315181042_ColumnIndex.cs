using Microsoft.EntityFrameworkCore.Migrations;

namespace ArbitralSystem.PublicMarketInfoService.Persistence.Migrations
{
    public partial class ColumnIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                sql: "CREATE CLUSTERED COLUMNSTORE INDEX [CCI-PairPrices] ON [dbo].[PairPrices] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [PRIMARY]",
                suppressTransaction: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX [CCI-PairPrices] ON [dbo].[PairPrices]");
        }
    }
}
