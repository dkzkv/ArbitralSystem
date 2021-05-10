using System.Collections.Generic;
using System.Linq;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Common;
using ArbitralSystem.Distributor.MQDistributor.MQManagerService.v1.Models;
using ArbitralSystem.Distributor.MQDistributor.MQManagerService.v1.Models.LookUps;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ArbitralSystem.Distributor.MQDistributor.MQManagerService.v1.Controllers
{
    /// <inheritdoc />
    [Route("api/v1/distributor")]
    [ApiController]
    public class DistributorController : ControllerBase
    {
        private readonly IMapper _mapper;
        /// <inheritdoc />
        public DistributorController(IMapper mapper)
        {
            _mapper = mapper;
        }
        
        /// <summary>
        /// Get distributor statuses
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("statuses")]
        public IActionResult GetStatuses([FromQuery] QueryFilter filter)
        {
            var exp = StatusHelper.GetAll();
            if (!string.IsNullOrEmpty(filter.Query))
                exp = exp.Where(o => o.ToString().ToLower().Contains(filter.Query.ToLower()));
            var statuses = _mapper.Map<IEnumerable<StatusInfo>>(exp);
            return Ok(statuses);
        }
    }
}