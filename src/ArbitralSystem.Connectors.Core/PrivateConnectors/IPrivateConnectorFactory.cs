using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.PrivateConnectors
{
    public interface IAccountConnectorFactory
    {
        IExtraConnector GetInstance(Exchange exchange);
    }

    public interface IPrivateConnectorFactory
    {
        IPrivateConnector GetInstance(Exchange exchange);
    }
}