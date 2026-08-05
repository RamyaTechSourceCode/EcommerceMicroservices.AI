using ECommerce.Contracts.Events;
using ECommerce.Messaging.Kafka;
using InventoryService.Application;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InventoryService.Infrastructure.Messaging.Consumers
{
    using InventoryService.Infrastructure.Persistence;
    using MassTransit;
    using MassTransit.Transports;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using StackExchange.Redis;
    using static Confluent.Kafka.ConfigPropertyNames;

    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        public Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            Console.WriteLine($"Consumer received {context.Message.OrderId}");
            return Task.CompletedTask;
        }
        /*   private readonly InventoryDbContext _db;
           private readonly ITopicProducer<InventoryReservedEvent> _producer;
           private readonly ITopicProducer<InventoryRejectedEvent> _rejectedProducer;

           public OrderCreatedConsumer(InventoryDbContext db, 
               ITopicProducer<InventoryReservedEvent> producer,
               ITopicProducer<InventoryRejectedEvent> rejectedProducer)
           {
               _db = db;
               _producer = producer;
               _rejectedProducer = rejectedProducer;
           }

           public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
           {
               var order = context.Message;

               var canReserve = true;

               foreach (var item in order.Items)
               {
                   var stock = await _db.Inventories
                       .FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

                   if (stock == null || stock.Quantity < item.Quantity)
                   {
                       canReserve = false;
                       break;
                   }
               }

               if (!canReserve)
               {
                  /* await context.Publish(new InventoryRejectedEvent
                   {
                       OrderId = order.OrderId,
                       Reason = "Insufficient stock"
                   });
                   await _rejectedProducer.Produce(new InventoryRejectedEvent
                   {
                       OrderId = order.OrderId,
                       Reason = "Insufficient stock"
                   });
                   return;
               }

               foreach (var item in order.Items)
               {
                   var stock = await _db.Inventories
                       .FirstAsync(x => x.ProductId == item.ProductId);

                   stock.Quantity -= item.Quantity;
               }

               await _db.SaveChangesAsync();

               /*  await context.Publish(new InventoryReservedEvent
                 {
                     OrderId = order.OrderId
                 });

               await _producer.Produce(new InventoryReservedEvent
               {
                   OrderId = order.OrderId,
                   ReservedAt =  DateTime.UtcNow
               });

           }*/
    }
}
