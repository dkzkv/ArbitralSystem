using System;
using System.Collections.Generic;

namespace ArbitralSystem.Distributor.MQDistributor.MQDomain.Common
{
    public enum ServerType
    {
        OrderBookDistributor,
    }
    
    public static class ServerTypeHelper
    {
        public static IEnumerable<ServerType> GetAll()
        {
            var allServerTypes = new List<ServerType>();
            foreach (ServerType exchange in Enum.GetValues(typeof(ServerType)))
            {
                allServerTypes.Add(exchange);
            }
            return allServerTypes;
        }
    }
}