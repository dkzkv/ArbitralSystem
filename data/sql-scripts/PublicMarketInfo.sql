/*
   Market Public Info Storage Database Scripts
*/


-- # Global settings ----

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- # Security ----

CREATE ROLE [db_executor] AUTHORIZATION [dbo]
GO

GRANT EXECUTE TO [db_executor]
GO

CREATE USER [mlbot] FOR LOGIN [mlbot] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember @rolename = N'db_datareader', @membername = N'mlbot'
EXEC sp_addrolemember @rolename = N'db_executor', @membername = N'mlbot'
go



-- # Tables ----

CREATE CLUSTERED COLUMNSTORE INDEX [CCI-PairPrices] ON [dbo].[PairPrices] 
WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [PRIMARY]
go



-- # Programming ----

create procedure [dbo].[get_symbol_price_sp]
    @symbol varchar(16)
    , @fromTime datetime
    , @toTime datetime
as   
    select 
      [ExchangePairName] as Symbol
      ,[Price]
      ,[Exchange]
      ,[UtcDate] as [Timestamp]
    from [dbo].[PairPrices]
    where 
        [ExchangePairName] = @symbol
        and [UtcDate] >= @fromTime
        and [UtcDate] <= @toTime
;

go