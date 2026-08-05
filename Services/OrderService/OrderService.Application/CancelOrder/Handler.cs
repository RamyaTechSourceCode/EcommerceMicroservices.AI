using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Application
{
    
    public class CancelOrderHandler : IRequestHandler<CancelOrderCommand>
    {
        private readonly IOrderDbContext _db;

        public CancelOrderHandler(IOrderDbContext db)
        {
            _db = db;
        }

        public async Task Handle(CancelOrderCommand request, CancellationToken ct)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(x => x.Id == request.OrderId, ct);

            if (order == null)
                throw new Exception("Order not found");

            // prevent double processing (idempotency)
            if (order.Status == OrderStatus.Cancelled.ToString())
                return;

            order.Status = OrderStatus.Cancelled.ToString();

            await _db.SaveChangesAsync(ct);
        }
    }
}
