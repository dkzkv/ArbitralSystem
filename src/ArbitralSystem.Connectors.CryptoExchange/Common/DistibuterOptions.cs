using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange.Common
{

    public enum BinanceOrderBookLimit
    {
        limit_5 = 5,
        limit_10 = 10,
        limit_20 = 20,
        limit_50 = 50,
        limit_100 = 100,
        limit_500 = 500,
        limit_1000 = 1000,
        limit_5000 = 5000
    }

    public class BinanceDistributerOptions : BaseDistributerOptions, IDistributerOptions
    {
        public Exchange Exchange => Exchange.Binance;
        public void SetLimit(BinanceOrderBookLimit bookLimit)
        {
            Limit = (int)bookLimit;
        }
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class BittrexDistributerOptions : BaseDistributerOptions, IDistributerOptions
    {
        public Exchange Exchange => Exchange.Bittrex;
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class CoinExDistributerOptions : BaseDistributerOptions, IDistributerOptions
    {
        public Exchange Exchange => Exchange.CoinEx;
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class DistributerOptions : BaseDistributerOptions , IDistributerOptions
    {
        public Exchange Exchange => Exchange.Undefined;
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }


    public abstract class BaseDistributerOptions 
    {
        public int? Frequency { get; set; }

        public int? Limit { get; protected set; }

        public int SilenceLimitInSeconds { get; set; } = 5 * 60;
    }
}
