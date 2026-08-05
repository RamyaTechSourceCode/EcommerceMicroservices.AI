using Confluent.Kafka;
using ECommerce.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECommerce.Messaging.Kafka
{
    public abstract class KafkaConsumerBase : BackgroundService
    {
        private readonly string _topic;
        private readonly string _groupId;


        protected KafkaConsumerBase(string topic, string groupId)
        {
            _topic = topic;
            _groupId = groupId;
        }

        protected abstract Task HandleMessage(string message);

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = _groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(_topic);

            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = consumer.Consume(stoppingToken);
                    await HandleMessage(result.Value);
                }
            }, stoppingToken);
        }
    }
}
