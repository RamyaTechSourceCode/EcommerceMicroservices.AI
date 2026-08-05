using Confluent.Kafka;
using ECommerce.Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECommerce.Messaging.Kafka
{
    public class KafkaEventBus : IEventBus
    {
        private readonly IProducer<string, string> _producer;
        private readonly JsonSerializerOptions _options;

        public KafkaEventBus()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:9092"
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            _options = new JsonSerializerOptions();
        }

        public async Task PublishAsync<T>(string topic, T @event)
        {
            var message = JsonSerializer.Serialize(@event, _options);

            await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = message
            });
        }
    }
}
