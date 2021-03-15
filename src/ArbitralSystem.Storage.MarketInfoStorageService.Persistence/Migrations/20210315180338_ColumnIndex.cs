using Microsoft.EntityFrameworkCore.Migrations;

namespace ArbitralSystem.Storage.MarketInfoStorageService.Persistence.Migrations
{
    public partial class ColumnIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                sql: "CREATE CLUSTERED COLUMNSTORE INDEX [CCI-OrderbookPriceEntries] ON [dbo].[OrderbookPriceEntries] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [PRIMARY]",
                suppressTransaction: true);
            
            migrationBuilder.Sql(
                sql: "CREATE CLUSTERED COLUMNSTORE INDEX [CCI-DistributerStates] ON [dbo].[DistributerStates] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [PRIMARY]",
                suppressTransaction: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX [CCI-OrderbookPriceEntries] ON [dbo].[OrderbookPriceEntries]");
            migrationBuilder.Sql("DROP INDEX [CCI-DistributerStates] ON [dbo].[DistributerStates]");
        }
    }
}
