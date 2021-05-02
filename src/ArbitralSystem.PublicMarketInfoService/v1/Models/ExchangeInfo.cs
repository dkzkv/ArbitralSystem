namespace ArbitralSystem.PublicMarketInfoService.v1.Models
{
    /// <summary>
    /// Exchange info, this data is common for all system.
    /// </summary>
    public class ExchangeInfo
    {
        /// <summary>
        /// Exchange system id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Exchange name
        /// </summary>
        public string Name { get; set; }
    }
}