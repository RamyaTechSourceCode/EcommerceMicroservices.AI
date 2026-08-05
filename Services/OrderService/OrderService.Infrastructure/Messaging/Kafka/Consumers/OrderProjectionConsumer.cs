using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OrderService.Application.GetOrder;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Messaging.Kafka.Consumers
{
    public class OrderProjectionConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;

        public OrderProjectionConsumer(
            IConfiguration configuration,
            IConnectionMultiplexer redis)
        {
            _configuration = configuration;
            _redis = redis;
        }

        protected override Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = _configuration["Kafka:BootstrapServers"],
                    GroupId = "order-projection-group",
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };

                Console.WriteLine(_configuration["Kafka:BootstrapServers"]);

                using var consumer =
                    new ConsumerBuilder<string, string>(config).Build();

                consumer.Subscribe("orders.created");

                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var result = consumer.Consume(stoppingToken);
                        Console.WriteLine($"Consumed: {result.Message.Value}");

                        _ = ProcessMessageAsync(result.Message.Value);
                    }
                }
                catch (OperationCanceledException)
                {
                    consumer.Close();
                }
            }, stoppingToken);
        }
        private async Task ProcessMessageAsync(string message)
        {
            var db = _redis.GetDatabase();

            var readModel = JsonSerializer.Deserialize<OrderReadModel>(message);

            if (readModel == null) return;

            await db.StringSetAsync(
                $"order:{readModel.OrderId}",
                JsonSerializer.Serialize(readModel)
            );
        }
    }
}

