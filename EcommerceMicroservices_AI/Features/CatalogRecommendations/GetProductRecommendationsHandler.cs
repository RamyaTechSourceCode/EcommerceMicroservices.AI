using MediatR;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using System.Linq;

namespace EcommerceMicroservices.Ai.Features.CatalogRecommendations;

public class GetProductRecommendationsHandler : IRequestHandler<GetProductRecommendationsQuery, RecommendationResponse>
{
    private readonly QdrantClient _qdrantClient;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

    private readonly IChatCompletionService _chatService;

    public GetProductRecommendationsHandler(
        QdrantClient qdrantClient,
        Kernel kernel)
    {
        _qdrantClient = qdrantClient;
        _embeddingGenerator = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<RecommendationResponse> Handle(GetProductRecommendationsQuery request, CancellationToken cancellationToken)
    {
        // Step 1: Generate vector representation from raw text query
        // Wrap request.UserPrompt into a collection since GenerateAsync expects a list of inputs
        var embeddingResult = await _embeddingGenerator.GenerateAsync(
            new[] { request.UserPrompt },
            cancellationToken: cancellationToken
        );

        // Extract the raw ReadOnlyMemory<float> vector from the first result item
        ReadOnlyMemory<float> singleVector = embeddingResult.First().Vector;

        // Step 2: Fetch closest contextual semantic items out of Qdrant cluster database
        var searchResult = await _qdrantClient.SearchAsync(
            collectionName: "products",
            vector: singleVector.Span.ToArray(), // FIX: Convert the extracted vector to an array using .Span
            limit: (ulong)request.Limit,
            cancellationToken: cancellationToken
        );

        var matchedProducts = new List<ProductDto>();
        var contextItemsText = new List<string>();

        foreach (var point in searchResult)
        {
            var payload = point.Payload;
            var product = new ProductDto(
                Guid.Parse(payload["id"].StringValue),
                payload["name"].StringValue,
                payload["description"].StringValue,
                payload["price"].DoubleValue
            );
            matchedProducts.Add(product);
            contextItemsText.Add($"ID: {product.Id}, Product: {product.Name}, Details: {product.Description}, Cost: ${product.Price}");
        }

        // Step 3: Run generative completion loop using enriched data points
        var chatHistory = new ChatHistory("You are a helpful e-commerce system catalog recommendation engine assistant.");
        string promptContext = string.Join("\n", contextItemsText);

        chatHistory.AddUserMessage(
            $"The shopper is looking for: '{request.UserPrompt}'.\n" +
            $"Here are the matching catalog choices:\n{promptContext}\n" +
            "Provide a cohesive 2-3 sentence overview explaining why these match best."
        );

        var llmResponse = await _chatService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

        return new RecommendationResponse(llmResponse.Content ?? "No recommendation summary could be generated.", matchedProducts);
    }
}
