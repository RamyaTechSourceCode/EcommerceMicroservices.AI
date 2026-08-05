using ECommerce.Contracts.Events;
using ECommerce.Messaging.Abstractions;
using MassTransit;
using MassTransit.Transports;
using MediatR;
using OrderService.Domain.Entities;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MassTransit.Logging.OperationName;

namespace OrderService.Application.CreateOrder
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IOrderDbContext _db;
        private readonly IEventBus _bus;
        private readonly IPublishEndpoint _publishEndpoint;
        //private readonly OrderCreatedProducer _producer;

        public CreateOrderHandler(
            IOrderDbContext db, IEventBus bus, IPublishEndpoint publishEndpoint)
            //OrderCreatedProducer producer)
        {
            _db = db;
            _bus = bus;
            _publishEndpoint = publishEndpoint;
            //_producer = producer;
        }

        public async Task<Guid> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken)
        {
            var order = new Domain.Entities.Order
            {
                Id = Guid.NewGuid(),
                CorrelationId =Guid.NewGuid(),
                CustomerId = request.CustomerId,
                Items = request.Items.Select(x => new OrderItem
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    Price = x.Price
                }).ToList(),
                TotalAmount = request.Items.Sum(x => x.Price * x.Quantity)
            };

            _db.Orders.Add(order);

            await _db.SaveChangesAsync(cancellationToken);

            var sagaId = NewId.NextGuid();

            Console.Write("Publishing OrderCreatedEvent");

            await _publishEndpoint.Publish(new OrderCreatedEvent
            {
                CorrelationId = sagaId,
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                Items = request.Items.Select(x => new OrderItemEvent
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    Price = x.Price
                }).ToList()
            });
            /* await _bus.PublishAsync("order.created",
                 new OrderCreatedEvent
                 {
                     CorrelationId = order.CorrelationId,
                     OrderId = order.Id,
                     CustomerId = order.CustomerId,
                     TotalAmount = order.TotalAmount,
                     CreatedAt = order.CreatedAt,
                     Items = request.Items.Select(x => new OrderItemEvent
                     {
                         ProductId = x.ProductId,
                         Quantity = x.Quantity,
                         Price = x.Price
                     }).ToList()
                 });*/

            /* var orderCreated = new OrderCreatedEvent
             {
                 OrderId = order.Id,
                 CustomerId = order.CustomerId,
                 TotalAmount = order.TotalAmount,
                 CreatedAt = order.CreatedAt,
                 Items = order.Items.Select(i => new OrderItemEvent
                 {
                     ProductId = i.ProductId,
                     Quantity = i.Quantity
                 }).ToList()
             };

             await _producer.PublishAsync(
                 "orders.created",
                 order.Id.ToString(),
                 orderCreated);*/

            return order.Id;
        }
    }
}
