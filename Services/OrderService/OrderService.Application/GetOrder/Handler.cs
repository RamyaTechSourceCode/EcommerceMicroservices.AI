using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderService.Application.GetOrder
{
   public class GetOrderHandler : IRequestHandler<GetOrderQuery, OrderReadModel?>
{
    private readonly IDatabase _redis;
    private readonly IOrderDbContext _db;

    public GetOrderHandler(IConnectionMultiplexer redis, IOrderDbContext db)
    {
        _redis = redis.GetDatabase();
        _db = db;
    }

    public async Task<OrderReadModel?> Handle(GetOrderQuery request, CancellationToken ct)
    {
        var key = $"order:{request.Id}";

        // 1. Redis lookup
        var cached = await _redis.StringGetAsync(key);

        if (!cached.IsNullOrEmpty)
            return JsonSerializer.Deserialize<OrderReadModel>(cached!);

        // 2. DB fallback
        var order = await _db.Orders
            .Where(x => x.Id == request.Id)
            .Select(x => new OrderReadModel
            {
                OrderId = x.Id,
                TotalAmount = x.TotalAmount,
                CustomerId = x.CustomerId,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (order == null)
            return null;

        // 3. update cache
        await _redis.StringSetAsync(
            key,
            JsonSerializer.Serialize(order),
            TimeSpan.FromMinutes(30)
        );

        return order;
    }
}
}
