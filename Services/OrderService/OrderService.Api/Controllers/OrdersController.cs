using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.CreateOrder;
using OrderService.Application.GetOrder;
using StackExchange.Redis;


[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConnectionMultiplexer _redis;

    public OrdersController(IMediator mediator, IConnectionMultiplexer redis)
    {
        _mediator = mediator;
        _redis = redis;
    }

    // POST /orders
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var orderId = await _mediator.Send(command);

        return Ok(new { OrderId = orderId });
    }

    // GET /orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var result = await _mediator.Send(new GetOrderQuery(id));

        return result is null ? NotFound() : Ok(result);

        /*var db = _redis.GetDatabase();

        var value = await db.StringGetAsync($"order:{id}");

        if (value.IsNullOrEmpty)
            return NotFound();

        return Content(value!, "application/json");*/
    }
}