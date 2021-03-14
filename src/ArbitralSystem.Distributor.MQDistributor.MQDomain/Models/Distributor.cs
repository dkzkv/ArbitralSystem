using System;
using System.Collections.Generic;
using System.Linq;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Common;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Distributor.MQDistributor.MQDomain.Models
{
    public class Distributor
    {
        public Distributor(string name, DistributorType type, Exchange[] exchanges)
        {
            Id = Guid.NewGuid();
            Name = name;
            Type = type;
            CreatedAt = DateTimeOffset.Now;
            Status = Status.Created;
            if(!exchanges.Any())
                throw new ArgumentException("Exchanges should not be empty on distributor.");
            Exchanges = exchanges;
        }

        public Guid Id { get; }
        public string Name { get; }
        public DistributorType Type { get; }
        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset? ModifyAt { get; private set; }
        public IEnumerable<Exchange> Exchanges { get; }
        public Status Status { get; private set;}
        public Server Server { get; private set;}

        public Distributor UpdateStatus(Status status)
        {
            Status = status;
            ModifyAt = DateTimeOffset.Now;
            return this;
        }

        public Distributor SetServer(Server server)
        {
            Server = server;
            ModifyAt = DateTimeOffset.Now;
            return this;
        }
        
        //only for mapping from persistence layer
        public Distributor(Guid id, string name, DistributorType type, DateTimeOffset createdAt, DateTimeOffset? modifyAt, Status status, Server server, IEnumerable<Exchange> exchanges)
        {
            Id = id;
            Name = name;
            Type = type;
            CreatedAt = createdAt;
            ModifyAt = modifyAt;
            Status = status;
            Server = server;
            Exchanges = exchanges;
        }
    }
}