using ECommerce.Contracts.Events;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using ECommerce.Contracts.Commands;
using InventoryService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ECommerce.Messaging.Kafka;
using InventoryService.Application;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using InventoryService.Domain.Entities;

namespace InventoryService.Infrastructure.Messaging.Consumers
{
 
    public class ReserveInventoryConsumer : IConsumer<ReserveInventoryCommand>
    {
        private readonly InventoryDbContext _db;
        private readonly ITopicProducer<InventoryReservedEvent> _producer;
        private readonly ITopicProducer<InventoryRejectedEvent> _rejectedProducer;

        public ReserveInventoryConsumer(InventoryDbContext db,
            ITopicProducer<InventoryReservedEvent> producer,
            ITopicProducer<InventoryRejectedEvent> rejectedProducer)
        {
            _db = db;
            _producer = producer;   
            _rejectedProducer = rejectedProducer;
        }
        public async Task Consume(ConsumeContext<ReserveInventoryCommand> context)
        {
            var message = context.Message;

            // 1. Idempotency check
            var alreadyProcessed = await _db.InventoryReservations
                .AnyAsync(x =>
                    x.CorrelationId == message.CorrelationId &&
                    x.ProductId == message.ProductId);

            if (alreadyProcessed)
            {

                return;
            }

            // 2. Get stock
            var stock = await _db.Inventories
                .FirstOrDefaultAsync(x => x.ProductId == message.ProductId);

            if (stock == null || stock.Quantity < message.StockQuantity)
            {

                await _rejectedProducer.Produce(new InventoryRejectedEvent
                {
                    CorrelationId = message.CorrelationId,
                    OrderId = message.OrderId,
                    ProductId = message.ProductId,
                    Reason = "Insufficient stock"
                });

                return;
            }

            // 3. Reserve stock
            stock.Quantity -= message.StockQuantity;

            // 4. Save reservation (idempotency tracking)
            _db.InventoryReservations.Add(new InventoryReservation
            {
                CorrelationId = message.CorrelationId,
                ProductId = message.ProductId,
                OrderId = message.OrderId,
                Quantity = message.StockQuantity,
                ProcessedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();


            // 5. Publish success event
            await _producer.Produce(new InventoryReservedEvent
            {
                CorrelationId = message.CorrelationId,
                OrderId = message.OrderId,
                ProductId = message.ProductId
            });
        }
    }
}
