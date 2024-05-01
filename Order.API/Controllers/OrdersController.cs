using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Commands.CreateOrder;

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
        public async Task<IActionResult> Post(CreateOrderCommand createOrder,CancellationToken token)
        {
            var result = await _commandDispatcher.Dispatch<CreateOrderCommand, CreateOrderCommandResult>(createOrder, token);
            return Ok(result);
        }
    }
}
