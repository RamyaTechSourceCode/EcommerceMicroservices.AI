using Confluent.Kafka;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Text.Json;


namespace EcommerceMicroservices.Ai.IntegrationEvents;

public class ProductUpdatedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly QdrantClient _qdrantClient;

    public ProductUpdatedConsumer(IServiceProvider serviceProvider, QdrantClient qdrantClient)
    {
        _serviceProvider = serviceProvider;
        _qdrantClient = qdrantClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig { BootstrapServers = "localhost:9092", GroupId = "ecommercemicroservices-ai-group", AutoOffsetReset = AutoOffsetReset.Earliest };
        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("product.updated");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);
                var eventData = JsonSerializer.Deserialize<ProductUpdatedPayload>(consumeResult.Message.Value);

                if (eventData != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var kernel = scope.ServiceProvider.GetRequiredService<Kernel>();

                    // ◄ 2. FIX: Resolve the unified ecosystem generator interface instead of the obsolete SK service
                    var embeddingService = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

                    string denseStringContext = $"{eventData.Name} {eventData.Description}";

                    // ◄ 3. FIX: Update method call signature to match the new asynchronous generation engine
                    var embeddingResult = await embeddingService.GenerateAsync(new[] { denseStringContext }, cancellationToken: stoppingToken);
                    var vectorResult = embeddingResult.First().Vector.ToArray();

                    var point = new PointStruct
                    {
                        Id = (ulong)eventData.Id.GetHashCode(),
                        Vectors = vectorResult // Plural property fixed
                    };

                    point.Payload.Add("id", eventData.Id.ToString());
                    point.Payload.Add("name", eventData.Name);
                    point.Payload.Add("description", eventData.Description);
                    point.Payload.Add("price", eventData.Price);

                    await _qdrantClient.UpsertAsync("products", new[] { point }, cancellationToken: stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing updated stream layer: {ex.Message}");
            }
        }
    }

    private record ProductUpdatedPayload(Guid Id, string Name, string Description, double Price);
}
