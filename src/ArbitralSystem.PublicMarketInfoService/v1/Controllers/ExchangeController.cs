using System.Collections.Generic;
using System.Linq;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.PublicMarketInfoService.v1.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ArbitralSystem.PublicMarketInfoService.v1.Controllers
{
    /// <inheritdoc />
    [Route("api/v1/exchange")]
    [ApiController]
    public class ExchangeController : ControllerBase
    {
        private readonly IMapper _mapper;
        /// <inheritdoc />
        public ExchangeController(IMapper mapper)
        {
            _mapper = mapper;
        }
        
        /// <summary>
        /// Get exchange lookup
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(ExchangeInfo), 200)]
        public IActionResult Get([FromQuery] QueryFilter filter)
        {
            var exp = ExchangeHelper.GetAll()
                .Where(o => o != Exchange.Undefined);
            if (!string.IsNullOrEmpty(filter.Query))
                exp = exp.Where(o => o.ToString().ToLower().Contains(filter.Query.ToLower()));
            var exchangeInfos =  _mapper.Map<IEnumerable<ExchangeInfo>>(exp);
            return Ok(exchangeInfos);
        }
    }
}