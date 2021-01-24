using Newtonsoft.Json;

namespace ArbitralSystem.Connectors.CoinEx.Models.Auxiliary
{
    internal class MarketInfo : IMarketInfo
    {
        [JsonProperty(PropertyName = "taker_fee_rate")]
        public decimal FreeRate { get; set; }

        [JsonProperty(PropertyName = "pricing_name")]
        public string PricingName { get; set; }

        [JsonProperty(PropertyName = "trading_name")]
        public string TradingName { get; set; }

        [JsonProperty(PropertyName = "min_amount")]
        public decimal MinAmount { get; set; }

        [JsonProperty(PropertyName = "name")] 
        public string Name { get; set; }

        [JsonProperty(PropertyName = "trading_decimal")]
        public int TradingPrecision { get; set; }

        [JsonProperty(PropertyName = "maker_fee_rate")]
        public decimal MakerFee { get; set; }

        [JsonProperty(PropertyName = "pricing_decimal")]
        public int PricingPrecision { get; set; }
    }
}