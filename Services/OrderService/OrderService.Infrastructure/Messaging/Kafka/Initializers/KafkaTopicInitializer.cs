using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

public class KafkaTopicInitializer : IHostedService
{
    private readonly IConfiguration _configuration;

    public KafkaTopicInitializer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Topic initializer started");

        var config = new AdminClientConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"]
        };

        using var admin = new AdminClientBuilder(config).Build();

        Console.WriteLine("Creating topic...");

        var metadata = admin.GetMetadata(TimeSpan.FromSeconds(5));

        bool exists = metadata.Topics.Any(t => t.Topic == "orders.created");

        if (!exists)
        {
            await admin.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = "orders.created",
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
            });

            Console.WriteLine("Topic created.");
        }
        else
        {
            Console.WriteLine("Topic already exists.");
        }

        Console.WriteLine("Topic creation completed");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}