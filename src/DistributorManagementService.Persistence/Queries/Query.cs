using System;
using AutoMapper;

namespace DistributorManagementService.Persistence.Queries
{
    public abstract class Query
    {
        protected readonly DistributorDbContext DbContext;
        protected readonly IMapper Mapper;

        protected Query(DistributorDbContext dbContext, IMapper mapper)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
    }
}