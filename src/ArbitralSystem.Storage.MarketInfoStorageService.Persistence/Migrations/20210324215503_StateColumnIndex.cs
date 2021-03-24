using Microsoft.EntityFrameworkCore.Migrations;

namespace ArbitralSystem.Storage.MarketInfoStorageService.Persistence.Migrations
{
    public partial class StateColumnIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(" ALTER TABLE [dbo].[DistributerStates] DROP CONSTRAINT [PK_DistributerStates];");
            
            migrationBuilder.Sql("CREATE CLUSTERED COLUMNSTORE INDEX [CCI-DistributerStates] ON [dbo].[DistributerStates] WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [PRIMARY]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
