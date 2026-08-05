using ECommerce.Messaging.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application
{
    public class ConfirmOrderHandler : IRequestHandler<ConfirmOrderCommand>
    {
        private readonly IOrderDbContext _db;

        public ConfirmOrderHandler(IOrderDbContext db)
        {
            _db = db;
        }

        public async Task Handle(ConfirmOrderCommand request, CancellationToken ct)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(x => x.Id == request.OrderId, ct);

            if (order == null) return;


            order.Status = OrderStatus.Confirmed.ToString();

            await _db.SaveChangesAsync(ct);
        }
    }
}
