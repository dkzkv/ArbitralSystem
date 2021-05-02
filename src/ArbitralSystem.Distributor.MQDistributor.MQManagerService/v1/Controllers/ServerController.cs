using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Common;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Queries.Server;
using ArbitralSystem.Distributor.MQDistributor.MQManagerService.v1.Models;
using ArbitralSystem.Distributor.MQDistributor.MQManagerService.v1.Models.LookUps;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ArbitralSystem.Distributor.MQDistributor.MQManagerService.v1.Controllers
{
    /// <inheritdoc />
    [Route("api/v1/server")]
    [ApiController]
    public class ServerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        /// <inheritdoc />
        public ServerController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }
        
        /// <summary>
        /// Get paginated servers
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ServerFilter filter, CancellationToken cancellationToken)
        {
            var query = new ServerQuery(_mapper.Map<MQDomain.Queries.QueryModels.ServerFilter>(filter));
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(_mapper.Map<Models.Paging.Page<ShortServerResult>>(result));
        }
        
        
        /// <summary>
        /// Get server types
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("types")]
        public IActionResult GetTypes([FromQuery] QueryFilter filter)
        {
            var exp = ServerTypeHelper.GetAll();
            if (!string.IsNullOrEmpty(filter.Query))
                exp = exp.Where(o => o.ToString().ToLower().Contains(filter.Query.ToLower()));
            var types =  _mapper.Map<IEnumerable<ServerTypeInfo>>(exp);
            return Ok(types);
        }
        
        /// <summary>
        /// Get server types
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("statuses")]
        public IActionResult GetStatuses([FromQuery] QueryFilter filter)
        {
            var exp = StatusHelper.GetAll();
            if (!string.IsNullOrEmpty(filter.Query))
                exp = exp.Where(o => o.ToString().ToLower().Contains(filter.Query.ToLower()));
            var statuses =  _mapper.Map<IEnumerable<StatusInfo>>(exp);
            return Ok(statuses);
        }
        
        /// <summary>
        /// Full server info result
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isDeletedDistributors"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] Guid id,[FromQuery] bool? isDeletedDistributors , CancellationToken cancellationToken)
        {
            var query = new ServerQueryById(id, isDeletedDistributors);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(_mapper.Map<FullServerResult>(result));
        }
    }
}