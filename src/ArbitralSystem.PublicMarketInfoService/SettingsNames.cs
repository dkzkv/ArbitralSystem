namespace ArbitralSystem.PublicMarketInfoService
{
    internal static class SettingsNames
    {
        public static string DatabaseConnection = "Settings:DataBaseConnection";
        public static string AvailableExchanges  = "Settings:AvailableExchanges";
        public static string PairInfosCron = "Settings:JobCronExpressions:PairInfos";
        public static string PairPricesCron = "Settings:JobCronExpressions:PairPrices";
        public static string CoineExSection = "Connectors:CoinEx";
    }
}