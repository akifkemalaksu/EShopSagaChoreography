using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Dtos;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ICommandDispatcher _commandDispatcher;

        public OrdersController(ICommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateOrderDto createOrder,CancellationToken token)
        {
            var result = await _commandDispatcher.Dispatch<CreateOrderDto, CreateOrderResultDto>(createOrder, token);
            return Ok(result);
        }
    }
}
