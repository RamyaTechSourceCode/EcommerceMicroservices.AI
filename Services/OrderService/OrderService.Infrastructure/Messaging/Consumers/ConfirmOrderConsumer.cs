using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Application;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Messaging.Consumers
{
    public class ConfirmOrderConsumer : IConsumer<ConfirmOrderCommand>
    {
        private readonly IOrderDbContext _db;

        public ConfirmOrderConsumer(IOrderDbContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<ConfirmOrderCommand> context)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(x => x.Id == context.Message.OrderId, context.CancellationToken);

            if (order == null) return;

            order.Status = OrderStatus.Confirmed.ToString();

            await _db.SaveChangesAsync(context.CancellationToken);
        }
    }
}
