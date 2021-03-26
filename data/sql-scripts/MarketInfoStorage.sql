
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


CREATE USER dkazakov FOR LOGIN dkazakov WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember @rolename = N'db_owner', @membername = N'dkazakov'
go


-- # Tables ----

CREATE TABLE [dbo].[PairInfos](
	[Id] [int] NOT NULL,
	[ExchangePairName] [varchar](32) NOT NULL,
	[UnificatedPairName] [varchar](32) NOT NULL,
	[BaseCurrency] [varchar](16) NOT NULL,
	[QuoteCurrency] [varchar](16) NOT NULL,
	[UtcCreatedAt] [smalldatetime] NOT NULL,
	[UtcDelistedAt] [smalldatetime] NULL,
	[Exchange] [tinyint] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[PairInfos] ADD  CONSTRAINT [PK_PairInfos] PRIMARY KEY CLUSTERED 
([Id] ASC)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_PairInfos_ExchangePairName_Exchange_UtcDelistedAt] ON [dbo].[PairInfos]
(
	[ExchangePairName] ASC,
	[Exchange] ASC,
	[UtcDelistedAt] ASC
)
WHERE ([ExchangePairName] IS NOT NULL AND [UtcDelistedAt] IS NOT NULL)
WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO


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

ALTER TABLE OrderbookPriceEntries
ADD FOREIGN KEY (ClientPairId) REFERENCES PairInfos(Id);
go


CREATE TABLE [dbo].[DistributerStates](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClientPairId] [int] NOT NULL,
	[UtcChangedAt] [datetime2](7) NOT NULL,
	[PreviousStatus] [tinyint] NOT NULL,
	[CurrentStatus] [tinyint] NOT NULL
) ON [PRIMARY]
GO

CREATE CLUSTERED COLUMNSTORE INDEX [CCI-DistributerStates] ON [dbo].[DistributerStates] 
WITH (DROP_EXISTING = OFF, COMPRESSION_DELAY = 0, DATA_COMPRESSION = COLUMNSTORE) ON [PRIMARY]
go

ALTER TABLE DistributerStates
ADD FOREIGN KEY (ClientPairId) REFERENCES PairInfos(Id);
GO


-- # Views ----

drop view [dbo].[orderbook_timestamps_vw] 
go

create view [dbo].[orderbook_timestamps_vw] 
as
select 
    pi.UnificatedPairName as symbol
    ,min([UtcCatchAt]) as first_orderbook_timestamp
    ,max([UtcCatchAt]) as last_orderbook_timestamp
    ,count(distinct(Exchange)) as exchanges_n
    ,count(*) as quotes_n
from [dbo].[OrderbookPriceEntries] as o
inner join dbo.PairInfos as pi ON o.ClientPairId = pi.Id
where [UtcCatchAt] > DATEADD(DAY, -30, GETDATE())
group by pi.UnificatedPairName

go


drop view [dbo].[current_month_trades_stats] 
go

create view [dbo].[current_month_trades_stats] 
as
select 
	pi.UnificatedPairName
    ,min([UtcCatchAt]) as start_time
    ,max([UtcCatchAt]) as end_time
    ,count(*) as N
from [dbo].[OrderbookPriceEntries] as o
inner join dbo.PairInfos as pi ON o.ClientPairId = pi.Id
group by pi.UnificatedPairName, YEAR([UtcCatchAt]), MONTH([UtcCatchAt]), DAY([UtcCatchAt])

go



drop view [dbo].[last_orderbooks_vw] 
go

create view [dbo].[last_orderbooks_vw] 
as
select ClientPairId
      ,[Price]
      
      ,[Quantity] as Volume
      ,Orderside as [Direction]
      
      ,[UtcCatchAt] as [Timestamp]
from [dbo].[OrderbookPriceEntries]
where [UtcCatchAt] > DATEADD(DAY, -7, GETDATE())

go


select * from [dbo].[current_month_trades_stats] 



-- # Programming ----
drop procedure [dbo].[get_orderbook_sp]
go 

create procedure [dbo].[get_orderbook_sp]
    @symbol varchar(32) 
    ,@fromTime datetime
    ,@toTime datetime
as
select 
    UnificatedPairName as Symbol
	,Exchange
    ,[Price]

    ,[Quantity] as Volume
    ,CAST(OrderSide as int) as [Direction]

    ,[UtcCatchAt] as [Timestamp]
from [dbo].[OrderbookPriceEntries] as o
inner join dbo.PairInfos as pi on pi.Id = o.ClientPairId
where 
    UnificatedPairName = @symbol
    and [UtcCatchAt] >= @fromTime
    and [UtcCatchAt] <= @toTime
;
    
go  


drop procedure [dbo].[get_bot_statuses_history_sp]
go

create procedure [dbo].[get_bot_statuses_history_sp]
    @symbol varchar(32) 
    , @fromTime datetime
    , @toTime datetime
as   
    select 
      ds.[Id]
	  ,UnificatedPairName as Symbol
	  ,Exchange
      ,[CurrentStatus] as [Status]
      ,[UtcChangedAt] as [Timestamp]
    from [dbo].[DistributerStates] as ds
	inner join dbo.PairInfos as pi on pi.Id = ds.ClientPairId
    where 
        UnificatedPairName = @symbol
        and [UtcChangedAt] >= @fromTime
        and [UtcChangedAt] <= @toTime
;    
go
