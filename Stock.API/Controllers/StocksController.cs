using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stock.API.Queries.GetStocks;

namespace Stock.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;

        public StocksController(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _queryDispatcher.Dispatch<GetStocksQuery, GetStocksQueryResult>(new GetStocksQuery());
            return Ok(result);
        }
    }
}
