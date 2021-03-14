using System;

namespace ArbitralSystem.Connectors.Core.Models
{
    public interface IDistributorOrderBook : IOrderBook
    {
        int? ClientPairId { get; }
        DateTimeOffset CatchAt { get; }
    }
}