
/*
   Market Info Storage Database Scripts
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

create table [dbo].[OrderbookPriceEntries](
    [Symbol] [varchar](16) NOT NULL,
    [Price] [decimal](19, 9) NOT NULL,
    [Quantity] [decimal](19, 9) NOT NULL,
    [Exchange] [tinyint] NOT NULL,
    [Direction] [tinyint] NOT NULL,
    [UtcCatchAt] [datetime2](7) NOT NULL
) on [PRIMARY]

go

CREATE CLUSTERED COLUMNSTORE INDEX [CCI-OrderbookPriceEntries] ON [dbo].[OrderbookPriceEntries] 
WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [PRIMARY]
go


CREATE CLUSTERED COLUMNSTORE INDEX [CCI-DistributerStates] ON [dbo].[DistributerStates] 
WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [PRIMARY]
go



-- # Views ----

drop view [dbo].[orderbook_timestamps_vw] 
go

create view [dbo].[orderbook_timestamps_vw] 
as
select 
    [Symbol]
    ,min([UtcCatchAt]) as first_orderbook_timestamp
    ,max([UtcCatchAt]) as last_orderbook_timestamp
    ,count(distinct(Exchange)) as exchanges_n
    ,count(*) as quotes_n
from [dbo].[OrderbookPriceEntries]
where [UtcCatchAt] > DATEADD(DAY, -30, GETDATE())
group by [Symbol]

go


drop view [dbo].[current_month_trades_stats] 
go

create view [dbo].[current_month_trades_stats] 
as
select 
    min([UtcCatchAt]) as start_time
    ,max([UtcCatchAt]) as end_time
    ,count(*) as N
from [dbo].[OrderbookPriceEntries]
group by YEAR([UtcCatchAt]), MONTH([UtcCatchAt]), DAY([UtcCatchAt])

go


drop view [dbo].[last_orderbooks_vw] 
go

create view [dbo].[last_orderbooks_vw] 
as
select 
    [Exchange]
    ,[Symbol]
    ,[Price]
    
    ,[Quantity] as Volume
    ,[Direction]
    
    ,[UtcCatchAt] as [Timestamp]
from [dbo].[OrderbookPriceEntries]
where [UtcCatchAt] > DATEADD(DAY, -7, GETDATE())

go


-- # Programming ----

create procedure [dbo].[get_orderbook_sp]
    @symbol varchar(16)
    ,@fromTime datetime
    ,@toTime datetime
as
select 
    [Exchange]
    ,[Symbol]
    ,[Price]

    ,[Quantity] as Volume
    ,[Direction]

    ,[UtcCatchAt] as [Timestamp]
from [dbo].[OrderbookPriceEntries]
where 
    [Symbol] = @symbol
    and [UtcCatchAt] >= @fromTime
    and [UtcCatchAt] <= @toTime
;
    
go  


create procedure [dbo].[get_bot_statuses_history_sp]
    @symbol varchar(16)
    ,@fromTime datetime
    ,@toTime datetime
as   
    select
      [Exchange]
      ,[Symbol]
      ,[CurrentStatus] as [Status]
      ,[UtcChangedAt] as [Timestamp]
    from [dbo].[DistributerStates]
    where 
        [Symbol] = @symbol
        and [UtcChangedAt] >= @fromTime
        and [UtcChangedAt] <= @toTime
;
    
go
