using System;
using System.Collections.Generic;

namespace ArbitralSystem.Distributor.MQDistributor.MQDomain.Common
{
    public enum Status
    {
        Created,
        Processing,
        OnDeleting,
        Deleted
    }
    
    public static class StatusHelper
    {
        public static IEnumerable<Status> GetAll()
        {
            var allStatuses = new List<Status>();
            foreach (Status status in Enum.GetValues(typeof(Status)))
            {
                allStatuses.Add(status);
            }
            return allStatuses;
        }
    }
}