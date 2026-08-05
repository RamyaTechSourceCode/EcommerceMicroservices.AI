using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Application;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Messaging.Consumers
{
    public class CancelOrderConsumer : IConsumer<CancelOrderCommand>
    {
        private readonly IOrderDbContext _db;

        public CancelOrderConsumer(IOrderDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<CancelOrderCommand> context)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(x => x.Id == context.Message.OrderId, context.CancellationToken);

            if (order == null) return;

            order.Status = OrderStatus.Cancelled.ToString();

            await _db.SaveChangesAsync(context.CancellationToken);
        }
    }
}
